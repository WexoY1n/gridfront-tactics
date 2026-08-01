# Architecture

## Goal

把战斗内核当成一台可复现的状态机：玩家输入变成 Command；固定 Tick 的 Systems 依次修改 `BattleState` 并产生 Event；Unity 只负责表现；服务端用相同 Command、Seed 与配置复盘校验。

## Layers

| Module | Responsibility |
|---|---|
| `Battle.Core` | Tick、实体、寻路、索敌、阻挡、攻击、技能、Checksum |
| `Battle.Contracts` | Command / DTO / Schema / 版本 |
| `Client.Application` | 输入转 Command、驱动核心、同步 View |
| `Client.Presentation` | 地图、动画、UI、Debug Overlay |
| `Client.Infrastructure` | HTTP、JSON、本地缓存 |
| `Server.Api` | Auth、Manifest、Run 生命周期 |
| `Server.Validation` | 无头复盘与结果校验 |
| `Server.Persistence` | EF Core + PostgreSQL |

## Dependency rules

- `Battle.Core` 不引用 `UnityEngine`、HTTP、数据库、真实时间
- Presentation 不直接修改核心实体，只提交 Command
- Server 可引用 Core 与 Contracts，不可引用 Unity 客户端

## Tick pipeline

建议 `20 ticks/second`（50ms/tick）。

1. 读取本 Tick 指令  
2. 生成与费用  
3. 移动与路径进度  
4. 阻挡分配与释放  
5. 索敌与技能  
6. 攻击、伤害、治疗  
7. 死亡清理与胜负  
8. 领域事件 + Checksum  
