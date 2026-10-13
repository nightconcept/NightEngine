# NightEngine

Nightcircle's game engine supports Python/Pyxel, Lua/LÖVE and C#/MonoGame. All implement the
same deterministic frame, input, RNG, replay and trace contracts. Their hosts own
runtime loops, recordings and development controls. Games supply rules, scenes,
graphics, sounds and state callbacks.

| Directory | Purpose |
| --- | --- |
| `python/src/nightengine/` | Python core, Pyxel host, browser harness and builders |
| `python/tests/` | Python core, host and packaging checks |
| `lua/` | Flat Lua package root, core and LÖVE host |
| `tests/lua/` | Lua core and host checks |
| `tests/fixtures/` | Shared compatibility fixtures |
| `csharp/` | C# core, MonoGame host sources and contract tests |
| `skills/` | Agent skills that games link |
| `docs/`, `site/` | Documentation and the site that renders it |
| `tools/` | Pinned MCP bootstraps for the LÖVE and MonoGame bridges |
| `examples/` | Runnable examples for both implementations |

## Develop

```sh
mise install
uv sync --project python
just check
just lint
```

`mise install` supplies Python, uv, ruff, ty, LuaJIT, StyLua, LÖVE, Java and just
on Linux x64, macOS arm64 and Windows x64. Use `just format` and `just typecheck`
for formatting and ty type checks. GitHub is the primary repository; a workflow
mirrors it to [Forgejo](https://forge.solivan.dev/nightconcept/NightEngine). CI checks both implementations on all three platforms
and builds the Python distribution and both example archives. Engine version:
0.2.0. Games pin the engine commit or release deliberately.

- [Documentation Map](docs/index.md)
- [Vision](docs/vision.md) and [Roadmap](docs/roadmap.md)
- [Use the engine in a game](docs/games/consume.md)
- [Runtime parity](docs/concepts/parity.md) and [Platforms](docs/concepts/platforms.md)

Unicycle and Nightrunner are integration games. Shared development starts from
`main`; games pin verified engine commits. Nightrunner preserves its existing
Pyxel fork, seeded randomness, settings and display pacing. See the
[Nightrunner API reference](docs/reference/nightrunner-api.md).

Unicycle's GitHub release builds exercise Pyxel web,
Android, Linux and Windows, plus LÖVE archives, Windows, macOS and Android.

## License

[MIT](LICENSE). `python/src/nightengine/host/gamepads/` keeps the SDL zlib license in its folder.
