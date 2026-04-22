# Architecture

## Repo Shape

This is a monorepo containing two products: **NightFrame** (framework) and **NightEngine** (higher-level engine).

- `src/NightFrame/`: NightFrame library (assembly: `Night`, namespace: `Night`). The primary active development surface.
- `src/NightEngine/`: NightEngine stub (assembly: `NightEngine`, namespace: `NightEngine`). Depends on NightFrame; higher-level systems go here.
- `src/NightFrame.Sample/`: NightFrame sample executable. Consumers of the framework and runtime validation target.
- `tests/NightFrame/`: xUnit tests for NightFrame modules.
- `tests/NightEngine/`: xUnit tests for NightEngine (minimal stub).
- `docs/`: docfx config and authored docs (one tree with NightFrame and NightEngine sections).
- `scripts/`: maintenance scripts for SDL sync, tool updates, and API doc refresh.
- `lib/`: prebuilt native SDL assets and related external material.

## Solutions

- `night-mono.slnx`: umbrella solution — all projects (primary build target).
- `NightFrame.slnx`: framework-focused solution — NightFrame, NightFrame.Sample, NightFrame.Tests only.

## Core Boundaries

- Keep reusable framework behavior in `src/NightFrame/`, not in `src/NightFrame.Sample/`.
- Keep NightEngine behavior in `src/NightEngine/`. NightEngine may depend on NightFrame; the reverse is not permitted.
- Treat `src/NightFrame.Sample/` as a consumer of the framework and a validation target for developer experience.
- Add or update tests in `tests/NightFrame/` for NightFrame behavior changes.
- Keep documentation-facing changes aligned with `docs/` and `scripts/update_api_doc.py` when public API shifts.

## NightFrame Layout

- `Framework.cs` and `IGame.cs` define the high-level game loop surface.
- Feature folders such as `Graphics/`, `Window/`, `Keyboard/`, `Mouse/`, `Timer/`, `Filesystem/`, and `Configuration/` hold the public API modules.
- `SDL/` is the low-level bridge to SDL bindings. Keep direct SDL coupling localized there when possible.
- `VersionInfo.cs`, `Error.cs`, and config classes are shared support surfaces.

## Runtime Notes

- Native SDL binaries are copied from `lib/SDL3-Prebuilt/` by project configuration.
- Sample and test projects include OS-conditional native library content entries.
- This means changes that compile may still fail at runtime if native assets or platform conditions drift.

## Change Placement

- New NightFrame capability: `src/NightFrame/` + matching tests in `tests/NightFrame/`.
- New NightEngine capability: `src/NightEngine/` + matching tests in `tests/NightEngine/`.
- Sample/demo behavior: `src/NightFrame.Sample/`.
- Docs/tooling pipeline: `docs/`, `scripts/`, or `mise.toml`.
- Vendored library refreshes: isolate from unrelated engine edits.
