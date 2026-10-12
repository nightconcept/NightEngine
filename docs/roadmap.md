---
sidebar:
  order: 2
---

# Roadmap

This page lists the gaps between the engine today and the [Vision](vision.md),
most important first. When you start an item, write an execution plan in
`exec-plans/active/` and link it here. When you finish an item, remove it and
update the page that it changes.

Last reviewed: 2026-10-12.

## 1. An engine-owned game toolkit

Games copy the engine commands into their own `justfile`. Unicycle has about 45
recipes, and most of them only call engine modules. Each game wraps them in a
different way.

Goal: the engine ships the standard recipes in [Game commands](games/commands.md).
One option is a just module in this repository that a game imports with
`import 'nightengine/game.just'`. The game then adds only its own recipes.

## 2. MonoGame tooling in the engine

The engine has the C# core, `Host.cs` and the dev bridge. The MonoGame build script,
Android checks, asset export and project files live in Unicycle's `monogame/`.
Move the game-agnostic parts into the engine. Then a second MonoGame game needs no copy.

## 3. A new-game template for each runtime

A game starts by copying another game. Add a scaffold that writes the
[Game layout](games/layout.md), the `pyproject.toml` keys, the `justfile`, the
`AGENTS.md` hub and the skill links for the chosen runtime.

## 4. iOS

No runtime builds for iOS yet. Each runtime needs its own path:

- Pyxel: a native shell around the web build, like the Android shell.
- LÖVE: the LÖVE iOS Xcode project with the game archive.
- MonoGame: the .NET iOS workload.

All three need macOS and an Apple signing setup in CI.

## 5. One way to consume the engine

Some games still use older ways. See [Use the engine in a game](games/consume.md).

- Dawnfall (`df1`), Thunderwing (`raiden`), Cinderspire (`spire`) and Vesper (`vania`)
  keep a vendored copy and check it with a digest.
- Time Tumble (`tmnt`) uses the submodule on the old `unicycle` branch.

Move each game to the submodule on a pinned `main` commit or tag.

## 6. Close the parity work

[Runtime parity](concepts/parity.md) lists the open items. Each `undecided` status must
be decided before the next engine release; `just check` fails after the version moves.
Pay off the contract debts with shared fixtures for repeats, the scene stack, pointers
and targets. The rules are in [Layers and parity](concepts/layers.md).

## 7. Verify every platform in CI

[Platforms](concepts/platforms.md) marks the builds that no release exercises yet,
for example the Pyxel macOS desktop build. Add each one to an integration game's
release workflow.

## 8. Release tags for games

Games pin raw engine commits today. When releases are regular, games pin `v*` tags,
and each tag has a changelog entry that says what a game must change.
