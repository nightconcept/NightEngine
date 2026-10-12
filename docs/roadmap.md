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

## 1. Move games to the standard justfile surface

[Game commands](games/commands.md) fixes the public recipes, and
`python -m nightengine.recipes` checks them. No game passes yet. Unicycle has 32
problems, mostly `web-*`, `love-*` and `mg-*` recipes that become `--runtime` and
`build` arguments or private helpers. Move each game, then add the check to its
`just check`. Give each game a `just dev` with its own keys.

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

## 6. Runtime catch-up

Every `planned` status in `parity.toml` points here. [Runtime parity](concepts/parity.md)
lists them. Move a group into an execution plan when work starts.

- **C# core**: screen effects, scene music, input axis helpers, pointer helpers, target
  tables, replay play, trace tools and a test driver.
- **Lua and C# core helpers**: custom buttons, storage, rebinding, content and registry,
  systems and the autopilot. Port each one when a game on that runtime needs it, or before.
- **Hosts**: gamepads in the MonoGame host; settings storage and platform info in the LÖVE
  and MonoGame hosts.
- **Contract debts**: shared fixtures for repeats, the scene stack, pointers and targets.

A release can ship with these gaps, because each is decided. `just parity --release` fails
only on `undecided` statuses and missing plans.

## 7. Verify every platform in CI

[Platforms](concepts/platforms.md) marks the builds that no release exercises yet,
for example the Pyxel macOS desktop build. Add each one to an integration game's
release workflow.

## 8. Release tags for games

Games pin raw engine commits today. When releases are regular, games pin `v*` tags,
and each tag has a changelog entry that says what a game must change.
