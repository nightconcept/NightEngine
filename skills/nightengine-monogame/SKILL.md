---
name: nightengine-monogame
description: Build, test and ship the C#/MonoGame side of a NightEngine game, and inspect it live through the monogame-mcp bridge. Use with the nightengine skill when a task touches monogame/ or a MonoGame build.
---

# NightEngine on MonoGame

Reference: `nightengine/docs/runtimes/csharp-monogame.md`.
Load the `nightengine` skill first.

## Setup

- `mise install` supplies the .NET SDK. `just mg-setup` adds the Android workload.
- The rules project references `nightengine/csharp/NightEngine.Core`.
  Platform projects compile `nightengine/csharp/MonoGame/*.cs` against MonoGame 3.8.6.

## Run and inspect

- `just mg-run` starts a Debug build with `--dev`, which opens the DevBridge on loopback port 12346.
- `just mg-mcp` installs the MCP adapter into `.tools/monogame-mcp`. The game's `.mcp.json`
  registers it. Its tools read state, capture the framebuffer, send input, run typed
  actions (pause, resume, step), watch state, list assets and reload assets.
  It cannot run arbitrary code, and it cannot change input during a replay.
- `just mg-frames <file>` renders a recording to PNG files.

## Code

- `<Game>.Rules` never references MonoGame. Drawing and sound live in `<Game>.MonoGame`.
- `just mg-check` runs the engine contract tests and the game tests. The game's
  recordings must give the same traces as Python and Lua.
- State hashes prove gameplay parity. They do not prove that the frames look right.
  Look at the PNG files after a visual change.

## Build

`just mg-build desktop <linux-x64|win-x64|osx-arm64>` or `just mg-build android`.
Release builds omit the DevBridge. Test Android launch, input, pause and resume,
background and return, and orientation in an emulator. Report real-phone tests separately.
