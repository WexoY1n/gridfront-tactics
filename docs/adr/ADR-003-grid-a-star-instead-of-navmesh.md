# ADR-003: Grid A* Instead of NavMesh

## Status

Accepted

## Context

玩法需要稳定、可测、可缓存的格子路径，并为索敌提供 `routeProgress`。NavMesh 更偏连续空间导航，难以直接表达高台/近战位规则与确定性决胜。

## Decision

关卡使用整数网格；P0 以必经点分段 A\* 生成路径并缓存；禁止斜向；同代价路径用稳定键决胜。

## Alternatives

1. Unity NavMesh / NavMeshAgent  
2. 纯手工折线路径，不跑 A\*  
3. 每帧对每个敌人全图重寻路  

## Consequences

- 优点：规则可单测；路径可 Debug；易做缓存与进度  
- 代价：需要自研网格工具与可视化；动态堵路要显式 `navVersion`（P1）  
