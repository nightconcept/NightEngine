---
sidebar:
  order: 4
---

# Runtime parity

This page shows which capability each runtime has, by layer. `parity.toml` is the
source. [Layers and parity](layers.md) explains the statuses, the checks and how to
decide whether a runtime catches up.

Change `parity.toml` in the same commit that adds or removes a capability, then run
`just parity` to rewrite this page.

<!-- parity:start -->

_Generated from `parity.toml` by `just parity`. Do not edit by hand._

### Contract

| Capability | Python/Pyxel | Lua/LÖVE | C#/MonoGame |
| --- | --- | --- | --- |
| `buttons`: The eight engine button bits: UP 1 … MENU 128 | Yes | Yes | Yes |
| `input-edges`: Held, pressed and repeat state from one int per frame | Yes | Yes | Yes |
| `step-order`: Game.step: filter, track, effects, before hook, top scene, after hook, frame + 1, return cues | Yes | Yes | Yes |
| `cues`: Sound cue names returned by each step and cleared after it | Yes | Yes | Yes |
| `scene-stack`: Push, pop, replace and the visible scenes under an overlay | Yes | Yes | Yes |
| `rng`: Portable seeded RNG | Yes | Yes | Yes |
| `trace`: Canonical snapshot text and CRC-32 digest | Yes | Yes | Yes |
| `replay-json`: Recording JSON: seed, width, inputs, sparse pointers and events | Yes | Yes | Yes |
| `events`: Host events: text input recorded per frame and fed on playback | Yes | Yes | Yes |
| `pointers`: Pointer kinds, phases and their replay encoding | Yes | Yes | Yes |
| `targets`: Targets and the pointer kinds that each accepts | Yes | Yes | Yes |

### Core

| Capability | Python/Pyxel | Lua/LÖVE | C#/MonoGame |
| --- | --- | --- | --- |
| `screen-size`: The game's logical screen size for its target | Yes | Yes | Yes |
| `fx`: Screen effects ticked by the step: shake and fade | Yes | Yes | Planned |
| `scene-music`: Each scene names its music track | Yes | Yes | Planned |
| `input-axis`: Axis and direction helpers on Input | Yes | Yes | Planned |
| `custom-buttons`: Game-declared buttons above the eight engine bits | Yes | Planned | Planned |
| `pointer-helpers`: Primary pointer, press and release, and hit tests | Yes | Yes | Planned |
| `target-tables`: Named targets, sizing and target JSON | Yes | Yes | Planned |
| `replay-play`: Play a recording into a game; replay widths | Yes | Yes | Planned |
| `trace-tools`: Trace a recording and find the first differing frame | Yes | Yes | Planned |
| `test-driver`: Headless driver for tests: step, tap, press, run until | Yes | Yes | Planned |
| `store`: Settings and save storage through Game.write | Yes | Planned | Planned |
| `bindings`: Runtime rebinding of keys and pad buttons | Yes | Planned | Planned |
| `content`: JSON data records, reference checks and the handler registry | Yes | Planned | Planned |
| `systems`: Ordered per-frame systems on a scene | Yes | Planned | Planned |
| `autopilot`: A bot that plays a seed and records the run | Yes | Planned | Planned |
| `pacing`: Simulation steps paced against a vsync display | Yes | Native | Native |
| `canvas`: Code-drawn pixel art and palettes for Pyxel image banks | Yes | None | None |
| `sound-check`: Validation of Pyxel sound tables | Yes | None | None |

### Host

| Capability | Python/Pyxel | Lua/LÖVE | C#/MonoGame |
| --- | --- | --- | --- |
| `loop`: Fixed frame loop: read input, step, render, play cues | Yes | Yes | Yes |
| `input`: Keyboard, touch and mouse into one int and pointers | Yes | Yes | Yes |
| `gamepad`: Gamepads with a shared controller database | Yes | Yes | Planned |
| `record-replay`: Record live input; play a recording back | Yes | Yes | Yes |
| `render`: Draw dispatch to the game's renderer | Yes | Yes | Yes |
| `audio`: Play cues and scene music | Yes | Yes | Yes |
| `viewport`: Letterbox the game screen and map window points to game points | Yes | Yes | Yes |
| `frames`: Render a recording to PNG files with no window | Yes | Yes | Yes |
| `storage`: Where settings live on desktop, browser and phone | Yes | Planned | Planned |
| `platform-info`: Safe area, touch device and screen size hints | Yes | Planned | Planned |
| `pyxel-helpers`: Pyxel text, sprite and image-bank helpers | Yes | None | None |

### Dev

| Capability | Python/Pyxel | Lua/LÖVE | C#/MonoGame |
| --- | --- | --- | --- |
| `state`: Read frame, scene, pause flag, state hash and game inspection | Yes | Yes | Yes |
| `pause-step`: Pause, resume and step n simulation frames | Yes | Yes | Yes |
| `hold`: Inject buttons through the normal, recorded input path | Yes | Yes | Yes |
| `shot`: Save the rendered game pixels to PNG | Yes | Yes | Yes |
| `save`: Save the live recording | Yes | Yes | Yes |
| `mcp-bridge`: Live MCP bridge for agents | None | Yes | Yes |
| `asset-reload`: List and reload built assets in a running game | None | None | Yes |
| `pointer-diagnostic`: Browser page that shows raw pointer input (?app=debug) | Yes | None | None |

### Catch-up work and open decisions

- `core.fx` on C#/MonoGame: planned. Not ported yet.
- `core.scene-music` on C#/MonoGame: planned. Not ported yet.
- `core.input-axis` on C#/MonoGame: planned. Not ported yet.
- `core.custom-buttons` on Lua/LÖVE: planned. Games pass a wider mask by hand.
- `core.custom-buttons` on C#/MonoGame: planned. Games pass a wider mask by hand.
- `core.pointer-helpers` on C#/MonoGame: planned. Not ported yet.
- `core.target-tables` on C#/MonoGame: planned. Recording.FromJson reads targets inline.
- `core.replay-play` on C#/MonoGame: planned. Hosts and tools loop over frames by hand.
- `core.trace-tools` on C#/MonoGame: planned. Unicycle.Tools traces by hand.
- `core.test-driver` on C#/MonoGame: planned. C# tests drive Game.Step directly.
- `core.store` on Lua/LÖVE: planned. Engine core helper.
- `core.store` on C#/MonoGame: planned. Engine core helper.
- `core.bindings` on Lua/LÖVE: planned. Engine core helper.
- `core.bindings` on C#/MonoGame: planned. Engine core helper.
- `core.content` on Lua/LÖVE: planned. Engine core helper.
- `core.content` on C#/MonoGame: planned. Engine core helper.
- `core.systems` on Lua/LÖVE: planned. Engine core helper.
- `core.systems` on C#/MonoGame: planned. Engine core helper.
- `core.autopilot` on Lua/LÖVE: planned. Engine core helper.
- `core.autopilot` on C#/MonoGame: planned. Engine core helper.
- `host.gamepad` on C#/MonoGame: planned. Unicycle reads the pad in its own host.
- `host.storage` on Lua/LÖVE: planned. Follows core.store.
- `host.storage` on C#/MonoGame: planned. Follows core.store.
- `host.platform-info` on Lua/LÖVE: planned. LÖVE exposes the safe area; no engine API yet.
- `host.platform-info` on C#/MonoGame: planned. No engine API yet.

### Contract debts

- `contract.input-edges`: The fixture has no held button long enough to fire a repeat.
- `contract.step-order`: C# has no effects to tick (see core.fx). The fixture uses no effects.
- `contract.scene-stack`: No shared fixture. Each suite tests the stack on its own.
- `contract.pointers`: No shared fixture. Replays with pointers are not checked across runtimes.
- `contract.targets`: No shared fixture for pointer filtering by target.

<!-- parity:end -->
