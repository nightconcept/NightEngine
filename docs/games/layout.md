---
sidebar:
  order: 2
---

# Game layout

Every game uses this layout. The engine, the skills and other agents expect it.
A game changes the layout only when the change is a parameter that the engine reads.

`<game>` is the game's package name, for example `unicycle` or `nightrunner`.

## Repository root

```
<game-repo>/
  AGENTS.md            Agent hub. 100 lines or fewer. Links to docs/.
  README.md            For people: what the game is and how to run it.
  justfile             The standard commands (see Game commands), then game-only ones.
  mise.toml            Tool versions. mise.lock pins them.
  pyproject.toml       Name, version, engine pin and [tool.nightengine] keys.
  nightengine/         The engine submodule. Never edit it here.
  docs/
    design/            GDD and design decisions: what the game is.
    repo/              structure.md, content.md, testing.md, release.md: how the repo works.
    exec-plans/        active/ and completed/ multi-step plans.
  replays/             Recorded runs (JSON). Test fixtures live in tests/fixtures/.
  frames/              Rendered PNG frames. Not committed.
  tests/               Game tests. tests/fixtures/ holds golden replays and traces.
  .agents/skills/      Links to the engine skills, plus game-only skills.
```

## Pyxel game

```
  game.py              Entry point: `uv run game.py`.
  <game>/
    config.py          Base size, playfield and paths. No tuning numbers.
    game.py            Game subclass: scene stack and seed.
    content.py         Loads data/*.json into frozen records.
    data/              JSON records: the numbers.
    scenes/            Scene subclasses: the rules.
    render/            One draw function per scene. Imports pyxel.
    art/               Code-drawn art by name.
    app.py             Builds the host App. Imports pyxel.
    audio.py           Sound and music tables. Imports pyxel.
    frames.py          Renders a replay to PNG. Imports pyxel.
    autopilot.py       Plays the game to make recordings.
```

## LÖVE game

```
  love/
    main.lua  conf.lua Entry point for `love love`.
    nightengine        Link to ../nightengine/lua.
    <game>/            Rules (no `love`), then app, draw, render, audio, input (use `love`).
    assets/            Images, fonts and sound files.
    spec/              Lua specs. They replay the shared fixtures.
```

## MonoGame game

```
  monogame/
    <Game>.Rules/      Rules. References NightEngine.Core. Never references MonoGame.
    <Game>.MonoGame/   Drawing and sound, shared by every platform project.
    <Game>.Desktop/    DesktopGL project. Compiles the engine host sources.
    <Game>.Android/    Android project.
    <Game>.Tests/      Contract and rules tests.
    <Game>.Tools/      Command-line tools: trace and autoplay.
    assets/
```

## Layer rule

Game rules never import the runtime. Only the drawing, sound, app and frame modules
do: `render/`, `app`, `audio` and `frames` in Python and Lua, and the `.MonoGame`
and platform projects in C#. A test in each game checks this.

## Content rule

Data holds the numbers. Code holds the behaviour. A record in `data/` names its
handler as a string, and the game's registry maps the name to a function. Each game
lists its record formats in `docs/repo/content.md`. See "Adding content" in
[Python/Pyxel](../runtimes/python-pyxel.md).
