# Tests

- `lua.lua`: runs the Lua specs with plain LuaJIT.
- `lua/`: Lua core and host specs.
- `fixtures/`: shared data that the Python and Lua suites both check.
  - `rng.json`: RNG outputs per seed, plus canonical snapshot text and hash.
    `python -m nightengine.trace` writes it.
  - `behavior.json`: frame progression, held and pressed input, and cue clearing.
  - `events.json`: a sparse host event recording that must round-trip unchanged.

Python tests are in `python/tests/`. If a simulation result changes, update
the fixtures on purpose. See [Compatibility](../docs/compatibility.md).
