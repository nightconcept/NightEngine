---
sidebar:
  order: 3
---

# Layers and parity

Every runtime builds the engine in the same layers. The code in each language is
different. The borders between the layers, and what each layer promises, are the
same. `parity.toml` lists every capability. `just parity` checks the list against
the code, and `just check` runs that check.

## The layers

| Layer | What it is | Same in every runtime | Proof |
| --- | --- | --- | --- |
| **Contract** | Data, algorithms and their API: button bits, input edges, step order, scenes, RNG, trace, replay JSON, pointers, targets | The data byte for byte, and the API names | Shared fixtures in `tests/fixtures/`, and the derived names |
| **Core** | Engine helpers on top of the contract: effects, content and registry, storage, rebinding, systems, autopilot, test driver | The API names. Every runtime gets every helper in time. | The derived names, and each suite's own tests |
| **Host** | The runtime adapter: frame loop, input capture, record and replay, draw and sound, letterbox, PNG frames | The responsibilities. Names follow the runtime. | `file:Symbol` locations in `parity.toml` |
| **Dev** | Tools for agents: state, hash, pause, step, hold, shot, save, MCP bridges | The operations. Names follow the runtime. | `file:Symbol` locations in `parity.toml` |
| **Delivery** | Builders for each platform | The platform list | [Platforms](platforms.md) and integration game releases |

The contract and the core never import a runtime. Each suite has a boundary test.

## Canonical names

Python names are canonical for the contract and the core. The other runtimes derive
their names from them:

| Python | Lua | C# |
| --- | --- | --- |
| `rng.Rng.randint` | `Rng:randint` in `rng.lua` | `Rng.Randint` |
| `trace.digest` | `M.digest` in `trace.lua` | `Trace.Digest` (module becomes a static class) |
| `inputs.UP` | `M.UP` in `inputs.lua` | `Inputs.Up` |
| `replay.Recording.from_json` | `from_json` in `replay.lua` | `Recording.FromJson` |

`parity.toml` lists names only in their Python form. The check derives and finds the
others, so a rename in one runtime fails the check at once.

## What a game can expect

A game that moves up the ladder (Pyxel, then LÖVE, then MonoGame) is promised the
**contract**: its recordings play the same and give the same traces. Core helpers
reach every runtime in time, but a game may have to wait for one, or port its own
use of it. Host and dev tools differ by runtime.

## Statuses

Contract capabilities have no statuses: every runtime implements them. Core, host
and dev capabilities have one status for each runtime:

| Status | Meaning |
| --- | --- |
| `yes` | Implemented and found by the check. |
| `planned` | Will catch up. `plan` names the document that holds the work. |
| `native` | The framework already does it, so the engine adds no code. |
| `none` | Deliberately absent. In the core, only for a `specific` capability that exists because of one runtime. |
| `undecided` | Not decided yet. |

## Should the other runtimes catch up?

When you add a capability to one runtime, give the other two a status in the same commit.

1. **Contract: always, in the same commit.** Change the three runtimes and the fixtures together.
2. **Core: yes, in time.** Core helpers belong to the engine. Use `planned` with a plan.
   Use `none` only when the capability exists because of one runtime, and mark it `specific`.
3. **Host and dev: the responsibility, in the runtime's own way.** Use `planned`,
   `native` or `none` with a reason.
4. **Not sure which layer it is in: `undecided`.** Decide before the next release.

## When drift blocks

| Problem | Day to day (`just check`) | Release tag (`--release`) |
| --- | --- | --- |
| A contract name, fixture or runtime is missing | Fails | Fails |
| A `yes` points at code that does not exist | Fails | Fails |
| A core, host or dev source file belongs to no capability | Fails | Fails |
| A core capability is `none` without `specific` | Fails | Fails |
| The parity page does not match `parity.toml` | Fails | Fails |
| A status is `undecided` | Warning | Fails |
| A `planned` status has no plan, or its plan file is missing | Warning | Fails |

CI runs `just parity --release` on `v*` tags. Gaps can ship in a release when each one
is decided and planned.

## Contract debts

A debt is a contract capability that the shared fixtures do not fully prove. Each
runtime may test it alone, but nothing checks that the three agree. The parity page
lists the debts. Pay one off with a fixture that all three suites read.
