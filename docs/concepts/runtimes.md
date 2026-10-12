---
sidebar:
  order: 3
---

# Choosing a runtime

Start on the lowest runtime that can make the game. Move up only when the game needs
something that the current runtime cannot give.

| Need | Pyxel | LÖVE | MonoGame |
| --- | --- | --- | --- |
| Fastest prototype, code-generated art and sound | Best | Good | Slow |
| Fixed 16-colour palette and Pyxel limits are acceptable | Yes | Not needed | Not needed |
| Browser build | Yes | No | No |
| Free resolution, many colours, larger images | Limited | Yes | Yes |
| Shaders | pyxel-ne screen shader only | Yes | Yes |
| Native Android app | WebView shell | Yes | Yes |
| Performance for many objects | Low | Medium | High |
| Large toolset (content pipeline, .NET libraries, debugger) | No | No | Yes |
| Live agent bridge | No (use pyxel-mcp) | love2d-mcp | monogame-mcp |

## Move up the ladder

The [contract](contract.md) makes a port safe:

1. Record replays on the current runtime.
2. Write their golden traces.
3. Port the rules to the new runtime. Keep rules free of runtime imports.
4. Play the same replays on the new runtime. The traces must match frame for frame.
5. Port drawing and sound. Compare the rendered PNG frames by eye.

Unicycle keeps all three runtimes in one repository: Python in `unicycle/`, Lua in
`love/` and C# in `monogame/`. It checks its recordings against all three.

## Before you choose

Check [Runtime parity](parity.md). Some engine features exist only in Python.
Check [Platforms](platforms.md) for the platforms that you must ship.
