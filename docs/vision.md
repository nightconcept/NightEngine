---
sidebar:
  order: 1
---

# Vision

This page is the charter for NightEngine. When a change conflicts with this page,
change this page first, on purpose, or do not make the change.

## What NightEngine is

NightEngine is the single engine entry point for Nightcircle, an indie game studio.
Nightcircle is polyglot and AI native. Agents and people build games together, and
agents do most of the reading.

NightEngine gives a game project everything it needs to go from game code to a
shipped build:

- A deterministic core for each runtime: frames, input, RNG, replay and trace.
- A thin host for each runtime that turns the core into windows, sound and builds.
- Builders for each output platform.
- Folder and naming conventions, so a game needs little configuration.
- Agent skills for each runtime.
- A documentation site for agents and people, built from `docs/`.

A game is a thin layer on top: its data, rules, scenes, art and sound.

## Three runtimes, one contract

NightEngine supports three runtimes. Each step up gives more features and more cost.

| Runtime | Language | Use it for |
| --- | --- | --- |
| [Pyxel](runtimes/python-pyxel.md) | Python | Proofs of concept and small retro games. Fastest to iterate. |
| [LÖVE](runtimes/lua-love.md) | Lua | Games that need more than Pyxel gives: free resolution, shaders, larger assets. |
| [MonoGame](runtimes/csharp-monogame.md) | C# | Games that need the most features, performance and the .NET toolset. |

All three implement the same [contract](concepts/contract.md). One recording plays
the same on every runtime and gives the same trace. A game can start on Pyxel and
move up the ladder. The traces prove that the port keeps the rules. Unicycle did
this from Python to Lua to C#.

See [Choosing a runtime](concepts/runtimes.md).

### Pyxel and pyxel-ne

Pyxel is the proof-of-concept runtime. Nightcircle keeps the full Pyxel API intact.
When a game needs more from Pyxel, add the feature to the
[pyxel-ne fork](https://github.com/nightconcept/pyxel) as a small, marked change on
top of upstream Pyxel. Do not fork the Pyxel API. Today pyxel-ne adds screen
shaders, display capture and vsync pacing.

## Target platforms

| Platform | Architecture |
| --- | --- |
| Linux | x64 |
| macOS | arm64 only |
| Windows | x64 |
| Android | arm64 |
| iOS | arm64 |

[Platforms](concepts/platforms.md) shows which runtime builds which platform today.

## A versioned dependency

A game does not copy the engine. A game adds this repository as a git submodule at
`nightengine/` and pins a verified commit or release tag. The game moves the pin
deliberately. See [Use the engine in a game](games/consume.md).

## Convention over configuration

Every game uses the same folder layout, file names and command names for its
runtime. The engine reads that layout, so a game configures only what is different.
See [Game layout](games/layout.md) and [Game commands](games/commands.md).

## AI native

Agents are first-class users of NightEngine. So:

- Every workflow has a [skill](games/skills.md) that the engine ships.
- Every check is a command with a pass or fail result.
- Every visual change can be seen: PNG frames from replays, and live MCP bridges
  for LÖVE and MonoGame.
- The docs are the single source of truth for agents and people. The site and the
  agent hub files link to the same pages.

## Non-goals

- The engine knows no game. A difference between games becomes a parameter.
- The engine has no editor.
- The engine does not own physics, terrain, scoring or scene decisions. Games do.
