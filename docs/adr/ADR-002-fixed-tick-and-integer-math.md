# ADR-002: Fixed Tick and Integer Math

## Status

Accepted

## Context

跨端复盘要求同一 Seed、配置与 Command Log 得到相同结果。`Time.deltaTime`、浮点坐标与不稳定遍历顺序会导致客户端与服务端 Checksum 分歧。

## Decision

- 核心时间只使用整数 Tick（目标 20 TPS）  
- 位置与百分比使用整数/定点数（例如 `1 tile = 1000 units`）  
- 禁止在核心读取 `DateTime.Now` / `Random.Shared`  
- 决策遍历前按稳定键排序  

## Alternatives

1. 变帧 + 浮点模拟  
2. 锁步联网帧同步（超出 Demo 范围）  

## Consequences

- 优点：可复现、易测试、易定位首次分歧 Tick  
- 代价：表现层需要插值；部分公式需明确舍入策略  
