---
sidebar:
  order: 5
---

# Platforms

This page shows which runtime builds which platform today. Keep it true: change it
in the same commit as a builder or a release workflow.

| Platform | Pyxel | LÖVE | MonoGame |
| --- | --- | --- | --- |
| Linux x64 | Released | Released (AppImage, build on Linux) | Released |
| macOS arm64 | Released | Released (build on macOS) | Released |
| Windows x64 | Released | Released (fused exe) | Released |
| Android | Released (WebView shell) | Released | Released (native) |
| iOS | Not started | Not started | Not started |
| Browser | Released | Not supported | Not supported |

- **Released**: an integration game's release workflow builds it.
- **Not verified**: the builder can run, but no release exercises it.
- **Not started**: no builder exists. See the [Roadmap](../roadmap.md).

## Who builds what

| Runtime | Builder | Owner |
| --- | --- | --- |
| Pyxel | `python -m nightengine.desktop build`, `nightengine.web build`, `nightengine.android build` | Engine |
| LÖVE | `python -m nightengine.love build [love\|windows\|macos\|linux\|android]` | Engine |
| MonoGame | `monogame/build.py desktop\|android` | Unicycle (move to engine, see Roadmap) |

Builders read the game's `pyproject.toml`. See [Game configuration](../games/configuration.md).

## Development platforms

Engine and game development runs on Linux x64, macOS arm64 and Windows x64.
`mise install` supplies the same tool versions on all three.
