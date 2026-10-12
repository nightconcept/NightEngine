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
| `rng`: Portable seeded RNG: randint and getrandbits | Yes | Yes | Yes |
| `trace`: Canonical snapshot text and CRC-32 hash | Yes | Yes | Yes |
| `replay-json`: Recording JSON: seed, width, inputs, sparse pointers and events | Yes | Yes | Yes |
| `events`: Host events: text input recorded per frame and fed on playback | Yes | Yes | Yes |
| `pointers`: Pointer kinds, phases and their replay encoding | Yes | Yes | Yes |
| `targets`: Named targets and the controls that each accepts | Yes | Yes | Yes |

### Core

| Capability | Python/Pyxel | Lua/LÖVE | C#/MonoGame |
| --- | --- | --- | --- |
| `fx`: Screen effects ticked by the step: shake and flash | Yes | Yes | Planned |
| `scene-music`: Each scene names its music track; the host plays it | Yes | Yes | Planned |
| `custom-buttons`: Game-declared buttons above the eight engine bits | Yes | **Undecided** | **Undecided** |
| `bindings`: Runtime rebinding of keys and pad buttons | Yes | **Undecided** | **Undecided** |
| `store`: Settings and save storage through Game.write | Yes | **Undecided** | **Undecided** |
| `content`: JSON data records, reference checks and the handler registry | Yes | **Undecided** | **Undecided** |
| `systems`: Ordered per-frame systems on a scene | Yes | **Undecided** | **Undecided** |
| `pacing`: Fixed simulation rate decoupled from the display rate | Yes | Native | Native |
| `test-driver`: Headless driver: step, tap, press and run_until in tests | Yes | Yes | **Undecided** |
| `autopilot`: A bot that plays a seed and records the run | Yes | **Undecided** | **Undecided** |
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
| `storage`: Where settings live on desktop, browser and phone | Yes | **Undecided** | **Undecided** |
| `platform-info`: Safe area, touch device and screen size hints | Yes | **Undecided** | **Undecided** |
| `pyxel-helpers`: Pyxel text, sprite and image-bank helpers | Yes | None | None |

### Dev

| Capability | Python/Pyxel | Lua/LÖVE | C#/MonoGame |
| --- | --- | --- | --- |
| `state`: Read frame, scene, pause flag and game inspection | Yes | Yes | Yes |
| `hash`: Canonical state hash of the live game | Yes | Yes | Planned |
| `pause-step`: Pause, resume and step n simulation frames | Yes | Yes | Yes |
| `hold`: Inject buttons through the normal, recorded input path | Yes | Yes | Yes |
| `shot`: Save the rendered game pixels to PNG | Yes | Yes | Yes |
| `save`: Save the live recording | Yes | Yes | Yes |
| `mcp-bridge`: Live MCP bridge for agents | None | Yes | Yes |
| `asset-reload`: List and reload built assets in a running game | None | None | Yes |
| `trace-tools`: Trace a recording and find the first differing frame | Yes | Yes | **Undecided** |
| `pointer-diagnostic`: Browser page that shows raw pointer input (?app=debug) | Yes | None | None |

### Open decisions and catch-up work

- `core.fx` on C#/MonoGame: planned. Ladder rule. Port ScreenFx when a MonoGame game needs it.
- `core.scene-music` on C#/MonoGame: planned. Ladder rule.
- `core.custom-buttons` on Lua/LÖVE: undecided. A game can pass a wider mask, but there is no declare().
- `core.custom-buttons` on C#/MonoGame: undecided. The Game constructor takes a mask, but there is no declare().
- `core.bindings` on Lua/LÖVE: undecided. Only Nightrunner uses rebinding.
- `core.bindings` on C#/MonoGame: undecided. Only Nightrunner uses rebinding.
- `core.store` on Lua/LÖVE: undecided. Games on LÖVE keep no settings yet.
- `core.store` on C#/MonoGame: undecided. Games on MonoGame keep no settings yet.
- `core.content` on Lua/LÖVE: undecided. Unicycle keeps its numbers in code.
- `core.content` on C#/MonoGame: undecided. Unicycle keeps its numbers in code.
- `core.systems` on Lua/LÖVE: undecided. Not used by a LÖVE game yet.
- `core.systems` on C#/MonoGame: undecided. Not used by a MonoGame game yet.
- `core.test-driver` on C#/MonoGame: undecided. C# tests drive Game.Step directly.
- `core.autopilot` on Lua/LÖVE: undecided. Unicycle records with the Python autopilot.
- `core.autopilot` on C#/MonoGame: undecided. Unicycle has its own C# autoplay tool.
- `host.gamepad` on C#/MonoGame: planned. Unicycle reads the pad in its own host. Move it into Host.cs.
- `host.storage` on Lua/LÖVE: undecided. Follows core.store.
- `host.storage` on C#/MonoGame: undecided. Follows core.store.
- `host.platform-info` on Lua/LÖVE: undecided. LÖVE exposes safe area natively; no engine API yet.
- `host.platform-info` on C#/MonoGame: undecided. No engine API yet.
- `dev.hash` on C#/MonoGame: planned. Ladder rule. Add a hash action to the bridge.
- `dev.trace-tools` on C#/MonoGame: undecided. Unicycle.Tools traces; the engine has no first_difference.

### Contract debts

- `contract.input-edges`: The fixture has no held button long enough to fire a repeat.
- `contract.step-order`: C# has no effects to tick (see core.fx). The fixture uses no effects.
- `contract.scene-stack`: No shared fixture. Each suite tests the stack on its own.
- `contract.pointers`: No shared fixture. Replays with pointers are not checked across runtimes.
- `contract.targets`: No shared fixture for pointer filtering by target.

<!-- parity:end -->
