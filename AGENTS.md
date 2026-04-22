# ⚠ This file is hard-limited to ≤100 lines. Update spokes, not the hub.
# NightEngine

## Intent
This is a monorepo for two products: **NightFrame** (Love2D-style framework, `src/NightFrame/`) and **NightEngine** (higher-level engine, `src/NightEngine/`).
Current work centers on the `NightFrame` library, sample game, tests, and generated docs. NightEngine is a minimal stub for future work.

## Stack
- .NET 10 SDK
- C# 13
- SDL3 via `SDL3-CS`
- xUnit for tests
- docfx for API/site docs
- Python 3.13 for repo scripts
- `mise` for task orchestration

## Essential Commands
- **Install toolchain**: `mise install`
- **Build**: `mise build`
- **Run sample**: `mise game`
- **Test**: `mise test`
- **Format**: `mise format`
- **Docs**: `mise docs`
- **Pre-commit sweep**: `mise gate`
- **Refresh SDL/tooling**: `mise sdl`, `mise tools`

## Working Rules
- Start from the hub, then load only the spoke that matches the task.
- Keep edits scoped; this repo may contain user-owned binary or submodule-related changes.
- Prefer `mise` tasks when they exist; fall back to direct `dotnet` or `python` commands only when needed.
- Run build and test verification serially, not in parallel, to avoid file-lock and stale-asset noise.
- Treat generated docs and vendored binaries as separate surfaces from core engine code.
- Use one-line Conventional Commits for commit messages.

## Spoke Index
- [.agents/architecture.md](.agents/architecture.md) - Code layout, module boundaries, and where changes belong.
- [.agents/workflows.md](.agents/workflows.md) - Build, test, docs, formatting, and maintenance workflows.
- [.agents/context-harness.md](.agents/context-harness.md) - Context engineering and harness rules for AI agents.
- [.agents/guidelines.md](.agents/guidelines.md) - Code style, naming, organization, SDL3-CS mapping.
- [.agents/testing.md](.agents/testing.md) - Testing framework, test types, writing tests, macOS constraints.
- [.agents/roadmap.md](.agents/roadmap.md) - Version targets and feature roadmap.
- [.agents/love-api.md](.agents/love-api.md) - Love2D API coverage map (what is implemented vs. pending).
- [.agents/prd.md](.agents/prd.md) - Product vision, technical decisions, future Night.Engine plans.
- [.agents/epics/filesystem.md](.agents/epics/filesystem.md) - Active epic: Night.Filesystem module spec.
- [.agents/epics/keyboard.md](.agents/epics/keyboard.md) - Active epic: Night.Keyboard module spec.
- [.agents/epics/mouse.md](.agents/epics/mouse.md) - Active epic: Night.Mouse module spec.
- [docs/docs/introduction.md](docs/docs/introduction.md) - High-level product and architecture intent.
- [docs/docs/getting-started.md](docs/docs/getting-started.md) - Developer setup and runtime prerequisites.
- [README.md](README.md) - Current project status, roadmap, and top-level commands.
