# NightEngine

Nightcircle's game engine supports Python/Pyxel and Lua/LÖVE. Both implement the
same deterministic frame, input, RNG, replay and trace contracts. Their hosts own
runtime loops, recordings and development controls. Games supply rules, scenes,
graphics, sounds and state callbacks.

| Directory | Purpose |
| --- | --- |
| `python/src/nightengine/` | Python core, Pyxel host, browser harness and builders |
| `python/tests/` | Python core, host and packaging checks |
| `lua/` | Flat Lua package root, core and LÖVE host |
| `tests/lua/` | Lua core and host checks |
| `contract/` | Shared compatibility fixtures |
| `tools/love2d-mcp/` | Pinned MCP server bootstrap and connection example |
| `examples/` | Runnable examples for both implementations |

## Develop

```sh
mise install
uv sync --project python
just check
just lint
```

Lua checks need LuaJIT; formatting needs StyLua. CI checks both implementations
and builds the Python distribution and both example archives. Engine version:
0.1.2. Games pin the engine commit or release deliberately.

- [Architecture](docs/architecture.md)
- [Compatibility](docs/compatibility.md)
- [Python/Pyxel](docs/python-pyxel.md)
- [Lua/LÖVE](docs/lua-love.md)
- [Observability](docs/observability.md)

Unicycle and Nightrunner are integration games. Shared development starts from
`main`; games pin verified engine commits. Nightrunner preserves its existing
Pyxel fork, seeded randomness, settings and display pacing. See the
[Nightrunner API reference](docs/nightrunner-api.md).

Unicycle's GitHub release builds exercise Pyxel web,
Android, Linux and Windows, plus LÖVE archives, Windows, macOS and Android.
