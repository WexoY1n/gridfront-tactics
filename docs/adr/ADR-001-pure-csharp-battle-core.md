# ADR-001: Pure C# Battle Core

## Status

Accepted

## Context

求职 Demo 需要同时证明玩法系统、可测试性与服务端复盘能力。若战斗规则写在 MonoBehaviour 中，服务端无法复用，单元测试也依赖 Unity 运行时。

## Decision

将战斗逻辑放在无 `UnityEngine` 依赖的 `netstandard2.1` 程序集 `Battle.Core` 中。Unity 与 ASP.NET Core 通过同一份源码（本地 Unity Package + `.csproj` Compile Include）引用。

## Alternatives

1. 仅 Unity 内实现，服务端信任客户端结果  
2. 复制两份规则代码分别维护  
3. 使用完整 ECS/DOTS 作为唯一内核  

## Consequences

- 优点：规则可单测；客户端与服务端可共享；依赖方向清晰  
- 代价：需要 Adapter 同步 View；作者需保持整数/确定性纪律  
