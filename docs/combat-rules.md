# Combat Rules（草案）

详细规则以策划案为准；本文件随实现同步更新。

## Pathfinding

- 整数网格，禁止斜向
- A\*：直邻代价 10，启发为曼哈顿 × 10
- 同 `f` 时按 `h`、网格索引稳定决胜
- 路径缓存；仅 `navVersion` 变化时重规划（P1）

## Targeting

默认排序键：

```text
(-tauntLevel, -routeProgress, distanceSquared, spawnSequence, entityId)
```

## Blocking

- `BlockCapacity` / `BlockedEnemyIds` / `BlockedByOperatorId`
- 建立与解除必须双向一致
- 满容量时额外敌人继续移动

## Attack timeline

```text
Idle → AcquireTarget → Windup → HitFrame → Recovery → Idle
```

核心结果不依赖 Animator 事件。
