---
sidebar:
  order: 5
---

# Agent skills

The engine ships agent skills in `skills/`. Each skill is a folder with a `SKILL.md`.
A game gets the skills that match the engine commit it pins.

| Skill | Use it for |
| --- | --- |
| `nightengine` | Any work in a game on NightEngine: layout, determinism, commands, the engine pin. |
| `nightengine-pyxel` | A Pyxel game: run, replay, frames, web, Android and desktop builds, pyxel-ne. |
| `nightengine-love` | A LÖVE game: run, specs, builds and the love2d-mcp bridge. |
| `nightengine-monogame` | A MonoGame game: run, checks, builds and the monogame-mcp bridge. |

## Link the skills into a game

Agents find skills in `.agents/skills/` (and `.claude/skills/` for Claude Code).
Link the engine skills that the game needs:

```sh
mkdir -p .agents/skills
ln -s ../../nightengine/skills/nightengine .agents/skills/nightengine
ln -s ../../nightengine/skills/nightengine-pyxel .agents/skills/nightengine-pyxel
```

On Windows, enable Developer Mode or `core.symlinks` so git can make links.

A link follows the submodule. When the game moves its engine pin, the skills move too.
Game-only skills, for example a visual review loop for one game, stay in the game.

## Write or change a skill

- A skill describes how to work. The docs describe what things are. A skill links to
  docs pages for details and does not copy them.
- Keep `SKILL.md` short. Put long material in a `references/` folder beside it.
- Use only the recipe names in [Game commands](commands.md).
- Change a skill in the same commit as the workflow that it describes.
