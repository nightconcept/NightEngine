# Workflows

## Default Loop
1. Install toolchain with `mise install`.
2. Build with `mise build`.
3. Run targeted tests with `dotnet test` or the full sweep with `mise prepare`.
4. Regenerate docs with `mise docs` or `python scripts/update_api_doc.py` when public API/docs changed.

## Commands
- Build: `dotnet build Night.sln`
- Clean: `mise clean`
- Format: `dotnet format --verbosity verbose Night.sln`
- Test: `dotnet test`
- Run sample: `dotnet run --project src/SampleGame/SampleGame.csproj`
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
