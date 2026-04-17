# ⚠ This file is hard-limited to ≤100 lines. Update spokes, not the hub.
# NightEngine

## Intent
NightEngine is an experimental cross-platform C# game engine built on SDL3.
Current work centers on the `Night` framework library, sample game, tests, and generated docs rather than a finished higher-level engine.

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
- **Test**: `dotnet test`
- **Format**: `mise format`
- **Docs**: `mise docs`
- **Pre-commit sweep**: `mise prepare`
- **Refresh SDL/tooling**: `mise sdl`, `mise tools`

## Working Rules
- Start from the hub, then load only the spoke that matches the task.
- Keep edits scoped; this repo may contain user-owned binary or submodule-related changes.
- Prefer `mise` tasks when they exist; fall back to direct `dotnet` or `python` commands only when needed.
- Run build and test verification serially, not in parallel, to avoid file-lock and stale-asset noise.
- Treat generated docs and vendored binaries as separate surfaces from core engine code.

## Spoke Index
- [.agents/architecture.md](.agents/architecture.md) - Code layout, module boundaries, and where changes belong.
- [.agents/workflows.md](.agents/workflows.md) - Build, test, docs, formatting, and maintenance workflows.
- [.agents/context-harness.md](.agents/context-harness.md) - Repo-specific context engineering and harness engineering rules for AI agents.
- [docs/docs/introduction.md](docs/docs/introduction.md) - High-level product and architecture intent.
- [docs/docs/getting-started.md](docs/docs/getting-started.md) - Developer setup and runtime prerequisites.
- [README.md](README.md) - Current project status, roadmap, and top-level commands.
