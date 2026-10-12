---
name: nightengine
description: Work in a Nightcircle game built on NightEngine (Pyxel, LÖVE or MonoGame). Use for any change to a game repository that has a nightengine/ submodule, before the runtime skill. Covers the layout, determinism rules, standard commands, replays and the engine pin.
---

# NightEngine game

The engine is the `nightengine/` submodule. Its docs are in `nightengine/docs/`.
Start at `nightengine/docs/index.md`. Then load the runtime skill:
`nightengine-pyxel`, `nightengine-love` or `nightengine-monogame`.

## Rules

- Never edit `nightengine/` in the game. Change the engine in its own repository on
  `main`, then move the pin. See `nightengine/docs/games/consume.md`.
- The engine knows no game. If the game needs an engine change, make it a parameter.
- Keep the simulation deterministic. Use `game.rng` for all randomness. Do not read
  the wall clock or the real screen size in rules.
- Every input goes through `Game.step` as one int of button bits, plus pointers and
  events. Then a replay plays it back exactly.
- Game rules never import the runtime. Only drawing, sound, app and frame modules do.
- Data holds the numbers. Code holds the behaviour.
- Put files where `nightengine/docs/games/layout.md` and the game's
  `docs/repo/structure.md` say.

## Loop

1. Read the game's `AGENTS.md`.
2. Make the change.
3. Run `just check` and `just lint`. Fix every failure.
4. For a rules change: record a replay (`just autoplay <seed>`) and check the traces.
   Update golden fixtures (`just golden`) only when the rules change on purpose.
5. For a visual change: run `just frames <replay>` and look at the PNG files.
   Show the user before and after frames.

Recipe names are in `nightengine/docs/games/commands.md`.

## When you are blocked

- A feature exists in one runtime only: check `nightengine/docs/concepts/parity.md`.
- A platform does not build: check `nightengine/docs/concepts/platforms.md`.
- The docs and the code disagree: trust the code, then fix the docs in the engine.
