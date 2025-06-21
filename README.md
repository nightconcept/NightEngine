# Night Engine

![License](https://img.shields.io/github/license/nightconcept/NightEngine)
![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/nightconcept/NightEngine/ci.yml)
![GitHub last commit](https://img.shields.io/github/last-commit/nightconcept/NightEngine)
[![codecov](https://codecov.io/gh/nightconcept/NightEngine/graph/badge.svg?token=F9ERB4J3BX)](https://codecov.io/gh/nightconcept/NightEngine)
[![OpenSSF Scorecard](https://api.scorecard.dev/projects/github.com/nightconcept/NightEngine/badge)](https://scorecard.dev/viewer/?uri=github.com/nightconcept/NightEngine)

> [!WARNING]
>WORK IN PROGRESS - NOT FOR PRODUCTION USE
>
>This project is currently in active development and is considered highly experimental. APIs are subject to change, and features may be incomplete or unstable. It is not recommended for use in production environments.

A cross-platform C# game engine built on top of SDL3.

## Overview

Night Engine aims to provide a "batteries-included," in-code editor experience for game development. It consists of two main parts:

1. **`Night` Framework**: A heavily Love2D-inspired API for C# developers, providing a foundational layer for low-level game development tasks. This is the current focus of development. It leverages SDL3 for cross-platform capabilities. I am aiming for Love2D API parity.
2. **`Night.Engine`** (Future): An optional, opinionated game engine built on top of the `Night` framework. It will offer higher-level systems like Scene Management, ECS (Entity Component System), and more, allowing game designers to focus on building games.

This project also intends to be AI-friendly, with clear documentation and API design to assist non-programmer game designers.

## Current Features

* **Window Management:** Creating and managing the game window.
* **Input Handling:** Basic keyboard and mouse input.
* **2D Graphics Rendering:** Loading and drawing sprites, clearing the screen, and presenting frames.
* **Game Loop Structure:** A managed game loop with `Load`, `Update`, `Draw`, and `KeyPressed` callbacks.
* **Sample Game:** A basic platformer demonstrating framework features.
* **Documentation:** API documented.

## Library Structure

The core of the project is the `Night.dll` library. This assembly contains:

* The `Night` namespace: Provides the Love2D-inspired `Night.Framework` API.
* The `Night.Engine` namespace: Will house the future higher-level, more opinionated game engine components. Not yet implemented.

## Feature Roadmap

### `Night` Framework

* [x] **Project Foundation & SDL3 Integration**
* [x] **Window Management** (`Night.Window`)
* [x] **Input Handling**
* [x] **2D Graphics Rendering** (`Night.Graphics`)
* [x] **Game Loop Structure** (`Night.Framework.Run`)
* [x] **Sample Game** (`Night.SampleGame`) demonstrates framework features.
* [ ] **Timer Module** (`Night.Timer`) - *Basic delta time is implemented, further Love2D parity planned.*
* [ ] **Filesystem Module** (`Night.Filesystem`) - *Basic needs met by .NET System.IO, further Love2D parity planned.*
* [ ] **Audio Module** (`Night.Audio`, `Night.Sound`)
* [ ] **Font Module** (`Night.Font`)
* [ ] **Joystick Module** (`Night.Joystick`)
* [ ] **Event Module** (`Night.Event`) - *Beyond basic KeyPressed callback.*
* [ ] **Touch Module** (`Night.Touch`)
* [ ] **Video Module** (`Night.Video`)
* [ ] **Data Module** (`Night.Data`)
* [ ] **Math Module** (`Night.Math`) - *Beyond System.Math, for Love2D specific functions.*
* [ ] **System Module** (`Night.System`) - *Beyond basic OS info.*
* [ ] LLM friendly documentation
* [ ] More Game samples

### `Night.Engine` (Future - Higher-Level Opinionated Systems)

* [ ] Manager system (Assets, Scenes, etc.)
* [ ] Entity Component System (ECS)
* [ ] Scene Management & Scene Graph
* [ ] Advanced Asset Management
* [ ] Dear ImGui Integration
* [ ] Quake-Style Debug Console

## Getting Started (Development)

1. Ensure [mise](https://mise.jdx.dev/) is installed.
2. Clone the repository:

    ```bash
    git clone https://github.com/nightconcept/NightEngine.git
    cd NightEngine
    ```

3. Install project-specific tools and dependencies:

    ```bash
    mise install
    ```

4. Build the solution:

    ```bash
    mise build
    ```

5. Run the sample game:

    ```bash
    mise game
    ```

## Contributing

Contributions... eventually! When I feel like the code-base is in a good place, I will update contributions.

## License

This project is licensed under the [zlib License](LICENSE).
See also [NOTICE.md](project/NOTICE.md) for details on third-party software.
