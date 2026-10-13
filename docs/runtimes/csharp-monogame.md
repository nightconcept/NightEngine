---
sidebar:
  order: 3
---

# C# and MonoGame

The C# core implements deterministic frames, scenes, input edges and repeats, target filtering, the portable RNG, replay JSON, and canonical traces. It imports no graphics runtime. Unicycle checks its five recordings against this core.

`csharp/MonoGame/Host.cs` supplies the fixed 30 Hz loop, viewport mapping, recording, replay, PNG capture, and lifecycle handling. Games derive from `Host` and supply bindings, drawing and sound. `Post(event)` queues a host event for the next live frame, and `PostDev(pairs)` sends `just dev` pairs (`level=3`) as `Inputs.DevEvent` events; the game reads them with `Input.Dev()`. Compile these host sources against the platform's MonoGame 3.8.6 package. DesktopGL and Android use separate projects.

Use .NET SDK 10.0.401. The core package version is 0.2.1. `just check` runs the contract checks. `just build` produces `NightEngine.Core.0.2.1.nupkg`. The release includes a source archive with the host and MCP adapter. It does not publish to an external NuGet feed.

The Debug-only `DevBridge` accepts newline JSON on loopback port 12346. The game opts in when started with `--dev`. Request IDs return with each response. The MonoGame MCP adapter uses a locked MCP SDK dependency and stdio transport. Install it with:

```sh
python3 tools/monogame-mcp/setup.py --out .tools/monogame-mcp
node tools/monogame-mcp/server.mjs .tools/monogame-mcp
```

The eight tools inspect state, capture the actual framebuffer, send recorded input, run typed actions, watch/unwatch state, list assets, and reload assets. A game supplies the snapshot callback. Actions use the normal frame path. Arbitrary code execution is unavailable. Asset reload reads built content files; copy changed source assets into the build output first. Unicycle's repository skill describes its visual inspection loop.

Release builds omit `DevBridge`. The core is trimmable (`IsTrimmable`), so a game can set `PublishTrimmed` on its
desktop project: Unicycle's self-contained desktop archives drop from about 38 MB to about 12 MB. This implementation supports Unicycle's contract surface; Python-specific settings storage, rebinding and display pacing remain Python facilities.
