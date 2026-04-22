# Research: Feedback Loop Foundation

**Date:** 2026-04-19T07:26:40-07:00  
**Commit:** 8c167db04a409184ccc0fa8595d5bd0d16445b15  
**Branch:** dev  
**Repo:** NightEngine  

---

## Topic

What exists in the codebase today that is relevant to building autonomous agent feedback loops: testing infrastructure, runtime observability, headless/headed split, verification pipelines, and agent harness.

---

## Territory Map

### Testing Infrastructure

Three test types exist in `tests/Core/`:

| Class | Window | Trait | Use |
|---|---|---|---|
| `ModTestCase` | No | `Automated` | Isolated unit tests |
| `GameTestCase` | Yes | `Automated` | In-engine-loop automated |
| `ManualTestCase` | Yes | `Manual` | User-confirmed visual |

Test runner: `scripts/run_tests.py` wraps `dotnet test`.

**Headless default** (`run_tests.py:191`): `SDL_VIDEODRIVER=dummy`, filter `TestType=Automated`.  
**Headed** (`run_tests.py:260-266`): removes dummy driver, enables real SDL.  
**Headed on macOS**: requires Screen Recording permission; manual tests cannot run via `dotnet test` (xUnit sandbox lacks entitlements — `tests/Core/` docs, `.agents/testing.md:189-208`).

Result file: `test-results/results.trx` (TRX XML, `run_tests.py:182-185`).  
Failure tracking: `.last-test-failures` (`run_tests.py:38`).  
Exit codes: 0=pass, 1=fail, 2=partial/skipped.

### Build & Verification Pipeline

Gate task (`mise.toml:50-61`): tool restore → clean → format → build → test → docs → API doc update. All serial.

Narrower entry points:
- `mise build` → `dotnet build Night.slnx`
- `mise test` → `python scripts/run_tests.py`
- `mise format` → `dotnet format --verbosity verbose Night.slnx`
- `mise gate` → full pipeline

No structured machine-readable verdict emitted by any task today. All output is human-readable stdout.

### Runtime Observability

**Logging**: `src/Night/Log/LogManager.cs` — 6 levels (Trace→Fatal), 4 sinks: `SystemConsoleSink`, `FileSink`, `MemorySink`, `InGameConsoleSink`. Format: `YYYY-MM-DDTHH:mm:ss.fffZ [LEVEL] [Category] Message`.

Session file logging: `--session-log` CLI flag writes to `./session/session_log_YYYYMMDD_HHmmss.log` (`src/Night/CLI.cs`).

**Headless detection** (`Framework.Run.cs:104-117`): reads `SDL_VIDEODRIVER` env var; accepts `dummy` or `offscreen`. In headless mode: sets `SDL_RENDER_DRIVER=software`, elevates log level to Debug.

**Observable frame state** (public API):
- `Timer.GetFPS()` — int, updated per second
- `Timer.GetDelta()` — float, current frame
- `Timer.GetAverageDelta()` — double, 60-frame rolling
- `Timer.GetTime()` — double, elapsed seconds
- `Window.GetMode()` — WindowMode struct (dims, fullscreen, HiDPI)
- `Framework.IsInputInitialized` — bool
- `Framework.GetVersion()` — string

**Game loop** (`Framework.Run.cs:354-421`): `loopCount` (total iterations) and `frameCount` (per-second counter) exist as local vars but are not exposed publicly.

**SDL events**: all dispatched via callbacks on `IGame` — keyboard, mouse, joystick, gamepad, file drop. Each logged at Debug level on dispatch (`Framework.Events.cs:43`).

**No screenshot/frame capture exists.** Searched for `screenshot`, `capture`, `ReadPixels`, `SaveImage` — none found. `SDL_RenderReadPixels` is not wrapped anywhere.

### Agent Harness

Hub: `AGENTS.md` / `CLAUDE.md` (identical, ≤100 lines, hard limit).  
Spokes: `.agents/*.md` — architecture, context-harness, guidelines, love-api, prd, roadmap, testing, workflows.  
Epics: `.agents/epics/` — filesystem.md, keyboard.md, mouse.md (active specs).

**Operating pattern** (`.agents/context-harness.md:23-28`):
1. Gather minimal context
2. Make smallest coherent change
3. Run narrowest meaningful verification
4. Expand if shared surfaces changed
5. Report changed / verified / unproven

**Verification depth rule** (`.agents/context-harness.md:14-21`): single-module → targeted build/test; shared API → full test + docs; SDL/native → sample run when feasible. Record blockers explicitly when visual/runtime verification cannot run.

**No `thoughts/`, `.claude/`, or skill files exist** in the repo today (created `thoughts/` for this document).

---

## Key Facts for Feedback Loop Design

1. **Headless/headed split is already built** — `SDL_VIDEODRIVER=dummy` is the mechanism; `mise test` uses it by default.
2. **Structured test results exist** — TRX XML at `test-results/results.trx`; `run_tests.py` exit codes are machine-readable.
3. **Logging is multi-sink and configurable** — `FileSink` + `MemorySink` exist; agents can redirect output to a file without modifying engine code.
4. **No pixel-level output capture** — `SDL_RenderReadPixels` not exposed; screenshot skill requires new engine surface.
5. **Frame state is readable but internal** — `loopCount` / `frameCount` are local vars in `Framework.Run.cs`; not exposed; smoke-run verification must rely on process exit code + log parsing today.
6. **Gate pipeline has no machine-readable summary** — `mise gate` emits human stdout only; a structured verdict layer doesn't exist yet.
7. **macOS has hard constraints on headed automation** — xUnit sandbox + entitlements block automated headed tests via `dotnet test`; only `mise game` (direct run) can show a real window in automation.
8. **`offscreen` SDL driver is recognized** — `Framework.Run.cs:106` accepts `SDL_VIDEODRIVER=offscreen`; this is the path to headless frame rendering without a dummy driver.

---

## Concrete Risks

- **macOS headed tests silently do nothing**: `ManualTestCase` tests are filtered out by default; an agent that runs `mise test` believes it passed visual tests when it didn't run any.
- **Gate failure is opaque**: `mise gate` exits non-zero but the step that failed requires parsing stdout to identify — no structured exit code per stage.
- **Frame render without display on macOS**: `offscreen` driver + Metal backend has known initialization quirks (`Window.Mode.cs:74-82` has a macOS-specific OpenGL fallback); screenshot capture may behave differently per platform.
- **`loopCount` not surfaced**: an agent cannot confirm the game loop ran N frames without adding instrumentation.
