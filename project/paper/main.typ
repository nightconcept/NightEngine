#import "@preview/rubber-article:0.4.2": *

#show: article.with(
  lang: "en",
  header-display: true,
  header-title: "Night Engine: A C# Game Development Engine",
  eq-numbering: "(1.1)",
  eq-chapterwise: true,
  margins: 1.25in,
  cols: none,
)

#maketitle(title: "Night Engine: A C# Game Development Engine", authors: ("Danny Solivan",), date: datetime
  .today()
  .display("[day]. [month repr:long] [year]"))

#block(width: 100%)[
  *Abstract*

  This paper presents Night Engine, a "batteries\-included" C\# game development framework built upon the SDL3 library. The project addresses the need for a streamlined and productive development workflow for C\# developers by providing a high-level, Love2D\-inspired API. The core architecture is two-tiered, consisting of `Night.Framework`, a foundational wrapper around SDL3, and the planned `Night.Engine`, a more opinionated system for common game patterns. Key features of the initial implementation include declarative window management, simplified input polling, 2D sprite-based graphics rendering, and a structured game loop. This work serves as a case study in API design, demonstrating how a low-level, high-performance C++ library can be wrapped to create an idiomatic and developer-friendly C\# experience. The viability of this approach is evaluated through the implementation of a sample game and analysis of the API's expressiveness.
]

// ============== Main Body ==============

= Introduction
In the landscape of C\# game development, developers are often presented with a choice between monolithic, feature-rich engines like Unity and Godot, or the direct, verbose use of low-level C++ libraries via C\# bindings. While powerful, large engines impose significant architectural opinions and overhead. Conversely, direct library interaction, for instance with SDL3, offers maximum control at the cost of productivity and requires boilerplate for fundamental tasks. This creates a gap for lightweight, "batteries\-included" frameworks that offer high-level abstractions without sacrificing performance or flexibility.

This project, Night Engine, aims to fill that niche. It is inspired by the design philosophy of LÖVE, a popular framework for the Lua language, which provides a simple, module\-based API for 2D game development. Night Engine adapts this philosophy to the C\# ecosystem, leveraging the modern capabilities of `.NET 9` and the cross-platform power of SDL3.

The primary goals of this project were to:
- Design and implement a foundational C\# framework (`Night.Framework`) that provides a simple, productive, and Love2D\-inspired API over SDL3.
- Establish a robust architectural base for a future, higher-level, and more opinionated game engine (`Night.Engine`) that will include systems like an ECS and scene management.
- Validate the API design and framework capabilities through the creation of a sample platformer game, demonstrating its ease of use and feature set.

This paper is structured as follows. Section 2 discusses the theoretical foundations and compares Night Engine to related work in the field. Section 3 details the system's architecture and justifies key design decisions. Section 4 covers notable implementation challenges and their solutions. Section 5 presents an evaluation of the framework. Finally, Section 6 concludes and outlines directions for future work.

= Theoretical Foundations & Related Work
The design of Night Engine is grounded in principles of API design, focusing on creating a high-level abstraction over a low-level system. This involves trade-offs between expressiveness, performance, and control.

The primary influence is *LÖVE*, a 2D game framework for Lua. LÖVE's success stems from its simple, module\-centric API (e.g., `love.graphics`, `love.keyboard`) that exposes core functionalities in an easy-to-use manner. Night Engine seeks to replicate this developer experience in a statically\-typed C\# context, which introduces challenges and opportunities in API design, such as the use of static classes to mimic Lua's global tables.

Night Engine also exists within the ecosystem of C\# game frameworks. It can be compared to:
- *MonoGame* and *FNA*: These frameworks are implementations of the Microsoft `XNA 4.0` API. While mature and powerful, their API design is rooted in a paradigm from over a decade ago. Night Engine differentiates itself by building on the modern SDL3 and adopting a different API philosophy.
- *`Raylib-cs`*: A C\# binding for Raylib, another excellent C-based game framework. The key difference lies in the level of abstraction; `Raylib-cs` is a more direct binding, whereas Night Engine provides a more curated and idiomatic C\# layer.
- *Direct `SDL3-CS` usage*: The `SDL3-CS` library provides the raw C\# bindings for SDL3. Night Engine's value proposition is the abstraction layer it builds on top of these bindings, shielding the developer from pointer arithmetic and verbose SDL function calls.

// TODO: Are there other C\# frameworks that attempt a similar "minimalist wrapper" approach that should be cited here? For instance, frameworks focused on specific genres like roguelikes (e.g., SadConsole)?

= System Architecture & Design
The architecture of Night Engine is intentionally layered to separate concerns and provide a stable foundation for future growth. It is composed of two primary libraries: `Night.Framework` and the prospective `Night.Engine`.

#figure(
  rect(width: 80%, height: 30%, stroke: black)[
    #align(center)[
      Application (`Night.SampleGame`)\
      (Implements `IGame` interface)\
      ↓\
      `Night.Engine` (Future)\
      (ECS, Scene Graph, Physics)\
      ↓\
      `Night.Framework` (Love2D-style API)\
      (`Night.Graphics`, `Night.Window`, `Night.Input`)\
      ↓\
      `SDL3-CS` (C\# Bindings)\
      ↓\
      `SDL3` (Native C++ Library)
    ]
  ],
  caption: [The layered architecture of the Night Engine ecosystem.],
) <fig:architecture>

The choice of C\# 13 and `.NET 9` was motivated by the desire to use a modern, performant, and type-safe language with a rich ecosystem. SDL3 was chosen as the underlying backend due to its industry-standard status, cross-platform capabilities, and modern features compared to its predecessor.

== Component A: Night.Framework
The core of the current implementation is `Night.Framework`. It exposes functionality through a series of static classes within the `Night` namespace (e.g., `Night.Graphics`, `Night.Window`, `Night.Mouse`). This design directly emulates the module-based API of LÖVE and provides a simple, accessible surface area for developers. All interactions with SDL3 are encapsulated within this layer.

== Component B: The Game Loop
The framework defines a structured game loop managed by `Night.Framework.Run()`. Developers do not write their own loop but instead implement the `Night.IGame` interface, which provides callbacks for key stages:
- `Load()`: Called once at the start to load assets.
- `Update(deltaTime)`: Called each frame for game logic.
- `Draw()`: Called each frame for rendering.
- `KeyPressed(key)`: An event-based callback for input.

This inversion of control simplifies game creation and ensures a consistent execution model, including details like delta time calculation.

// TODO: Should this section include a more detailed diagram of the call flow from the user's `IGame.Update` and `IGame.Draw` implementations through `FrameworkLoop.cs` to the underlying SDL3 calls?

= Implementation Challenges & Solutions
Translating the design into a functional framework presented several technical challenges.

== Challenge 1: Idiomatic API Translation
A significant challenge was translating LÖVE's dynamic, table-based Lua API into idiomatic C\#. The decision was made to use static classes to provide a global, module\-like access pattern (e.g., `Night.Graphics.Draw(etc.)`). This avoids the need for singleton instances while providing a familiar structure for those coming from LÖVE. For callbacks, C\# events and interfaces (`IGame`) provide a type-safe alternative to Lua's function-based approach.

== Challenge 2: Native Library Management
A common problem in `.NET` projects that rely on native code is ensuring the unmanaged binaries (e.g., `SDL3.dll`) are available at runtime. The solution involved two parts:
1. A Python script (`scripts/sync_sdl3.py`) to fetch the correct pre-built SDL3 binaries for different platforms and place them in a known location (`lib/SDL3-Prebuilt/`).
2. Custom MSBuild targets in the `Night.SampleGame.csproj` file to automatically copy the required native libraries from the `lib` directory to the application's output directory during the build process.

// TODO: What was the most difficult bug encountered during the implementation of the graphics renderer or input system? For example, was there an issue with texture management, coordinate systems, or event polling logic? Describe it here.

= Evaluation & Results
To be a successful project, Night Engine must be evaluated against its primary goals: providing a usable, productive API for C\# game development.

Our evaluation is currently qualitative, based on the implementation of `Night.SampleGame`. The API's ability to support a simple platformer with player movement, sprite rendering, and input handling demonstrates its functional completeness for the `v0.1.0` feature set.

For quantitative analysis, future work will focus on benchmarking. We propose the following metrics:
- *Performance*: Measure the overhead of the framework by comparing the framerate of drawing N sprites using `Night.Graphics` versus raw `SDL3-CS` calls.
- *API Expressiveness*: Compare the lines of code required to perform common tasks (e.g., opening a window and drawing a sprite) in Night Engine versus other frameworks.

#figure(
  table(
    columns: (1fr, 1fr, 1fr),
    align: (center, center, center),
    [*Task*], [*Night Engine (LoC)*], [*Raw `SDL3-CS` (LoC)*],
    [Open 800x600 Window], [2], [~10-15],
    [Load & Draw Sprite], [2], [~15-20],
  ),
  caption: [A hypothetical comparison of lines-of-code (LoC) for common tasks, illustrating the abstraction benefit.],
) <fig:results>

// TODO: What are the most meaningful benchmarks to run? Raw sprite drawing throughput? Input latency? Game loop overhead with N entities? Define the specific experiments to be conducted.

= Conclusion & Future Work
This paper has presented Night Engine, a C\# game development framework designed to provide a LÖVE\-inspired API over SDL3. The project successfully implemented the core `v0.1.0` feature set for `Night.Framework`, including windowing, input, and graphics, and validated its design through a sample application. The primary goal of creating a productive, high-level abstraction over a powerful low-level library was met.

The main limitation of the current work is its scope. The framework currently only covers a fraction of the LÖVE API and does not yet include the higher-level `Night.Engine` components.

Future work will proceed in two main directions, as outlined in the project's product requirements document:
1. Expanding `Night.Framework`: Implementing further modules to achieve greater parity with the LÖVE API, including audio (`Night.Audio`), fonts (`Night.Font`), and joystick support (`Night.Joystick`).
2. Developing `Night.Engine`: Building the higher-level, opinionated engine on top of the framework. This will include an Entity Component System (ECS), a scene graph, and more advanced asset management.
3. Tooling and Performance: Investigating tooling, such as Dear ImGui integration for debug consoles, and performance enhancements, potentially including a migration from SDL_Renderer to the more powerful SDL_GPU backend.

= References
// TODO: Add citations for influential works.
// e.g., The LÖVE documentation, the SDL3 wiki, books on API design or game engine architecture.
// #bibliography("references.bib")
