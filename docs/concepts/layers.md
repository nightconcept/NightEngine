---
sidebar:
  order: 3
---

# Layers and parity

Every runtime builds the engine in the same layers. The code in each language is
different. The layers, their boundaries and their capabilities are the same.
`parity.toml` lists every capability and its status in each runtime.
`just parity` checks the list against the code. `just check` runs that check.

## The layers

| Layer | What it is | Same in every runtime | Proof |
| --- | --- | --- | --- |
| **Contract** | Data formats and algorithms: button bits, input edges, step order, RNG, trace, replay JSON, pointers, targets | Exactly. Byte for byte. | Shared fixtures in `tests/fixtures/`, read by all three test suites |
| **Core** | The deterministic simulation API on top of the contract: scenes, effects, content, storage, testing | The behaviour and the names of the concepts | `parity.toml` symbols, and each suite's own tests |
| **Host** | The runtime adapter: frame loop, input capture, record and replay, draw and sound dispatch, letterbox, PNG frames | The responsibilities | `parity.toml` symbols and host tests |
| **Dev** | Tools for agents and people: dev controls, MCP bridges, trace tools | The operations: state, hash, pause, step, hold, shot, save | `parity.toml` symbols |
| **Delivery** | Builders for each platform | The platform list | [Platforms](platforms.md) and integration game releases |

A layer depends only on the layers above it in this table. The contract and core
never import a runtime. Each suite has a boundary test that checks this:
`python/tests/test_rules.py`, the plain-LuaJIT spec run, and
`csharp/NightEngine.Tests/BoundaryTests.cs`.

## The manifest

Each capability in `parity.toml` has an id (`layer.name`), a summary, and one entry
for each runtime:

| Status | Meaning |
| --- | --- |
| `yes` | Implemented. `at` names the file and symbol, for example `core.lua:Game.step`. The check fails if the symbol is gone. |
| `planned` | This runtime will catch up. The reason says when. |
| `native` | The framework already does it, so the engine adds no code. |
| `none` | This runtime deliberately does not have it. |
| `undecided` | Not decided yet. `since` records the engine version. |

The check fails when:

1. A `yes` points at a file or symbol that does not exist.
2. A source file in a core, host or dev folder belongs to no capability. New code
   must declare what it is, and so it forces a parity decision.
3. A contract capability is not `yes` in every runtime.
4. A contract capability has neither a shared fixture nor a recorded debt, or a
   runtime's test suite does not read its fixture.
5. The ladder rule fails (see below).
6. An `undecided` status is older than the engine version. Decide before the next release.
7. `docs/concepts/parity.md` does not match the manifest.

## Should the other runtimes catch up?

When a capability is added to one runtime, record a status for the other two in the
same commit. Choose with these rules, in order:

1. **Contract: always catch up.** Change all three runtimes and the fixtures in one
   commit. A contract change that only one runtime has is a broken engine.
2. **Up the ladder: catch up.** The ladder is Pyxel, then LÖVE, then MonoGame. Games
   move up it. A capability that a lower runtime has must exist higher up, so a game
   can move without losing features. Use `planned`, or `native` if the framework
   gives it. The check rejects `none` above a `yes`.
3. **Down the ladder: catch up only on demand.** Higher runtimes have more features
   by design. Port a feature down only when a game on the lower runtime needs it.
   Until then, use `none` with a reason.
4. **Runtime-specific: never catch up.** Some capabilities exist only because of one
   runtime, for example Pyxel image banks or the browser pointer page. Mark the
   capability `specific = true` and the others `none`.
5. **Not sure: `undecided`.** Record the reason and the engine version. The decision
   must be made before the next release. Make it with the game that needs it.

When you choose `planned`, add the work to the [Roadmap](../roadmap.md) or an
execution plan if it is more than one commit.

## Contract debts

A debt is a contract capability that the shared fixtures do not fully prove. Each
runtime may test it alone, but nothing checks that the three agree. The parity page
lists the debts. Pay one off by adding a fixture that all three suites read.
