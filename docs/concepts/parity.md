---
sidebar:
  order: 5
---

# Runtime parity

This matrix shows which engine features each runtime has. The shared rows are the
[contract](contract.md). Fixtures in `tests/fixtures/` prove them. The other rows
are runtime features with no shared fixture.

Change this page in the same commit that adds or removes a feature.

## Shared contract

| Feature | Python | Lua | C# |
| --- | --- | --- | --- |
| `Game.step`, frame counter and hook order | Yes | Yes | Yes |
| Scene stack | Yes | Yes | Yes |
| Button bits, pressed edges and repeats | Yes | Yes | Yes |
| Pointers (touch and mouse) | Yes | Yes | Yes |
| Host events | Yes | Yes | Yes |
| Targets and control filters | Yes | Yes | Yes |
| Portable RNG | Yes | Yes | Yes |
| Replay JSON | Yes | Yes | Yes |
| Canonical trace and hash | Yes | Yes | Yes |
| Sound cues | Yes | Yes | Yes |

## Runtime features

| Feature | Python | Lua | C# |
| --- | --- | --- | --- |
| Screen effects (`fx`) | Yes | Yes | No |
| Game-declared buttons above the 8 engine bits | Yes | No | No |
| Runtime rebinding (`bindings`) | Yes | No | No |
| Settings and save storage (`store`) | Yes | No | No |
| Data records and handler registry (`content`, `registry`) | Yes | No | No |
| Code-drawn art and palettes (`canvas`, `palette`) | Yes | No | No |
| Autopilot | Yes | No | No |
| Display pacing (pyxel-ne vsync) | Yes | No | No |
| Gamepad database | Yes | No | No |
| `app.dev` controls (pause, step, hold, shot) | Yes | Yes | Through the bridge |
| Live MCP bridge | No (use pyxel-mcp) | Yes | Yes |
| PNG frames from a replay | Yes | Yes | Yes |

The [Roadmap](../roadmap.md) tracks which runtime features join the contract.
