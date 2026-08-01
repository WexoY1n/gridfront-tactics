# Contributing

本仓库目前以个人求职 Demo 为主。若你 fork 后提交 PR，请遵循以下约定。

## Branching

- `main` 保持可运行
- 短生命周期分支：`feat/...`、`fix/...`、`docs/...`

## Commits

使用 [Conventional Commits](https://www.conventionalcommits.org/)：

```text
feat(core): add stable target priority policy
fix(block): release enemies on operator retreat
docs(adr): record fixed-tick decision
```

## Pull requests

PR 需包含：

1. 动机
2. 改动摘要
3. 测试说明
4. 截图或 GIF（涉及可观察行为时）
5. 风险与回滚说明

## Definition of done（每个里程碑）

- 对应 Issue / 验收清单完成
- 核心行为至少有一个自动化测试
- README 或 `docs/` 已更新
- `main` CI 通过
- 不留下阻塞下一阶段的临时硬编码
