# Product Requirements

## Vision

NightEngine is a C# game engine on SDL3 with a Love2D-inspired API. Development is split into two layers:

- **`Night.Framework`** — low-level, Love2D-style static API over SDL3 via SDL3-CS. Current focus.
- **`Night.Engine`** — future opinionated layer (ECS, scene management) built on top of `Night.Framework`.

Secondary goal: AI-friendliness so non-programmers can build games with agent assistance.

## Core Technical Decisions

- Public API mirrors Love2D structure where practical and idiomatic in C#.
- All SDL3 interaction within `Night.Framework` goes through SDL3-CS bindings. `Night.Engine` will not use SDL3-CS directly.
- Rendering backend: SDL_Renderer for now. Future: evaluate SDL_GPU.
- `Night.dll` is a class library. `SampleGame` is an executable consumer.
- Target platforms: Windows, macOS, Linux. Long-term: iOS, Android.

## Stack

- .NET 10 / C# 13
- SDL3 via `SDL3-CS` submodule (vendored at `lib/SDL3-CS/`)
- Native SDL3 binaries fetched by `scripts/sync_sdl3.py` into `lib/SDL3-Prebuilt/`
- xUnit for tests; docfx for API docs; mise for task orchestration

## Current Development Phase

Complete `Night.Framework` to reach v0.1.0 API coverage. See `.agents/roadmap.md` for version targets and `.agents/love-api.md` for current coverage map. Active module epics are in `.agents/epics/`.

## Future: Night.Engine

Post-framework features planned for the `Night.Engine` namespace:
- Entity Component System (ECS)
- Scene Management and Scene Graph
- Advanced Asset Management
- Physics integration (optional)

## Post-0.1.0 Framework Expansions

- Audio (`Night.Audio`), Font (`Night.Font`), Sound decoding
- Expanded input: Joystick, Touch
- More graphics primitives, basic shader support, camera
- Tooling: Dear ImGui integration, Quake-style debug console, Lua scripting
- Platform verification: Android, iOS
