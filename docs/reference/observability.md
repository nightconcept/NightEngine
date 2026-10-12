# Observability

Both hosts expose `app.dev`:

| Method | Result |
| --- | --- |
| `state()` | Frame, scene, paused flag, game inspection and optional state hash |
| `hash()` | Canonical state hash; requires a snapshot callback |
| `pause()` / `resume()` | Stop or resume simulation; window updates continue |
| `step(n)` | Schedule n simulation frames while paused |
| `hold(code, frames)` | Inject buttons through ordinary input and recording |
| `shot(path)` | Save the rendered game pixels to PNG |
| `save(path)` | Save the live recording |

Pass `inspect(game)` for readable fields and `snapshot(game)` for canonical state.
Input injection is rejected during replay. Use positive integer frame counts.

## Python

Pass callbacks to `nightengine.host.app.App`. Browser checks use the packaged
Playwright harness to access `nightengine.host.app.current` through Pyodide:
`python(page, "app.dev.state()")`. Controls remain in process.
The harness, touch helpers, pointer diagnostic (`?app=debug`), frame exporter and
trace tools belong to the engine.

## Lua / love2d-mcp

Pass `--dev` (as `just dev` does) or set `NIGHTENGINE_DEV=1` for source runs. The engine host loads the vendored bridge
on `127.0.0.1:12345`. Its `run_lua` environment contains `ne`, the same controls
and game-supplied helpers.

```sh
python3 nightengine/tools/love2d-mcp/setup.py --out .tools/love2d-mcp
NIGHTENGINE_DEV=1 love love
```

Use `tools/love2d-mcp/mcp.json` for MCP settings. Bootstrap pins commit `160ebfe`
of [love2d-mcp](https://forge.solivan.dev/nightconcept/love2d-mcp), our fork of
LovePilot, before `npm ci`. The fork's lock updates the MCP SDK, proxy-addr,
fast-uri and ip-address to fix four upstream dependency audit findings.
Bridge provenance and MIT
license ship with the engine. Game release archives omit the executable bridge.
The bridge can execute Lua and is for local development.

Trace and recording tools work without a window. Screenshot export saves game
pixels independently of window letterboxing. Float the game window under a
tiling window manager when testing actual window placement.

Pyxel display pacing counts developer holds in simulation frames. Pause and manual
steps reset the pacer, so resume does not run accumulated steps. Paused captures
render the current state even when the display frame ran no simulation step.
