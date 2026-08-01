# Testing Strategy

归档来源：[planning/project-plan-v1.md](planning/project-plan-v1.md) §9。

## Priority order

1. **Battle.Core 纯 C# 单元测试**（最快、最重要）  
2. Server API / 复盘集成测试  
3. Unity Edit Mode（配置导出、映射）  
4. Unity Play Mode（交互与表现，少量）  
5. 性能基准（有数字再写进 README）  

核心规则不要依赖 Play Mode 才能证明。

## Core cases (P0 checklist)

1. A\* 找到最短路径  
2. 多条同成本路径结果稳定  
3. 断路返回明确失败，不死循环  
4. `routeProgress` 过拐点仍单调增加  
5. 默认索敌优先离目标最近（按路径进度）  
6. 完全同优先级时稳定决胜  
7. 飞行 / 隐匿 / 不可选中被过滤  
8. 合法目标锁定不抖动切换  
9. Block 1 只接收一个敌人  
10. Block N 按接触顺序分配  
11. 满阻挡时额外敌人继续前进  
12. 角色撤退后全部释放  
13. 死亡不留下反向阻挡引用  
14. HitFrame 只结算一次  
15. 同 Tick 伤害统一在 Cleanup 死亡  
16. 同 Seed + Command Log → 同 Checksum  
17. 改一个 Command → Checksum 分歧  
18. 战斗结束后拒绝玩法 Command  

## Server cases (from v0.6+)

- 鉴权与 API 合约  
- Seed / 版本仅由服务端颁发  
- Finish 幂等  
- 过期 Run、错误 Hash、超限指令、乱序 Sequence 被拒绝  
- 复盘成功/失败均有可解释原因  

## Performance targets (measure before claim)

- 客户端 1080p 目标 60 FPS  
- 200 敌人时 `Battle.Core.Step` p95 目标低于 2ms（以开发机实测为准）  
- 约 10 分钟战斗的服务端复盘目标低于 1s  
- README 必须同时写清 CPU、构建版本、实体数、Tick Rate、均值与 p95  

## CI stance

- Required：`dotnet-ci`（Core + Server）  
- Unity 测试与云构建可分阶段接入，不因 License 阻塞 `main`  
