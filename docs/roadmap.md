# Roadmap

按每周约 15–20 小时估算的兼职节奏。全职可压缩周期，但不要砍验收与测试。

归档来源：[planning/project-plan-v1.md](planning/project-plan-v1.md) §8。

## Milestones

| Version | Focus | Must-pass acceptance |
|---|---|---|
| `v0.1.0` | Monorepo、Battle.Core、固定 Tick、CI 雏形 | 无场景跑 1,000 Tick；同 Seed 结果一致 |
| `v0.2.0` | 网格、A\*、路径缓存、敌人移动、路径 Debug | 多路线正确；无路可控；稳定路径测试通过 |
| `v0.3.0` | 部署合法性、朝向射程、角色索敌、攻击时间线 | 目标排序稳定；失效后正确重选 |
| `v0.4.0` | 阻挡容量、双向关系、敌人索敌、死亡/撤退释放 | 1/2/3 阻挡正确；无幽灵引用 |
| `v0.5.0` | 波次、费用、技能、治疗、胜负、2 图 | 可完整玩到结算；教学关可理解 |
| `v0.6.0` | Auth、Manifest、Run、Finish、PostgreSQL、OpenAPI | Compose 一键启动；接口集成测试通过 |
| `v0.7.0` | Command Log、分段 Checksum、复盘与拒绝原因 | 合法通过；篡改指令/Hash 被拒绝 |
| `v0.8.0` | 配置校验、对象池、空间索引、Benchmark、Debug HUD | 200 敌人基准有数据；非法配置构建失败 |
| `v1.0.0` | 原创表现、教程、GIF/视频、Release、文档收口 | 新机器按 README 可运行；演示无阻塞 Bug |

## Definition of done (every version)

- 有对应 Issue 与验收清单  
- 核心行为至少一条自动化测试  
- README 或 `docs/` 已更新  
- 有 10–30 秒 GIF 或截图证明功能  
- `main` 构建通过并打 Tag  
- 不留下阻塞下一阶段的临时硬编码谎言  

## Current

`v0.3.0` 已发布（Tag `v0.3.0`）。  
下一里程碑 `v0.4.0`：阻挡容量、双向关系、敌人索敌、死亡/撤退释放。
