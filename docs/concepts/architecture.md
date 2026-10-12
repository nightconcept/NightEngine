---
sidebar:
  order: 1
---

# Architecture

Each implementation has a deterministic core and a runtime host. Python, Lua and
C# implement the same behavior separately. No core imports its runtime.

`Game.step(code, pointers)` filters target input, tracks button edges and repeats,
ticks effects, runs the before-scene hook, updates the top scene, runs the
after-scene hook, and increments the frame. It returns sound cue names.
The host records or replays input, steps the game, renders and plays sound.
Games configure frame rate, bindings, rendering and audio. Simulation randomness
uses the engine RNG. Games retain physics, terrain, scoring and scene decisions.

Python lives in `python/src/nightengine/`; Lua's package root is `lua/` directly;
C# lives in `csharp/NightEngine.Core/`. The Python and Lua hosts live under each
implementation's `host/`. The MonoGame host sources live in `csharp/MonoGame/`. Python tooling also builds
LÖVE games with `python -m nightengine.love build`.

Development controls live beside the hosts. Snapshot and inspection callbacks
keep game knowledge out of the engine. The licensed Lua MCP transport lives in
`lua/host/dev/`; release game archives exclude that directory.

A game checks out this repository at `nightengine/`, installs `nightengine/python`,
links `love/nightengine` to `../nightengine/lua` and references
`nightengine/csharp/NightEngine.Core`. See [Use the engine in a game](../games/consume.md). Python imports and Lua
requires retain their public `nightengine` names.
