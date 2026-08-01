# ADR-004: Command Log Server Replay

## Status

Proposed

## Context

单机 Demo 若只有客户端上报胜负，服务端无法证明可信结果，也缺少后端与一致性工程的展示点。

## Decision

客户端上传有序 Command Log 与 Checksum；服务端使用共享 `Battle.Core` 无头复盘并比较结果。Seed 与 `configHash` 由服务端颁发。

## Alternatives

1. 信任客户端结算  
2. 实时帧同步联网战斗  
3. 服务端逐步模拟玩家输入（操作延迟不可接受）  

## Consequences

- 优点：可展示校验闭环；篡改可定位首次分歧 Tick  
- 代价：必须保持确定性纪律；需要协议与集成测试  
