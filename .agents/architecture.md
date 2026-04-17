# Architecture

## Repo Shape
- `src/Night/`: primary library. This is the engine surface under active development.
- `src/SampleGame/`: executable sample and integration surface for runtime behavior.
- `tests/Night.Tests/`: xUnit coverage for framework modules.
- `docs/`: docfx config and authored docs.
- `scripts/`: maintenance scripts for SDL sync, tool updates, and API doc refresh.
- `lib/`: prebuilt native SDL assets and related external material.

## Core Boundaries
- Keep reusable engine behavior in `src/Night/`, not in `src/SampleGame/`.
- Treat `src/SampleGame/` as a consumer of the framework and a validation target for developer experience.
- Add or update tests in `tests/Night.Tests/` for engine behavior changes.
- Keep documentation-facing changes aligned with `docs/` and `scripts/update_api_doc.py` when public API shifts.

## Engine Layout
- `Framework.cs` and `IGame.cs` define the high-level game loop surface.
- Feature folders such as `Graphics/`, `Window/`, `Keyboard/`, `Mouse/`, `Timer/`, `Filesystem/`, and `Configuration/` hold the public API modules.
- `SDL/` is the low-level bridge to SDL bindings. Keep direct SDL coupling localized there when possible.
- `VersionInfo.cs`, `Error.cs`, and config classes are shared support surfaces.

## Runtime Notes
- Native SDL binaries are copied from `lib/SDL3-Prebuilt/` by project configuration.
- Sample and test projects include OS-conditional native library content entries.
- This means changes that compile may still fail at runtime if native assets or platform conditions drift.

## Change Placement
- New framework capability: `src/Night/` + matching tests.
- Sample/demo behavior: `src/SampleGame/`.
- Docs/tooling pipeline: `docs/`, `scripts/`, or `mise.toml`.
- Vendored library refreshes: isolate from unrelated engine edits.
