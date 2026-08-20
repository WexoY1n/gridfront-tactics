# Combat Rules

可测试规则摘要。实现与测试冲突时以测试锁定的行为为准，并回写本文。  
归档来源：[planning/project-plan-v1.md](planning/project-plan-v1.md) §3。

## Battle loop

1. 服务端创建 Run（`runId` / `seed` / `contentVersion` / `configHash`）  
2. 客户端加载关卡与编队，进入准备阶段  
3. 敌人按波次生成并沿路线移动  
4. 玩家消耗费用部署角色并选择朝向  
5. 角色按索敌规则攻击；近战可建立阻挡  
6. 玩家释放技能、撤退、再部署  
7. 敌人清空则胜；基地生命归零则负  
8. 上传 Command Log + Checksum，服务端复盘返回 `verified`  

## Map and coordinates

- 整数网格 `GridPos(x, y)`  
- 格类型：不可部署、近战位、高台位、出生点、目标点、可行走地面、特殊机关  
- 核心连续位置使用千分之一格整数（`1 tile = 1000 units`）  
- 表现层可插值，不得把 Unity 浮点写回核心状态  
- 射程用相对格坐标集合，不用圆形 Collider 作为规则真相  

## Pathfinding

### P0 — predefined route + A\*

关卡保存起点、终点、必经点与可行走网格；加载时分段 A\* 拼接并缓存。移动沿缓存路径推进，不每帧寻路。

| Term | Rule |
|---|---|
| `g` | 已移动成本；直邻默认 10 |
| `h` | 曼哈顿距离 × 10 |
| `f` | `g + h` |
| Tie-break | 同 `f` 时按更小 `h`，再按更小网格索引 `y * width + x` |
| Diagonal | 禁止 |
| Neighbor order | `+X`, `-X`, `+Y`, `-Y` |
| No path | `Found == false` 且节点列表为空；不返回局部假路径 |

`routeProgress` 使用路径折线的整数 milli-units（`1 tile = 1000`）。过拐点时仍单调增加；越出路径长度视为错误。

### P1 — dynamic replan

仅当 `navVersion` 变化时重规划。缓存键：`(start, goal, movementType, navVersion)`。  
普通部署不改导航版本；特殊路障才递增。无路进入 `WaitingForPath`，有限重试后按关卡规则处理。

### Route progress

```text
routeProgress = 已走完路径段长度 + 当前段内归一化距离
```

「离目标最近」按 `routeProgress` 降序，不用世界坐标直线距离。

## Operator targeting

1. 空间收集候选  
2. 合法性过滤（存活、阵营、地面/飞行、隐匿、可选中、射程）  
3. 优先级评分（默认更大 `routeProgress`，可叠加嘲讽等）  
4. 稳定决胜  

Default sort key:

```text
(-tauntLevel, -routeProgress, distanceSquared, spawnSequence, entityId)
```

目标仍合法时保持锁定；失效后再搜索。  
决胜距离为操作员所在格到候选格的 `dx² + dy²`。

## Facing and range

朝向 `N/E/S/W`。相对格以朝北为作者空间，旋转后再加上部署格：

| Facing | `(x, y)` |
|---|---|
| North | `(x, y)` |
| East | `(y, -x)` |
| South | `(-x, -y)` |
| West | `(-y, x)` |

地图外的相对格不进入射程。

## Deploy

近战槽只能放 `MeleePad`；高台槽只能放 `HighPad`。  
拒绝原因：`OutOfBounds` / `Occupied` / `WrongTile` / `InsufficientCost`。合法部署才扣费。

## Enemy targeting

- 被阻挡的近战：只打 `blockedByOperatorId`  
- 未被阻挡的远程：嘲讽 → 距离 → 部署顺序 → `EntityId`  
- 治疗/支援走独立 `TargetPolicy`，避免巨型 `if/else`  

## Blocking

领域系统，不依赖 `OnTriggerEnter` 作为真相。

```csharp
OperatorState.BlockCapacity
OperatorState.BlockedEnemyIds
EnemyState.BlockedByOperatorId
EnemyState.IsBlockable
```

建立条件：可阻挡、未被占、进入阻挡接触区、角色可阻挡、容量未满。  
必须原子写入双向关系并发出 `EnemyBlockedEvent`。  
解除：死亡、撤退、失去阻挡能力、变为不可阻挡、关卡结束；必须清双向引用。

不变量：

- 一个敌人最多被一个角色阻挡  
- `BlockedEnemyIds.Count <= BlockCapacity`  
- 双方引用一致  
- 死亡/撤退实体不保留阻挡或目标  

## Attack and damage

```text
Idle → AcquireTarget → Windup → HitFrame → Recovery → Idle
```

- `WindupTicks` 到点生成一次伤害事件；Windup 中目标失效则取消、不 Hit  
- 物理 P0：`max(minDamage, attack - defense)`  
- 法术 P0：`attack × (1 - resistance)`，统一整数舍入  
- 死亡在 Damage 阶段收集，Cleanup 阶段统一处理  

## Skills and buffs (P0)

数据驱动：手动/自动触发；时间/攻击/受击充能；瞬时或持续 Tick；改属性、目标数、治疗、单次伤害。

Modifier:

```text
最终值 = ((基础值 + FlatAdd) × PercentAdd) × FinalMultiplier
```

P0 只做少量通用 Effect，避免脚本类爆炸。

## Enemy state machine (summary)

`Spawning → Moving ⇄ Blocked/Attacking → ReachedGoal | Dead`
