---
sidebar:
  order: 1
---

# Use the engine in a game

A game adds NightEngine as a git submodule at `nightengine/`. It pins one commit.
It never edits the engine in place.

## Add the engine

```sh
git submodule add -b main https://github.com/nightconcept/NightEngine.git nightengine
git submodule update --init --recursive
```

Then connect the runtime that the game uses:

| Runtime | Connection |
| --- | --- |
| Pyxel | In `pyproject.toml`: `dependencies = ["nightengine==<version>"]` and `[tool.uv.sources] nightengine = { path = "nightengine/python", editable = true }`. Then `uv sync`. |
| LÖVE | Link the Lua package: `ln -s ../nightengine/lua love/nightengine`. Code uses `require("nightengine.core")`. |
| MonoGame | Reference `nightengine/csharp/NightEngine.Core` from the rules project. Compile `nightengine/csharp/MonoGame/*.cs` into the platform project. |

The Python package also holds the builders for every runtime, so LÖVE and MonoGame
games install it too.

Exclude the engine from the game's linters. The engine lints in its own repository.
For ruff, add `extend-exclude = ["nightengine"]`.

## Move the pin

1. Change the engine on its own `main` branch, in its own repository.
2. Push it there.
3. In the game: `git -C nightengine pull` (or check out a tag).
4. Run the game's `just check` and `just lint`.
5. Commit the new submodule pointer with a message that names the engine commit.

Do not follow `main` automatically. Pin a commit or a `v*` tag that the game has verified.

## Older ways (do not copy)

Some games still use an older way. They move to the submodule over time
(see the [Roadmap](../roadmap.md)):

- **Vendored copy with a digest.** Dawnfall, Thunderwing, Cinderspire and Vesper
  copy an older Python-only engine into `nightengine/` and check that all four
  copies match with `python -m nightengine.digest`.
- **Submodule on a side branch.** Time Tumble pins the old `unicycle` branch.
