# ADR-005: No ECS/DOTS for the Vertical Slice

## Status

Proposed

## Context

完整 ECS/DOTS 重构能展示技术广度，但会显著增加学习与调试成本，挤占寻路/阻挡/复盘等更有辨识度的交付。

## Decision

Vertical Slice 使用清晰的固定顺序 Systems + 普通 C# 状态模型；不引入完整 ECS/DOTS 作为交付前提。

## Alternatives

1. Unity DOTS/ECS 全量重构  
2. 自研微型 entity-component 容器（可在后续评估）  

## Consequences

- 优点：更快打穿可演示闭环；测试与复盘更直观  
- 代价：极端实体量下的性能叙事需靠实测与局部优化（池、空间索引）支撑  
