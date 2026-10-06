# Changelog

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
