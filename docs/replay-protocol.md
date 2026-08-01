# Replay Protocol（草案）

## Create run

服务端返回 `runId`、`seed`、`contentVersion`、`configHash`、`expiresAt`。

## Finish run

客户端上传：

- `runId`
- `orderedCommands`（含 `executeAtTick` 与单调 `sequence`）
- `finalTick`
- `clientChecksum`
- 可选分段 Checksum

服务端使用共享 `Battle.Core` 复盘，自行计算胜负与分数；不接受客户端指定的验证状态。

## Rejection reasons（规划）

- expired run
- config hash mismatch
- command limit exceeded
- out-of-order sequence
- checksum mismatch（附首次分歧 Tick）
