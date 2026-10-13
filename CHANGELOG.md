# Changelog

## 0.2.1 (2026-10-12)

- C#: `NightEngine.Core` is trimmable. `Recording.ToJson` writes with `Utf8JsonWriter` instead of reflection, with
  the same text, so a game's trimmed release build still saves replays.
- `python -m nightengine.desktop build` on macOS zips only `<slug>.app` (PyInstaller also leaves a bare
  program next to it) and keeps the app's framework links. Unicycle's macOS zip drops from 41 MB to 15 MB.

## 0.2.0 (2026-10-12)

- Add `parity.toml` and `tools/parity.py`: the layers, every capability and its status in each
  runtime. `just check` checks it; `just parity --release` gates `v*` tags. See docs/concepts/layers.md.
- The contract includes API names. Python names are canonical; Lua and C# derive theirs.
- C# (breaking): `Rng.Int` is `Randint`, `Rng.Bits` is `Getrandbits`, `Trace.Hash` is `Digest`,
  `Recording.Parse`/`Json` are `FromJson`/`ToJson`, `Pointer.Row()` is `Pointer.Encode(p)`.
  Add `Inputs` (button bits), `Pointer.Decode`, and `Game.Push`/`Pop`.
- Add `python -m nightengine.recipes`: checks that a game's justfile has the standard surface.
- `just dev` pairs: `dev_event` and `Input.dev` in all three cores (contract `dev.json`), and every host
  sends `key=value` arguments as frame-0 host events: Python `App(dev=...)`, Lua `--dev key=value`,
  C# `Host.Post` and `Host.PostDev`. Lua `--dev` also opens the LovePilot bridge.
- `python -m nightengine.love build linux`: a Linux x64 AppImage, LÖVE's AppImage with the game fused in.
  It builds on Linux with appimagetool 1.9.1 and the static AppImage runtime (type2-runtime 20251108).
- Fix: the LÖVE builder reads the engine's Lua files from the `nightengine/` submodule, so it builds on a
  Windows checkout where the `love/nightengine` link is a plain file.
- Fix: the Python and Lua trace helpers (`trace.recording_steps`, Lua `trace.trace`) feed recorded host events.

- Add the deterministic C# core and native MonoGame host.
- Add a Debug-only live MCP bridge and locked stdio adapter.
- Package the C# core and host sources in GitHub releases.

## 0.1.2 (2026-10-11)

- Consolidate `nightrunner` and `unicycle` onto `main`, preserving both histories.
- Keep Python modules and tests in `python/`, Lua modules directly in `lua/`,
  shared replay contracts in `contract/`, and observability tools in the engine.
- Developer pause and stepped input work with Pyxel display pacing. Holds count
  simulation frames, and paused screenshots render the current game state.
- Lua replays retain recorded host events, including playback and frame exports.


### Core

- A game declares its own buttons above the 8 engine buttons: `Button(name, keys, pad)`, `declare(...)` for the
  bits, and `Game.buttons`. They join `Game.input_mask`, so recordings and the tracker handle them.
  `host.keys.bindings(buttons)` and `gamepad.xbox(..., buttons=...)` give their default keyboard and pad tables.
- Host events: text that reaches the game from outside the buttons (saved settings, a key pressed to rebind).
  `Input.events`, `Game.step(code, pointers, events)`, `InputTracker.feed(..., events)`, and `Recording.events` with
  `add(..., events)` and `events_at(frame)`. The JSON key `"events"` is sparse and absent when empty, so older files
  load. `play()` feeds them. `Driver.step(..., events=())` and `Driver.event(*events)`.
- `store.py`: `Store`, `FileStore` (one `<key>.json` per key, written atomically), `MemoryStore`, and `load_json`
  (a bad file gives the default and is moved aside to `<key>.bad.json`). `Game.write(key, text)` and `Game.writes`.
- `bindings.py`: `Binding` and `Bindings` for runtime rebinding: primaries and fixed aliases by name, `assign` with the
  trade rule, locked entries, reserved names, and `to_json` and `from_json` over the defaults. `Game.bindings`.
- The web build drops a local version label (`2.9.9+ne.2` loads `2.9.9`), so a game on a Pyxel fork still loads
  the stock runtime from the CDN.
- Display mode: `AppConfig.vsync`, `App.set_vsync(on)` (pyxel-ne's `pyxel.vsync`; nothing with stock Pyxel),
  `Renderer.alpha` and `Renderer.smooth`. The rules step at `fps`, one recorded input for each step.
- `pacing.py`: `Pacer` (steps and a blend value for each display frame, locked at 60, 120, 180, and 240 Hz, an
  accumulator otherwise) and `InputLatch` (input merged across display frames that run no step).
- `Game.listen`: while True, the host reports the next key or pad button pressed as `"press key:<NAME>"` or
  `"press pad:<NAME>"`.

### Host (pyxel)

- `App.post(event)` and `App.boot()` (events for frame 0 of a live run). While `game.listen` is True the App adds a
  `"press ..."` event for the first key or pad button pressed. Live events are recorded; a replay feeds them.
- `AppConfig.vendor`: `App.store` (`host/storage.open_store`: the OS data folder from `storage.data_dir`, or browser
  storage in a page and the Android WebView) saves `game.writes` after each live frame. `AppConfig.display_scale` goes to
  `pyxel.init`.
- The key table follows `game.bindings` when the game sets them (`keys.table`).
- `App.device`: the device used last (`"keyboard"`, `"pad"`, or `"pointer"`), for button hints.
- `keys.first_pressed(pads)`, `keys.key_names()`, `keys.pad_names(pad)`, and `keys.is_pad(code)`.
- `AudioManager.set_volume(music, sfx)` scales the music and effect channel gains from their values after `setup()`.
- `frames` runs Pyxel headless (no window) and feeds the recorded host events.



- Two languages: `rng.py` (`Rng`, xoshiro128** with splitmix32 seeding and FNV-1a string seeds) replaces
  `random.Random` in `Game.rng`, and gives the same numbers in Lua. `trace.py` hashes a game's snapshot each frame
  (CRC-32 of a canonical text), so a port can prove it plays the same. `contract/rng.json` holds the test vectors.
- `lua/`: a small Lua core for LÖVE ports (input bits, pointers, scenes, `Game`, targets, replays in the
  same JSON, the rng, traces, and `test.lua`, a test runner for plain LuaJIT). Unicycle's LÖVE build uses it.

- Controllers, Xbox first (`host/gamepad.py`): the shared `XBOX` layout, `xbox(pad, layout, shift)` key tables for
  pads 1-4, `merge`, and the left stick as the d-pad (`AppConfig.sticks`, on for pad 1 by default). `App` loads the
  bundled SDL_GameControllerDB (`host/gamepads/gamecontrollerdb.txt`, zlib license) through SDL's
  `SDL_GAMECONTROLLERCONFIG_FILE` hint, so about 2,000 more pads work. `AppConfig.pad_mappings` picks another file.

- Targets (`target.py`): `desktop`, `web`, and `android`, each with a size and controls (keyboard, mouse, touch)
  from `[tool.nightengine.targets.<name>]`. Builds write their target into the app (`nightengine_build`), and a
  source run reads `NIGHTENGINE_TARGET`. `Game(seed, target)` and `Game.step` drop the pointers the target does not
  take, so the desktop mouse no longer counts as a touch. A touch-only target takes the mouse as a finger.
- `Pointer.kind` (`PointerKind`: touch, pen, mouse), from the browser's `pointerType` and the desktop mouse.
  Recordings save the kind, and rows without one load as touch.
- Recordings save the target, and `play`, `autopilot.run`, `frames`, and `App` call `make_game(seed, target)`.
- `Game.screen_size` lets a game change its screen size while it runs (a wide screen setting). `App`, the frames
  command, and the page follow it (`platform.resize`, `bridge.resize`).
- The page no longer claims that touch, mouse, and keyboard all work, since a target may drop some of them.
- `web build|serve --target`, and the page takes the target's size. The APK builds as `android`, the desktop
  program as `desktop`.

## 0.0.1 (2026-10-05)

The first version, made with Unicycle! as the first game to ship on every target.

### Core

- Scenes and the scene stack, `Game` with a seeded RNG and sound cues, input tracking with press edges and repeat.
- Pointers (touch, pen, mouse) as recorded input: `Input.pointers`, and `Recording` keeps them.
- Recordings store `width` hex digits per frame, so a game can use input bits above the 8 buttons
  (`digits_for(Game.input_mask)`). Older files load.
- `Game.quit_requested` lets a scene quit. Content records, registries, canvases, sound checks, the autopilot shell,
  and the test `Driver`.

### Host (pyxel)

- `App`: keys, pointers, recording, replay, and audio. It saves the run on Q, on a quit request, and at exit.
- `AudioManager(layers=N)` plays several cues of one frame on their own channels.
- `platform`: the browser bridge, the desktop mouse, screen and safe-area info, browser storage, the Back button
  (button B), and closing a native shell.
- The renderer, UI helpers, image packing, and the frames command line.

### Web

- `python -m nightengine.web build|serve` packages any game from its `pyproject.toml`. `--offline` copies the
  Pyxel and Pyodide runtime into the build.
- The page fits the game to the window (`?shell=app` for a full-screen app). Pyxel never draws below 1x, so on a
  screen smaller than the game the launcher keeps the game's pixels and shrinks them with a CSS transform.
- `web/harness`: Playwright helpers, a config for games, and engine checks.

### Packages

- `python -m nightengine.android build`: a debug-signed APK, a WebView that serves the offline build.
- `python -m nightengine.desktop build`: a `pyxel app2exe` program for the current system, zipped.

### Tested

- Unit tests (114), bridge tests (Node), and Unicycle's Playwright checks in Chromium.
- Unicycle's APK on an Android 15 emulator (WebView 124): layout, touch, Back, and Quit.
