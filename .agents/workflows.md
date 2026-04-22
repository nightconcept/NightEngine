# Workflows

## Default Loop
1. Install toolchain with `mise install`.
2. Restore repo-local tools with `mise setup`.
3. Build with `mise build`.
4. Run targeted tests with `dotnet test` or the full quality gate with `mise gate`.
5. Regenerate docs with `mise docs` or `python scripts/update_api_doc.py` when public API/docs changed.

## Commands
- Build: `dotnet build Night.slnx`
- Clean: `mise clean`
- Format: `dotnet format --verbosity verbose Night.slnx`
- Setup: `mise setup`
- Gate: `mise gate`
- Test: `mise test` (headless automated), `mise test -- --headed` (real SDL), `mise test -- --all --headed` (full `dotnet test`)
- Run sample: `dotnet run --project src/SampleGame/SampleGame.csproj`
- Smoke check: `mise smoke` (headless offscreen, 60 frames, JSON verdict to stdout; Linux/CI only — see Cautions)
- Gate verdict: `cat test-results/gate.json` (written after every `mise gate` run; `first_failure` identifies the broken stage)
- Screenshot: `dotnet run --project src/SampleGame/SampleGame.csproj -- --frame-limit N --screenshot-at N` (writes `test-results/frame_NNNNNN.ppm`)
- Generate docs site: `dotnet docfx docs/docfx.json`

## Expectations
- Prefer the smallest verification step that proves the change, then run broader checks when touching shared surfaces.
- Run tests after changes in `src/Night/` unless blocked by environment/runtime constraints.
- Run formatting when editing C# files or solution-wide config.
- Rebuild docs when XML comments, public APIs, or docs content change.

## Maintenance Tasks
- `mise sdl`: updates SDL bindings/submodule state and syncs native libs.
- `mise tools`: refreshes repo tools and SDL-related dependencies.
- `mise digest`: regenerates the project digest artifact under `project/`.

## Cautions
- `lib/` contains large binary assets; avoid incidental churn.
- The worktree may contain unrelated updates to prebuilt SDL binaries or tools. Do not revert them unless explicitly asked.
- docfx content lives under `docs/` and writes output to `docs/_site/`.
- `mise smoke` requires `SDL_VIDEODRIVER=offscreen`, which needs an OpenGL library on macOS. On macOS developer machines without a headless GL environment, the smoke run will report `passed: false` with an OpenGL error — this is expected. Use `mise game` for local runtime verification on macOS; smoke run is designed for Linux CI.
- `mise build` must succeed before `mise smoke` in a fresh environment (or pass `--no-build` to skip).
