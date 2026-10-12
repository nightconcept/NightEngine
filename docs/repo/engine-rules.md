---
sidebar:
  order: 1
---

# Engine rules

These rules apply to every change in `python/`, `lua/` and `csharp/`.

## Core limits

- The engine knows no game. Turn a difference between games into a parameter.
  Do not write `if game == ...`.
- The Python core imports only the standard library and `nightengine`.
- Only `python/src/nightengine/host/` imports `pyxel` or the browser module `js`.
- The core never imports `nightengine.host`.
- The core is deterministic. Use the engine RNG for simulation randomness.
  Do not read wall-clock time in the core.
- The Lua core runs on plain LuaJIT. Only `lua/host/` uses LÖVE.
- Games keep physics, terrain, scoring and scene decisions.

## Enforcement

- `python/tests/test_rules.py` parses every Python file with `ast` and checks the import limits.
- Host tests that stub `pyxel` are listed in `HOST_TESTS` in that file.
  If you add a host test that stubs `pyxel`, add it to that set.

## Contract changes

The contract is in [Compatibility](../concepts/contract.md) and [Layers and parity](../concepts/layers.md).
It includes the API names: Python names are canonical, and Lua and C# derive theirs. It covers button bits,
pointer kinds and phases, scene stack order, frame hook order, targets, RNG,
replay JSON and trace encoding.

1. Change the Python, Lua and C# implementations in the same commit.
2. If a simulation result changes, update the fixtures in `tests/fixtures/` on purpose.
3. Keep replay widths and recorded targets backward compatible.
4. Run `just check`. Both suites read the same fixtures.
5. Record the change in `CHANGELOG.md`.

## Parity

Every capability in a core, host or dev folder is listed in `parity.toml`.
When you add, move or rename one, update its entry and run `just parity`.
Decide the status for the other runtimes with the rules in
[Layers and parity](../concepts/layers.md).

## Development tools

- `app.dev` and the trace tools belong to the engine. Do not move them into a game.
- `lua/host/dev/` holds the licensed love2d-mcp transport. Keep its provenance and MIT license.
- The LÖVE builder excludes `lua/host/dev/` from release archives. Keep that exclusion.
- The bridge executes Lua. Use it for local development only.
