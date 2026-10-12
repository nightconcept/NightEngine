---
name: nightengine-love
description: Build, test and ship the Lua/LÖVE side of a NightEngine game, and inspect it live through the love2d-mcp bridge. Use with the nightengine skill when a task touches love/ or a LÖVE build.
---

# NightEngine on LÖVE

Reference: `nightengine/docs/runtimes/lua-love.md`.
Load the `nightengine` skill first.

## Setup

- `mise install` supplies LÖVE 11.5, LuaJIT and StyLua.
- `love/nightengine` is a link to `../nightengine/lua`. Code uses `require("nightengine.<module>")`.

## Run and inspect

- `just run --runtime love` plays the LÖVE build. `just dev --runtime love key=value ...` sets
  `NIGHTENGINE_DEV=1`, which opens the bridge on `127.0.0.1:12345`.
- `python3 nightengine/tools/love2d-mcp/setup.py --out .tools/love2d-mcp` installs the love2d-mcp server into `.tools/love2d-mcp`. The game's
  `.mcp.json` registers it. Its `run_lua` environment has `ne`, the dev controls.
- `just frames <file> --runtime love` renders a recording to PNG files.
- Under a tiling window manager, float the window. Otherwise the game letterboxes and
  screenshots are not the game size.

## Code

- Rules in `love/<game>/` use only plain LuaJIT and the engine core. No `love.*`.
- The specs run on plain LuaJIT and replay the shared fixtures. `just check` runs them.
  The traces must match the Python traces.
- `just fmt` formats with StyLua, and `just lint` checks it.

## Build

`just build love windows macos android` (any of them). With no argument it builds all that
this system can. macOS builds need a Mac. Android needs Java and the SDK build tools.
Release archives never include `nightengine/host/dev/`, the bridge that runs Lua.
