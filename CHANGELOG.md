# Changelog

## Unreleased

### Core

- A game declares its own buttons above the 8 engine buttons: `Button(name, keys, pad)`, `declare(...)` for the
  bits, and `Game.buttons`. They join `Game.input_mask`, so recordings and the tracker handle them.
  `host.keys.bindings(buttons)` and `gamepad.xbox(..., buttons=...)` give their default keyboard and pad tables.

## 0.0.1 (2026-10-07)

The first version, made with Unicycle! as the first game to ship on every target.

### Core

- Scenes and the scene stack, `Game` with a seeded RNG and sound cues, input tracking with press edges and repeat.
- Pointers (touch, pen, mouse) as recorded input: `Input.pointers`, and `Recording` keeps them.
- Recordings store `width` hex digits per frame, so a game can use input bits above the 8 buttons
  (`digits_for(Game.input_mask)`). Older files load.
- `Game.quit_requested` lets a scene quit. Content records, registries, canvases, sound checks, the autopilot shell,
  and the test `Driver`.

### Controllers and targets

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

- Unit tests (143), bridge tests (Node), and Unicycle's Playwright checks in Chromium.
- Unicycle's APK on an Android 15 emulator (WebView 124): layout, touch, Back, and Quit.
