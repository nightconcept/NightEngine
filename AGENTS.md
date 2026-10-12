# NightEngine

⚠ This file is hard-limited to ≤100 lines. Update spokes, not the hub.

## Intent

NightEngine is the single engine entry point for Nightcircle games. It supports three runtimes:
Python/Pyxel (prototypes), Lua/LÖVE (more features) and C#/MonoGame (most features).
All share deterministic frame, input, RNG, replay and trace contracts. Games pin it as a submodule.
The engine ships cores, hosts, builders, conventions, agent skills and the docs site.
Targets: Linux x64, macOS arm64, Windows x64, Android and iOS. See [Vision](docs/vision.md).

## Stack

- Python 3.14, Pyxel 2.x (the pyxel-ne fork for Nightrunner), uv, ruff, ty.
- LuaJIT 2.1, LÖVE 11.5, StyLua 2.
- C#/.NET 10 and MonoGame 3.8.6.
- Astro Starlight for the docs site in `site/` (pnpm).
- Node for the browser pointer bridge tests and the love2d-mcp dev tool.
- mise installs the tools from `mise.toml`; just runs the tasks.
- Development platforms: Linux x64, macOS arm64 and Windows x64.
- GitHub is the primary repository. A workflow mirrors it to Forgejo.

## Layout

- `python/src/nightengine/`: Python core, Pyxel host, web, Android and LÖVE builders.
- `lua/`: Lua package root. Games mount it as `nightengine`.
- `csharp/`: C# core (`NightEngine.Core`), MonoGame host sources and contract tests.
- `skills/`: agent skills that games link into `.agents/skills/`.
- `docs/`: the single source of truth. `site/` renders it.
- `tests/fixtures/`: shared fixtures that both implementations pass.
- `python/tests/`, `tests/lua/`: implementation checks.
- `tools/`: pinned MCP bootstraps for the LÖVE and MonoGame dev bridges.
- `examples/`: runnable examples for both implementations.

## Essential Commands

- **Setup**: `mise install && uv sync --project python`
- **Test**: `just check`
- **Lint and format check**: `just lint`
- **Format**: `just format`
- **Type check**: `just typecheck`
- **Build**: `just build`
- **Docs**: `just docs`

## Hard Rules

- Read [Architecture](docs/concepts/architecture.md) and the [Contract](docs/concepts/contract.md)
  before you change an interface.
- Keep each core deterministic. A core imports no runtime and knows no game.
- Change Python, Lua and C# together for contract changes. Keep all in agreement with `tests/fixtures/`.
- Every core, host and dev capability is in `parity.toml`. Update it and run `just parity`.
  Python names are canonical; Lua and C# derive theirs. Decide catch-up with
  [Layers and parity](docs/concepts/layers.md).
- Change the docs and skills in the same commit as the code. See [Documentation](docs/repo/docs.md).
- Keep the observability tools and their licenses in the engine.
- Exclude `lua/host/dev/` from game release archives.
- Start shared engine changes from `main`. Games pin verified commits.
- Use Conventional Commits.
- Run `just check` and `just lint` before you commit.
- Before you ship host or path changes, run the integration game release builds.

## Spoke Index

Agent workflow:

- [Engine rules](docs/repo/engine-rules.md) — Core limits, enforcement and contract changes
- [Workflow](docs/repo/workflow.md) — Branches, verification, CI, releases and plans
- [Documentation](docs/repo/docs.md) — Where a page goes, page format, keeping docs true

Direction:

- [Vision](docs/vision.md) — The charter: runtimes, platforms, conventions, AI native
- [Roadmap](docs/roadmap.md) — Gaps between today and the vision, in order
- [Execution plans](docs/exec-plans/active/README.md) — Active and completed plans

Reference:

- [Documentation Map](docs/index.md) — Engine portal for game makers and contributors
- [Layers and parity](docs/concepts/layers.md) — The layers, the manifest and the catch-up rules
- [Runtime parity](docs/concepts/parity.md) — Generated table of each capability per runtime
- [Platforms](docs/concepts/platforms.md) — Which runtime builds which platform
- [Game layout](docs/games/layout.md) — Convention-over-configuration folders for games
- [Game commands](docs/games/commands.md) — The fixed justfile surface for games, and `just dev`
- [Python/Pyxel](docs/runtimes/python-pyxel.md), [Lua/LÖVE](docs/runtimes/lua-love.md),
  [C#/MonoGame](docs/runtimes/csharp-monogame.md) — Runtime APIs, hosts and builds
- [Observability](docs/reference/observability.md) — `app.dev` controls and the MCP bridges
