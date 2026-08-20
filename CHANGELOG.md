# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.2.0] - 2026-08-20

### Added

- Orthogonal A* with stable tie-breaks, path cache, integer `routeProgress`, and path followers.
- Unity `PathMarch` adapter: grid tiles, marching dots, F2 path debug (read-only view).
- README path-march GIF for the v0.2.0 demo.

## [0.1.0] - 2026-08-19

### Added

- Repository skeleton: docs layout, shared/server/client placeholders, GitHub templates.
- Formal docs split from plan: product brief, architecture, combat rules, replay protocol, roadmap, testing.
- Archived full plan snapshot at `docs/planning/project-plan-v1.md`.
- Proposed ADRs 003–005 (A*, command-log replay, no ECS for slice).
- Unity 6 client project with local `battle-core` and MCP package references.
- `BattleRunner` fixed-tick loop with canonical SHA-256 checksum (`netstandard2.1`).
- Core determinism tests: identical seed over 1000 ticks; command divergence.
