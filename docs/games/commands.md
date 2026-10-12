---
sidebar:
  order: 4
---

# Game commands

Every game exposes these `just` recipes with these names. Agents and skills use the
names without reading the `justfile`. A game adds its own recipes after them.

Today each game writes these recipes itself. The [Roadmap](../roadmap.md) moves
them into the engine.

## All runtimes

| Recipe | Does |
| --- | --- |
| `just setup` | Fetch the engine submodule and install the tools. |
| `just run` | Play the game. `--target android` plays the phone layout. |
| `just check` | Run the game tests and the engine tests. |
| `just lint` | Check lint and format. |
| `just fmt` | Fix the format. |
| `just autoplay <seed>` | Let the autopilot play and record `replays/auto-<seed>.json`. |
| `just replay <file>` | Watch a recording. |
| `just frames <file>` | Render a recording to PNG files, with no window. |
| `just golden` | Record the fixture replays and traces again, after a rule change on purpose. |
| `just release` | Build every release file into `dist/release`. |

## Pyxel

| Recipe | Does |
| --- | --- |
| `just web` | Build and serve the browser version. |
| `just web-build` | Build the browser files. `--offline` includes the runtime. |
| `just apk` | Build the Android APK. |
| `just exe` | Build the desktop program for this system. |

## LÖVE (prefix `love-` in a multi-runtime game)

| Recipe | Does |
| --- | --- |
| `just love` | Play the LÖVE build with the dev bridge. |
| `just love-check` | Run the Lua specs against the shared fixtures. |
| `just love-build [love\|windows\|macos\|android]` | Build the LÖVE files. |
| `just lovepilot` | Install the love2d-mcp bridge into `.tools/`. |

## MonoGame (prefix `mg-` in a multi-runtime game)

| Recipe | Does |
| --- | --- |
| `just mg-run` | Play the Debug build with the dev bridge. |
| `just mg-check` | Run the engine contract tests and the game tests. |
| `just mg-build <desktop\|android> [runtime]` | Build a release. |
| `just mg-mcp` | Install the monogame-mcp adapter into `.tools/`. |

A game with one runtime may drop the prefix.
