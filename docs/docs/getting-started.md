# Getting Started with Night Engine

This guide will help you get Night Engine set up and running.

## Prerequisites

* **.NET 10 SDK**.
* A C# compatible IDE (e.g., Visual Studio, JetBrains Rider, VS Code with C# Dev Kit).

## Dependencies

Night Engine relies on SDL3 and its related libraries. These are managed as follows:

* **SDL3-CS Bindings:** The C# bindings for SDL3 (`SDL3-CS`) are included as a NuGet package (`SDL3-CS`) in the `src/Night/Night.csproj` project.
* **Native SDL3 Binaries:**
  * The core SDL3, SDL3_image, SDL3_mixer, and SDL3_ttf native libraries are required at runtime.
  * These are fetched into the `lib/SDL3-Prebuilt/` directory using the `scripts/sync_sdl3.py` Python script.
  * The `src/SampleGame/SampleGame.csproj` project is configured to copy these necessary native binaries (e.g., `SDL3.dll`, `SDL3_image.dll`) to its output directory during the build process.

## Setup and Building

1. **Clone the Repository:**

    ```bash
    git clone https://github.com/nightconcept/NightEngine.git
    cd NightEngine
    ```

2. **Build the Solution:**
    You can build the entire solution (`Night.slnx`) using your IDE or the .NET CLI:

    ```bash
    dotnet build Night.slnx
    ```

    This will:
    * Compile the `Night` class library (`src/Night/Night.csproj`) into `Night.dll`.
    * Compile the `SampleGame` application (`src/SampleGame/SampleGame.csproj`).
    * Copy the necessary SDL3 native binaries to the `SampleGame` output directory (e.g., `src/SampleGame/bin/Debug/net10.0/`).

## Running the Sample Game

After a successful build, you can run the sample game:

```bash
dotnet run --project src/SampleGame/SampleGame.csproj
```

This will launch the `SampleGame` application, which demonstrates various features of the `Night` framework.

## Project Structure Overview

* **`docs/`**: Contains project documentation, including this guide, the PRD, and operational guidelines.
* **`lib/`**: Contains third-party libraries, primarily the prebuilt SDL3 binaries.
* **`scripts/`**: Utility scripts for the project, like `sync_sdl3.py`.
* **`src/`**: All C# source code.
  * **`src/Night/`**: The core `Night.Framework` and future `Night.Engine` library.
  * **`src/SampleGame/`**: The sample game application demonstrating engine features. Use this as a starting template for your game.
* **`Night.slnx`**: The main Visual Studio solution file.

## Next Steps

* Explore the code in `src/SampleGame/Program.cs` to see how `Night.Framework` is used.
* Review the API documentation (once available) for `Night.Framework` modules (e.g., `Night.Window`, `Night.Graphics`, `Night.Keyboard`, `Night.Mouse`).
* Consult the [Introduction](introduction.md) for a higher-level overview of the engine.
