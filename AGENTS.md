# NightEngine

⚠ This file is hard-limited to ≤100 lines. Update spokes, not the hub.

## Intent

NightEngine is the Nightcircle game engine for Python/Pyxel and Lua/LÖVE.
Both implementations share deterministic frame, input, RNG, replay and trace contracts.

## Stack

- Python 3.14, Pyxel 2.x (the pyxel-ne fork for Nightrunner), uv, ruff, ty.
- LuaJIT 2.1, LÖVE 11.5, StyLua 2.
- Node for the browser pointer bridge tests and the love2d-mcp dev tool.
- mise installs the tools from `mise.toml`; just runs the tasks.
- Development platforms: Linux x64, macOS arm64 and Windows x64.
- GitHub is the primary repository. A workflow mirrors it to Forgejo.

## Layout

- `python/src/nightengine/`: Python core, Pyxel host, web, Android and LÖVE builders.
- `lua/`: Lua package root. Games mount it as `nightengine`.
- `tests/fixtures/`: shared fixtures that both implementations pass.
- `python/tests/`, `tests/lua/`: implementation checks.
- `tools/love2d-mcp/`: pinned MCP bootstrap for the Lua dev bridge.
- `examples/`: runnable examples for both implementations.

## Essential Commands

- **Setup**: `mise install && uv sync --project python`
- **Test**: `just check`
- **Lint and format check**: `just lint`
- **Format**: `just format`
- **Type check**: `just typecheck`
- **Build**: `just build`

## Hard Rules

- Read [Architecture](docs/architecture.md) and [Compatibility](docs/compatibility.md)
  before you change an interface.
- Keep each core deterministic. A core imports no runtime and knows no game.
- Change Python and Lua together. Keep both in agreement with `tests/fixtures/`.
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

Reference:

- [Architecture](docs/architecture.md) — Cores, hosts, layout and game checkout
- [Compatibility](docs/compatibility.md) — Shared contract and fixtures
- [Python/Pyxel](docs/python-pyxel.md) — Python API, host, web, Android and desktop builds
- [Lua/LÖVE](docs/lua-love.md) — LÖVE host setup and builds
- [Observability](docs/observability.md) — `app.dev` controls and the love2d-mcp bridge
- [Nightrunner APIs](docs/nightrunner-api.md) — Bindings, events, storage and pacing
- [Execution plans](docs/exec-plans/active/README.md) — Active and completed plans
