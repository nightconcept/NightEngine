# Execution Plan: merge pyxel-mobile into nightengine

**ID:** 001
**Created:** 2026-10-05
**Status:** complete
**Research:** none: self-contained. The findings are in "Approach".
**Repos:** `nightengine-py` (this repo, the engine), `nightrunner` (first game), `pyxel-mobile` (source, read only)

## Goal

nightengine owns everything a game needs to run on desktop and in a mobile browser:

- one replayable **pointer** API for touch and mouse;
- screen and safe-area info;
- browser storage;
- the browser bridge (JS);
- a web build tool that packages *any* game.

`App` samples pointers each frame, feeds them to `Game.step`, and records them in replays.
nightrunner drops its `pyxel-mobile` submodule and dependency, and its title accepts a tap or click.
pyxel-mobile is not changed. Archiving it is the owner's call (see Out of scope).

## Worker Context Bootstrap

1. Read `README.md` (engine interface and rules) and this plan.
2. Read `../pyxel-mobile/pyxel_mobile/__init__.py`, `web/touch.mjs`, `web/launcher.mjs`, `web/index.html`,
   `web/mobile.css`, `cli.py`, and `diagnostic.py`. These are the sources being merged.
3. Read `../nightrunner/AGENTS.md`.

## Approach

- **Pointers become game input, not a side channel.** pyxel-mobile is read straight from game code
  (`mobile.touches()`), so a replay cannot reproduce touch play. In the engine, pointers are a field of
  `Input` (core, pure) and part of `Recording`. A replay then plays back identically on every device.
  This follows the rule "one frame of input is data".
- **Integer coordinates.** The bridge gives float logical pixels. The host rounds them down to ints before the
  game sees them, so the live run and its replay see the same values.
- **Mouse is a pointer.** The browser bridge already uses Pointer Events, which cover mouse, touch, and pen. On the
  desktop, the host builds the same records from `pyxel.mouse_x`/`mouse_y` and the left button, with id `0`.
  So one API and one recording format cover every platform.
- **Layers follow the engine rules.** Pure records and helpers go in core (`nightengine/pointer.py`). Everything
  that touches pyxel or the browser (`js`) goes in `host/`. The web build tool uses only the stdlib
  (`nightengine/web/build.py`). It reads the Pyxel version with `importlib.metadata`, so it never imports pyxel.
- **Generic build.** The pyxel-mobile CLI hard-codes Unicycle's files and its 640x480 size. The engine's build
  takes a `WebConfig` (title, size, entry script, package dirs, asset dirs) and writes it into `index.html`.
  `launcher.mjs` reads it from there.

## Phases

### Phase 1: Core pointer model

**Depends on:** none
**Files to change:**
- `pointer.py` (new): `PointerPhase(StrEnum)` with the values `PRESSED HELD MOVED RELEASED CANCELLED`.
  `Pointer(id, x, y, start_x, start_y, phase)` is a frozen record with ints and an `active` property.
  Helpers take a tuple of pointers: `primary(ps)`, `pressed(ps)`, `released(ps)`, `in_rect(ps, x, y, w, h)`.
  A release counts as a tap only when it is not cancelled. `encode(p) -> list` and `decode(row) -> Pointer` use
  one-letter phases (`P H M R C`) for compact recordings.
- `inputs.py`: `Input` gets `pointers: tuple[Pointer, ...] = ()`. `InputTracker.feed(code, pointers=())`
  passes them through. Pointers have no edge logic here, because the phases already carry the edges.
- `core.py`: `Game.step(code, pointers=())` feeds both. Existing callers stay valid.
- `testing.py`: `Driver.step(code=0, frames=1, pointers=())`. `Driver.tap(x, y)` steps one PRESSED frame, then
  one RELEASED frame.
- `__init__.py`: export `Pointer`, `PointerPhase`.
- `tests/test_pointer.py` (new): the helpers, `active`, encode/decode round trip, `Game.step` delivering pointers
  to `Input`, and `Driver.tap`.

**Verification:** `python -m unittest discover -s nightengine/tests -t .` (from a dir where the engine is `nightengine/`)

**Status:** [x] complete

---

### Phase 2: Replayable pointers

**Depends on:** Phase 1
**Files to change:**
- `replay.py`: `Recording(seed, frames, pointers: dict[int, tuple[Pointer, ...]] = {})`. The map is sparse: only
  frames that have pointers. `save` writes `"pointers": {"<frame>": [[id, x, y, sx, sy, "P"], ...]}` only when
  the map is not empty, so a button-only file is unchanged. `load` accepts files with or without the key.
  `Recording.at(frame)` returns the frame's pointers. `play()` passes them to `game.step`.
- `autopilot.py`: `run(...)`. `choose(game)` may return an int or `(code, pointers)`. Store both.
- `tests/test_replay.py`: a round trip with pointers, an old file without pointers, and `play` replaying a tap.

**Verification:** the engine suite, as in Phase 1.

**Status:** [x] complete

---

### Phase 3: Host platform (browser bridge, mouse, screen, storage) and App wiring

**Depends on:** Phase 2
**Files to change:**
- `host/platform.py` (new), ported from `pyxel_mobile/__init__.py`. One module-level state, like `ui`.
  - `SafeArea`, `Screen(width, height, viewport_width, viewport_height, orientation, safe_area)`, `screen`.
  - `init(width, height)`: called by `App` after `pyxel.init`. It finds `js.window.nightBridge`.
  - `available()`, `is_touch_device()`, `platform()` (`android` | `desktop`).
  - `sample() -> tuple[Pointer, ...]`: the bridge snapshot when available, else `MouseTracker.sample()`.
    Coordinates are rounded down to ints.
  - `MouseTracker`: reads `pyxel.mouse_x`, `pyxel.mouse_y`, `btnp`/`btn`/`btnr(MOUSE_BUTTON_LEFT)`, and
    yields at most one pointer with id 0 and phases PRESSED, MOVED or HELD, then RELEASED.
  - `save(key, data) -> bool` and `load(key) -> str | None`: browser `localStorage`. Without the bridge they
    are False and None. A denied read raises `OSError` with an actionable message.
- `host/app.py`:
  - `App.__init__` calls `platform.init(config.width, config.height)` after `pyxel.init`.
  - In `update`, a live run samples `platform.sample()`, appends it to `recording.pointers` when it is not
    empty, and calls `game.step(code, pointers)`.
  - A replay feeds `replay.at(frame)` instead.
  - `AppConfig.mouse: bool = False` turns on `pyxel.mouse(True)` to show the system cursor.
- `host/diagnostic.py` (new): ported from `pyxel_mobile/diagnostic.py`. It reads `platform` and draws pointers
  and the safe area. It is served at `?app=debug`.
- `tests/test_rules.py`: `js` may be imported only under `host/`.
- `tests/test_platform.py` (new): the desktop fallback, a fake bridge through a patched `js` module (snapshot,
  info, safe area, int coordinates), `MouseTracker` phases with a patched pyxel, storage save/load and the denied
  read, and `App.update` recording and replaying pointers (pyxel stubbed, as `test_host.py` does).

**Verification:** the engine suite.

**Status:** [x] complete

---

### Phase 4: Web build tool and browser files

**Depends on:** Phase 3
**Files to change:**
- `web/__init__.py` and `web/__main__.py` (new): `python -m nightengine.web build|serve`.
- `web/build.py` (new), generalized from `pyxel_mobile/cli.py`:
  - `WebConfig(title, width, height, entry="game.py", packages=(), assets=())` is read from the game's
    `pyproject.toml` `[tool.nightengine.web]` table with `tomllib`.
  - `build(root, out) -> Path` zips the entry, the `.py` files of each package (recursive, no `__pycache__`),
    and the asset dirs into `game.pyxapp`. It writes `debug.pyxapp` (startup: `nightengine.host.diagnostic`).
    It copies the static files and fills `{{PYXEL_VERSION}}`, `{{TITLE}}`, `{{WIDTH}}`, and `{{HEIGHT}}`.
  - It refuses to write into the game root.
  - `serve(root, host, port, open_browser)` builds into a temp dir and serves it.
- `web/static/pointer.mjs`: `touch.mjs`, with the bridge name changed to `window.nightBridge` and a generic
  storage key prefix.
- `web/static/launcher.mjs`: reads the size from `<body data-width data-height>`. The debug size is 256x144.
- `web/static/index.html` and `web/static/style.css`: generic, with no Unicycle text.
- `web/tests/pointer.test.mjs`: ported `touch.test.mjs`. Optional: `node --test`.
- `tests/test_web.py` (new): a build of a temp game with a fake package and asset holds the expected archive
  entries, the startup scripts, and the filled placeholders. It reads the config. It refuses the game root.

**Verification:** the engine suite, plus `node --test nightengine/web/tests/` when Node is present.

**Status:** [x] complete

---

### Phase 5: nightrunner on the merged engine

**Depends on:** Phase 4 (engine pushed)
**Files to change (in `../nightrunner`):**
- Remove the `pyxel-mobile` submodule, `[tool.uv.sources]`, and the dependency. Move the `nightengine` pointer
  to the new commit.
- `pyproject.toml`: `[tool.nightengine.web]` (title, 640x360, `packages = ["nightrunner", "nightengine"]`,
  `assets = ["assets"]`). Add `nightengine.web` to the setuptools packages.
- `scenes/title.py`: a released pointer (`pointer.released(inp.pointers)`) confirms, like Z.
- `render/title.py`: the prompt reads "Press Z or tap".
- `justfile`: `web` (serve) and `web-build` (into `dist/`).
- `tests/`: a tap confirms the title, a replay with a tap, and the web build produces `game.pyxapp`.
  The dependency test is replaced by a check that `pyxel_mobile` is no longer used.
- `AGENTS.md`, `README.md`: one submodule, and the web commands.

**Verification (in `../nightrunner`):** `just check && just lint`, and `just web-build`. Also a real-window run
with the Pyxel MCP: a mouse click on the title confirms.

**Status:** [x] complete

---

### Phase 6: Engine docs

**Depends on:** Phase 5
**Files to change:** `README.md`: new sections "Pointers" (core), "Platform" (host), and "Web build", plus
`Recording`'s pointer field. Register this plan as complete.

**Verification:** `ruff check .` in the engine. Grep shows no `pyxel_mobile` or `pyxelMobile` left in either repo.

**Status:** [x] complete

## Testing Strategy

- Engine: the unittest suite (stubbed pyxel and a fake `js` module, no window) covers the records, replay
  compatibility, the bridge and mouse sources, storage, App recording and replay, and the build archive.
  A Node unit test covers the JS tracker when Node is present.
- nightrunner: tap and replay tests through `Driver`, a build test, and an MCP real-window click check.
- Not covered: a real phone browser (cutouts, backgrounding) and the Pyodide runtime itself. It loads from a CDN,
  as before. The browser Playwright suite stays in pyxel-mobile, because it tests Unicycle.

## Rollout / Integration Notes

- The engine changes are backward compatible: `step(code)`, `Driver.step(code)`, and button-only recordings are
  unchanged. The four older games keep their own engine copies and are not affected.
- Push the engine before nightrunner moves its submodule pointer.

## Known Risks

- A held finger records one entry per frame (about 25 bytes). A minute of holding is about 90 KB of JSON.
  Acceptable for now. Delta encoding is out of scope.
- `pyxel.mouse_x` on the web is also driven by touch. The browser path uses the bridge only, never
  `MouseTracker`, to avoid double input.
- The Pyxel version in `index.html` comes from the installed `pyxel` package. If the game pins another version,
  the web runtime follows the game's pin. That is intended.

## Out of Scope

- Archiving or making `pyxel-mobile` private on GitHub (the owner decides; it is public now).
- The Unicycle game and its Playwright browser suite.
- Hover (mouse position without a press), right and middle buttons, and pinch gestures.
- The display layout work (`fit()`, aspect expansion). See DECISIONS.md and the display research.
- Delta-compressed pointer recordings. Saving browser replays automatically from `App`.


## Progress Log
- 2026-10-05: Phases 1-3 complete (`86d9f77`). One deviation: the App test scripts `platform.sample` directly,
  because the mouse has its own test. The owner then made nightengine-py the version of record, so recordings
  use compact JSON. Files without `"pointers"` still load.
- 2026-10-05: Phase 4 complete (`a061650`). `ruff.toml` now targets py311 so `tomllib` sorts as stdlib.
  The 7 ported JS bridge tests pass under Node 24.
- 2026-10-05: Phases 5-6 complete. nightrunner has one submodule. A tap or click confirms its title, and a
  real-window MCP run confirmed a mouse click. `just web-build` writes the page at 640x360 with Pyxel 2.9.9.
  The engine suite has 100 tests, nightrunner 12. pyxel-mobile is unchanged. Archiving it is the owner's call.
