---
name: nightengine-pyxel
description: Build, test and ship the Python/Pyxel side of a NightEngine game, including web, Android and desktop builds and the pyxel-ne fork. Use with the nightengine skill when a task touches Python game code or a Pyxel build.
---

# NightEngine on Pyxel

API reference: `nightengine/docs/runtimes/python-pyxel.md`.
Load the `nightengine` skill first.

## Run and inspect

- `just run`. `just run --target android` plays the touch layout with the mouse as a finger.
- `just replay <file>` watches a recording. `just frames <file>` writes PNG files with no window.
- In a running game, `app.dev` gives `state()`, `pause()`, `step(n)`, `hold(code, frames)`
  and `shot(path)`. See `nightengine/docs/reference/observability.md`.
- To observe a single script headless, use the pyxel-mcp tools (`run`, `read_image`).

## Code

- A scene subclasses `nightengine.Scene`. It has one draw function in `render/`.
- New content: a JSON record in `data/`, plus a registered handler if it needs new behaviour.
  Use `check_refs` and `Registry.missing` in the content test.
- Only `render/`, `app.py`, `audio.py` and `frames.py` import `pyxel`.
  `tests/test_layers.py` checks this.

## Build

| Target | Command |
| --- | --- |
| Browser | `just web` (serve), `just web-build` (files). Add `--host 0.0.0.0` to test on a phone. |
| Android | `just apk`. Needs JDK 17+ and `ANDROID_HOME`. A WebView shell around the offline web build. |
| Desktop | `just exe`. Builds for the system you are on. |

After a browser or touch change, run the Playwright checks (`just web-test` where the game has it).

## pyxel-ne

Some games pin the pyxel-ne fork (`pyxel==<version>+ne.<n>`) for screen shaders and
vsync pacing. Check `getattr(pyxel, "NE", False)` before you use a fork call. The
browser build uses stock Pyxel at the same base version, so fork calls must be optional.
To add a feature, change the fork, not the Pyxel API. See `NE.md` in the fork.
