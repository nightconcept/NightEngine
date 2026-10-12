# Lua / LÖVE

Use LÖVE 11.5. In this repository, `mise install` supplies it on Linux x64,
macOS arm64 and Windows x64. Mount `lua/` as the package directory `nightengine`:

```sh
ln -s ../nightengine/lua love/nightengine
love love
```

Configure `nightengine.host.app` with the game constructor, input, renderer,
audio, dimensions and frame rate. Inspection and snapshot callbacks power debug
controls. See `examples/lua-love` and Unicycle's adapter. The core uses plain
LuaJIT without LÖVE.

```sh
luajit nightengine/tests/lua.lua --path love nightengine/tests/lua love/spec
uv run python -m nightengine.love build love windows
```

The builder reads version, title and size from the game's pyproject.toml.
`tool.nightengine.targets` defines targets. `tool.nightengine.android` supplies
identity, orientation, icon and keystore. Game source lives in `love/`.
Windows fusion builds on any system; macOS builds on macOS. Android requires
Java and SDK build-tools. Development bridge files are excluded from archives.
