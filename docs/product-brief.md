# Product Brief — Gridfront Tactics

一页范围说明。详细历史意图见 [planning/project-plan-v1.md](planning/project-plan-v1.md)。

## One-liner

原创命名与美术的网格战术塔防 Vertical Slice：用可测试的固定 Tick 战斗内核复现寻路、稳定索敌与容量阻挡，并用 ASP.NET Core 对客户端指令日志做权威复盘校验。

## Why this demo

向招聘者证明完整链路，而不是堆角色数量：

| Signal | Evidence in repo |
|---|---|
| 玩法工程 | 寻路 / 索敌 / 阻挡 / 攻击 / 技能 / 波次 + 规则文档 |
| 算法 | A\*、稳定排序、路径进度、确定性数学 |
| 架构 | 无引擎依赖的 `Battle.Core`、asmdef、DTO 边界 |
| 测试 | Core 单测、服务端集成测、回放一致性 |
| 后端 | Run API、PostgreSQL、OpenAPI、Docker Compose |
| 工程 | Issues/PR、SemVer、Actions、Release、Changelog |
| 产品 | 固定范围、验收标准、非目标、版权声明 |

## Principles

1. **先规则正确，再做表现** — 无场景、无动画也能测战斗结果  
2. **固定范围，拒绝堆内容** — 系统完成度优先于内容体量  
3. **服务端服务可信结果** — 不是登录摆设，必须参与复盘  
4. **可观察** — Debug Overlay 展示路径、索敌、阻挡、Tick、Checksum  
5. **原创表达** — 不使用任何商业塔防作品的名称、角色、图标、音效、地图或 UI 资源；仅声明 genre inspiration

## In scope (Vertical Slice)

- 2 张原创地图（教学关、综合机制关）
- 6 个角色原型，6 类敌人
- 部署费用 / 朝向 / 撤退 / 再部署冷却 / 基地生命
- 2 类技能（自动充能或手动释放为 P0）
- 一局 20–40 敌人完整波次
- 结算 + Command Log + 服务端验证
- Windows 可执行包（WebGL 仅时间充足时）

## Out of scope

- 抽卡、商城、付费、复杂养成、剧情
- 实时 PVP、联机协作、帧同步联网战斗
- 大量角色与关卡、LiveOps、生产级账号体系
- 完整 ECS/DOTS、微服务、Kubernetes
- 对任何商业作品 UI/美术的 1:1 复刻

## Success criteria

1. 新玩家约 3 分钟内理解部署、阻挡与技能  
2. Debug 能看清 A\* 路径、索敌顺序、阻挡槽位  
3. 同配置 + Seed + Command Log → 客户端与服务端最终 Checksum 一致  
4. PR 必须通过 Core 与 Server 自动化测试  
5. 仓库首页约 30 秒内可见玩法 GIF、架构、CI、下载入口  

## Tech stack (target)

| Layer | Choice |
|---|---|
| Client | Unity 6.3 LTS, URP |
| Core | C# `netstandard2.1`, no `UnityEngine` |
| Server | ASP.NET Core 10 Minimal API |
| Data | EF Core 10 + PostgreSQL |
| Local ops | Docker Compose |
| CI | GitHub Actions |

## Related docs

- [architecture.md](architecture.md)
- [roadmap.md](roadmap.md)
- [testing.md](testing.md)
