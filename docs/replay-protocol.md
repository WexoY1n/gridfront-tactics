# Replay Protocol

服务端价值是「可信 Run」闭环：发 Seed 与内容版本、收指令日志、用共享内核复盘、拒绝篡改后的胜负。  
归档来源：[planning/project-plan-v1.md](planning/project-plan-v1.md) §6；决策见 [ADR-004](adr/ADR-004-command-log-server-replay.md)。

## API (P0)

| Method | Path | Role |
|---|---|---|
| `POST` | `/api/v1/auth/guest` | 游客/开发身份 + Token |
| `GET` | `/api/v1/content/manifest` | 内容版本、Hash、关卡列表 |
| `POST` | `/api/v1/runs` | 创建 Run |
| `POST` | `/api/v1/runs/{id}/finish` | 上传日志并复盘 |
| `GET` | `/api/v1/runs/{id}` | 查询验证状态与摘要 |

P1：`profile`、`squads`、已验证 `leaderboard`。P2：公开 `replays`。

## Create run response (shape)

```json
{
  "runId": "01J...",
  "stageId": "stage_02",
  "seed": 92133741,
  "contentVersion": "0.7.0",
  "configHash": "sha256:...",
  "expiresAt": "2026-08-02T12:00:00Z"
}
```

客户端不得自指定 Seed、配置 Hash、最终分数或验证状态。

## Finish run request

上传「发生了什么」，不上传「我赢了」：

- `runId`
- `orderedCommands`（每条含 `executeAtTick`、单调 `sequence`）
- `finalTick`
- `clientChecksum`
- 可选分段 Checksum
- Idempotency Key（重复提交返回同一结果）

服务端按库中 Seed、关卡版本与编队复盘，自行计算胜负与分数。

## Command types

| Command | Enters validation? |
|---|---|
| `DeployOperatorCommand` | Yes |
| `RetreatOperatorCommand` | Yes |
| `ActivateSkillCommand` | Yes |
| `SetBattleSpeedCommand` | No（仅客户端表现） |

## Persistence (MVP)

| Table | Purpose |
|---|---|
| `users` | 身份 |
| `content_versions` | 版本与 `config_hash` |
| `squads` | 编队 JSON |
| `run_sessions` | Seed、阶段、过期、状态 |
| `run_submissions` | 指令日志与客户端 Checksum |
| `run_results` | `verified`、服务端 Checksum、分数、拒绝原因 |

MVP 可将指令日志放在 PostgreSQL `jsonb`。

## Rejection reasons

- expired run  
- config hash mismatch  
- command / payload limit exceeded  
- out-of-order or duplicate sequence  
- tick out of range  
- checksum mismatch（应附带首次分歧 Tick）  

## Security baseline

- Token 仅作 Demo 身份；密钥不进仓库  
- Finish 限制体积、Command 数、Tick 范围、过期时间  
- 日志不记录完整 Token  
