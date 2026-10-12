---
sidebar:
  order: 3
---

# Game configuration

The builders read the game's `pyproject.toml`. A game sets only these keys. Every
other value comes from the [layout](layout.md).

```toml
[project]
name = "mygame"
version = "0.1.0"                 # Every build uses this version.
dependencies = ["nightengine==0.2.0"]

[tool.uv.sources]
nightengine = { path = "nightengine/python", editable = true }

# Pyxel web build (and the Android shell, which wraps it).
[tool.nightengine.web]
title = "My Game"
width = 640
height = 480
packages = ["mygame", "nightengine"]
assets = ["mygame/assets"]

# One table per target. `controls` filters the input that the target accepts.
[tool.nightengine.targets.desktop]
controls = ["keyboard"]

[tool.nightengine.targets.web]
controls = ["keyboard", "touch"]

[tool.nightengine.targets.android]
controls = ["touch"]

# Android identity (Pyxel shell and LÖVE).
[tool.nightengine.android]
app_id = "dev.example.mygame"
orientation = "landscape"         # landscape | portrait | any
icon = "android/icon.png"
keystore = "android/debug.keystore"
```

A target table can also set `width`, `height` and `name`.

The source of truth for these keys is the builder code:
`python/src/nightengine/{target,web/build,android/build,love/build}.py`.
When you add a key there, add it here.
