# Context And Harness Engineering

## Definitions Used Here
- Context engineering: decide what information an agent sees, when it sees it, and in what form so it can act with high precision instead of reading the whole repo.
- Harness engineering: build the guardrails, workflows, checks, and feedback loops around the agent so changes are reliable, reviewable, and easy to verify.

## Context Rules For NightEngine
- Start with `AGENTS.md`, then load only the spoke and repo files needed for the task.
- For engine code, usually read the target module in `src/Night/`, its tests in `tests/Night.Tests/`, and any sample usage in `src/SampleGame/`.
- For docs or build tasks, read `mise.toml`, `docs/docfx.json`, and the affected docs before widening scope.
- Prefer repo-native sources over generated artifacts; avoid using `project/digest.txt` as primary truth if source files are available.
- Keep architecture context high-level unless the change crosses module boundaries or touches SDL interop.

## Harness Rules For NightEngine
- Use existing task entry points first: `mise build`, `mise format`, `dotnet test`, `mise docs`.
- Match verification depth to blast radius:
  - Single-module edit: targeted build/test for that area.
  - SDL/native/runtime or game-loop edit: run `mise smoke` (headless frame-count check) in addition to tests. On macOS, `mise smoke` will report `passed: false` due to the OpenGL/offscreen constraint — this is expected; use `mise game` for local runtime verification instead.
  - Shared API or config edit: full `dotnet test` and, if relevant, docs regeneration.
- When `mise gate` fails, read `test-results/gate.json` — the `first_failure` field identifies the broken stage so you don't need to parse stdout.
- Separate vendored-binary refreshes from source changes when possible to preserve reviewability.
- Record blockers plainly when verification cannot run, especially for graphics/runtime paths that may depend on local native assets.

## Operating Pattern
1. Gather minimal context.
2. Make the smallest coherent change.
3. Run the narrowest meaningful verification.
4. Expand verification if shared surfaces changed.
5. Report what was changed, what was verified, and what remains unproven.
