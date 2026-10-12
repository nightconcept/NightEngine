---
sidebar:
  order: 4
---

# Game commands

Every game exposes the same public `just` recipes. An agent never has to read a
justfile to find a command. A game writes the recipe bodies; the names, parameters
and doc comments are fixed. Everything else is private.

`python -m nightengine.recipes` checks a game's justfile against this page. Call it
from the game's `check` recipe, so `just check` fails when the surface grows.

## The surface

| Recipe | Required | Does |
| --- | --- | --- |
| `just setup` | Yes | Fetch the engine submodule and install the tools. |
| `just check` | Yes | Run every test for every runtime that the game has, and the surface check. |
| `just lint` | Yes | Check lint and format for every runtime. |
| `just fmt` | Yes | Fix the format. |
| `just run *args` | Yes | Play the game. |
| `just dev *args` | Yes | The agent entry point: start the game in a named state. The game defines the keys. |
| `just build *args` | No | Build release files into `dist/`. |
| `just replay <file> ...` | No | Watch a recording. |
| `just frames <file> ...` | No | Render a recording to PNG files, with no window. |
| `just autoplay ...` | No | Let the autopilot play a seed and record it. |
| `just golden` | No | Record the fixture replays and traces again, after a rule change on purpose. |

## Rules

- A public recipe has a standard name and a doc comment. `just --list` shows the comments.
- `run`, `dev` and `build` end with a variadic parameter (`*args`), so options never
  need a new recipe.
- `replay` and `frames` take the recording path first.
- Helpers are private: prefix the name with `_` or add `[private]`.
- No aliases and no `mod` modules.

## Standard options

Use these options with the same meaning in every game. A game passes them on to its runtime.

| Option | Recipes | Meaning |
| --- | --- | --- |
| `--runtime pyxel\|love\|monogame` | `run`, `dev`, `build` | Pick the runtime in a multi-runtime game. The default is the game's first runtime. |
| `--target desktop\|web\|android` | `run`, `dev` | Play another target's layout and controls. |
| `--seed N` | `run`, `dev` | Start with this seed. |
| `--replay FILE` | `run` | Play a recording. |
| `web`, `android`, `desktop`, `love`, … | `build` | The release files to build. No argument builds all that this system can. |

## `just dev`

`dev` takes `key=value` pairs and starts the game in that state with the dev tools on.
The keys belong to the game: a level, a boss, a room, a difficulty. The doc comment
lists them, so `just --list` tells an agent what it can ask for:

```just
# Start in a state with dev tools on: level=N stage=NAME boss=NAME seed=N
dev *args:
    uv run game.py --dev {{args}}
```

```sh
just dev level=3 boss=hydra
```

`just dev` with no arguments starts the first scene with the dev tools on. The game
parses the pairs in its own code and applies them before the first frame. A pair
must give the same state on every runtime, so a recording made after `just dev`
replays the same.
