# Project Guidelines

The "Night" engine project will ALWAYS adhere to the **Google C# Style Guide**. Key aspects of this guide, supplemented by project-specific interpretations, are outlined below.

- **Formatting & Style:**
  - **Indentation:** 2 spaces, no tabs.
  - **Column Limit:** 100 characters.
  - **Whitespace, Braces, Line Wrapping:** Adhere to the detailed rules in the Google C# Style Guide. This includes rules like no line break before an opening brace, and braces used even when optional.
  - Format the `using` directives with specific spacing. Place all System.* directives first, followed by a blank line. Then, group other using directives (like third-party libraries or project-specific namespaces) logically, and insert a blank line between each distinct group. For example, list System usings, then a blank line, then Night usings, then a blank line, then SDL3 usings, rather than listing them all contiguously.
  -`using` directives should NEVER have any comments associated with them or on the same line
- **Naming Conventions:**
  - **General Rules Summary:**
    - Names of classes, methods, enumerations, public fields, public properties, namespaces: `PascalCase`.
    - Names of local variables, parameters: `camelCase`.
    - Names of private, protected, internal, and protected internal fields and properties: `_camelCase` (e.g., `_privateField`).
    - Naming convention is unaffected by modifiers such as `const`, `static`, `readonly`, etc..
    - For casing, a “word” is anything written without internal spaces, including acronyms (e.g., `MyRpc` not `MyRPC`).
    - Names of interfaces start with `I` (e.g., `IInterface`).
    - Filenames and directory names are `PascalCase` (e.g., `MyFile.cs`).
- **Code Organization:**
  - **Modifier Order:** `public protected internal private new abstract virtual override sealed static readonly extern unsafe volatile async`.
  - **Namespace `using` Declarations:** Place at the top of the file, before any namespace declarations. Order alphabetically, with `System` imports always first.
  - **Class Member Ordering:** Follow the prescribed order: Nested types, static/const/readonly fields, instance fields/properties, constructors/finalizers, methods. Within each group, elements are ordered by access: Public, Internal, Protected internal, Protected, Private.
- **Key Principles (Project-Specific additions and emphasis):**
  - **API Design (Night framework and Night Engine):** Strive for an API design that is idiomatic to C# while closely mirroring the spirit, structure, and ease of use of the Love2D API for the features being implemented.
  - **Clarity over Premature Optimization:** For the prototype, prioritize clear, understandable, and maintainable code.

- **Logging:** Utilize the `Night.Log.LogManager` to obtain logger instances (e.g., `LogManager.GetLogger("MyCategory")`).
    Use appropriate log levels (`Info`, `Debug`, `Warn`, `Error`, `Fatal`) for messages.
    Refer to `project/epics/logger-tasks.md` for sink configuration and advanced usage.

## NightTest Framework Testing Guidelines

This section outlines how to write tests for the `Night` framework using the `NightTest` project. The testing approach combines xUnit for test execution and orchestration with custom `IGame` implementations for the actual test logic.

**1. Core Concepts & Directory Structure:**

- **xUnit Test Classes (Test Groups):**
  - These classes are responsible for running one or more `IGame` test cases.
  - They reside in subdirectories under [`tests/Groups/`](tests/Groups/) (e.g., [`tests/Groups/Timer/TimerGroup.cs`](tests/Groups/Timer/TimerGroup.cs), [`tests/Groups/Graphics/GraphicsGroup.cs`](tests/Groups/Graphics/GraphicsGroup.cs)).
  - They **must** inherit from [`NightTest.Core.TestGroup`](tests/Core/TestGroup.cs).
  - They use xUnit's `[Fact]` attribute to define individual test methods. Each `[Fact]` method typically runs one specific `IGame` test case.
- **`IGame` Test Case Classes:**
  - These classes contain the actual test logic and implement `Night.IGame` and [`NightTest.Core.ITestCase`](tests/Core/ITestCase.cs).
  - They **must** inherit from [`NightTest.Core.GameTestCase`](tests/Core/GameTestCase.cs) or [`NightTest.Core.ManualTestCase`](tests/Core/ManualTestCase.cs).
  - They are typically located alongside their corresponding xUnit test class or in a related file (e.g., `TimerTests.cs` contains multiple `IGame` test cases like `GetTimeTest`, run by `TimerGroup.cs`).
- **Core Infrastructure:**
  - [`tests/Core/`](tests/Core/): Contains base classes and core types for the testing framework.
    - [`ITestCase.cs`](tests/Core/ITestCase.cs): Interface defining metadata for an `IGame` test case.
    - [`GameTestCase.cs`](tests/Core/GameTestCase.cs): Base class for automated `IGame` test cases, providing common functionality (stopwatch, status tracking, completion helpers).
    - [`ManualTestCase.cs`](tests/Core/ManualTestCase.cs): Base class for manual `IGame` test cases, extending `GameTestCase` with UI for manual pass/fail confirmation.
    - [`TestGroup.cs`](tests/Core/TestGroup.cs): Base class for xUnit test classes, providing the `Run_GameTestCase` helper method.
    - [`TestTypes.cs`](tests/Core/TestTypes.cs): Enums for `TestType` (Automated, Manual) and `TestStatus` (NotRun, Passed, Failed, Skipped).

**2. Workflow for Adding New Tests:**

**Step 1: Create the `IGame` Test Case Class**

For each specific feature or function you want to test (e.g., a new `Night.Graphics` method):

- **Location:** Create the class in a relevant file within a subdirectory of `tests/Groups/` (e.g., for a new graphics test, it might go in `tests/Groups/Graphics/NewGraphicsFeatureTest.cs` or be added to an existing file like `GraphicsTests.cs` if it contains multiple small test case classes).
- **Inheritance:**
  - For automated tests: Inherit from [`NightTest.Core.GameTestCase`](tests/Core/GameTestCase.cs).
  - For tests requiring manual user confirmation: Inherit from [`NightTest.Core.ManualTestCase`](tests/Core/ManualTestCase.cs).
- **Implement `ITestCase` Properties (Abstract in `GameTestCase`):**
  - `public override string Name { get; }`: Provide a unique, descriptive name (e.g., `"Graphics.DrawSpriteAlpha"`).
  - `public override string Description { get; }`: Describe what the test does.
  - The `Type` property is automatically set to `TestType.Automated` by `GameTestCase` or `TestType.Manual` by `ManualTestCase`.
- **Implement `IGame` Logic (Override methods from `GameTestCase`):**
  - `protected override void Load()`: Initialize resources, set up initial state for your test.
  - `protected override void Update(double deltaTime)`: Implement the core test logic.
    - Use helper methods from `GameTestCase` like `CheckCompletionAfterDuration()` or `CheckCompletionAfterFrames()` to define pass/fail conditions and automatically set `CurrentStatus`, `Details`, and call `EndTest()`.
    - For manual tests inheriting from `ManualTestCase`, call `RequestManualConfirmation("Your question to the user?")` when ready for user input. The base class handles the UI and timeout.
  - `protected override void Draw()`: Implement any rendering needed for the test to be visually inspected or to function.
  - `public override void KeyPressed(...)`, `MousePressed(...)`, etc.: Override if your test needs specific input handling beyond what `ManualTestCase` provides.
- **Test Completion:**
  - Ensure your test logic eventually leads to `CurrentStatus` being set (e.g., to `TestStatus.Passed` or `TestStatus.Failed`) and `Details` populated.
  - The `EndTest()` method (called by completion helpers or directly) will stop the `TestStopwatch` and call `Night.Window.Close()`, which signals the `Night.Framework.Run()` method (invoked by `TestGroup.Run_GameTestCase`) to return.

**Step 2: Create/Update the xUnit Test Class (Test Group)**

For each "module" or logical grouping of tests (e.g., `Timer`, `Graphics`, `MyModule` corresponding to a `Night` namespace like `Night.Timer`):

- **Location:** Ensure an xUnit test class exists in the corresponding subdirectory under `tests/Groups/` (e.g., `tests/Groups/MyModule/MyModuleGroup.cs`). If it doesn't exist for a new module, create it. The filename should typically be `[ModuleName]Group.cs`.
- **Inheritance:** The class **must** inherit from [`NightTest.Core.TestGroup`](tests/Core/TestGroup.cs).
- **Constructor:** It must have a constructor that accepts `Xunit.Abstractions.ITestOutputHelper outputHelper` and passes it to the `base(outputHelper)` constructor.
- **Add `[Fact]` Method:** For each new `IGame` test case you created in Step 1, add a new public void method annotated with `[Fact]`.
  - **Naming:** Conventionally, `Run_YourIGameTestCaseName()` (e.g., `Run_MyAutomatedFeatureTest()`).
  - **Implementation:**
        1. Call `Run_GameTestCase(new MyAutomatedFeatureTest());`. This method is inherited from `NightTest.Core.TestGroup` and handles the execution, logging, and assertion.
  - **Traits:** Add `[Trait("TestType", "Automated")]` or `[Trait("TestType", "Manual")]` to the `[Fact]` method. This should match the `Type` of the `IGame` test case being run (which is determined by whether it inherits `GameTestCase` or `ManualTestCase`) and is used for filtering tests via the xUnit runner.

**3. Running Tests:**

- Tests can be run using the .NET CLI (`dotnet test`) or through the Test Explorer in Visual Studio.
- Use xUnit's filtering capabilities to run specific tests (e.g., `dotnet test --filter TestType=Automated`).

**4. Key Considerations:**

- **`Run_GameTestCase` Method:** The `NightTest.Core.TestGroup.Run_GameTestCase` method will:
  - Log the start and end of the `IGame` test case using `ITestOutputHelper`.
  - Call `Night.Framework.Run(testCase)`, which blocks until the `IGame` test case calls `Night.Window.Close()` (typically via `EndTest()` in `GameTestCase`).
  - Log the `CurrentStatus`, `Details`, and `TestStopwatch.ElapsedMilliseconds` from the `GameTestCase` instance.
  - Assert that `testCase.CurrentStatus == TestStatus.Passed`. If not, the xUnit test will fail.
- **Error Handling:** Unhandled exceptions during `Night.Framework.Run(testCase)` are caught by `Run_GameTestCase`, which will then call `testCase.RecordFailure()` and fail the xUnit test.
- **Clarity and Focus:** Each `IGame` test case should be focused on testing a specific piece of functionality. The `Name` and `Description` properties should clearly state its purpose.

By following these guidelines, tests for the `Night` framework can be added systematically, leveraging the provided base classes and xUnit integration.

## Mapping Native SDL3 Functions to SDL3-CS (C#) Bindings

When working with the `lib/SDL3-CS` C# wrapper for SDL3, it's often necessary to find the C# equivalent of a native SDL3 C function, enum, or struct. This section provides guidance on that process. The `lib/SDL3-CS` bindings are located in the `lib/SDL3-CS/SDL3-CS/` directory.

**1. Naming Conventions:**

- **Functions:** Native SDL3 functions (e.g., `SDL_CreateWindow`, `SDL_PollEvent`) are generally mapped to C# methods within the static `SDL` class using PascalCase. The `SDL_` prefix is removed, and the rest of the name is converted to PascalCase.
  - `SDL_CreateWindow` becomes `SDL.CreateWindow()`
  - `SDL_PollEvent` becomes `SDL.PollEvent()`
- **Enums and Structs:** Native SDL3 enums and structs (e.g., `SDL_WindowFlags`, `SDL_Event`, `SDL_Keycode`) are typically mapped to C# enums or structs within the `SDL` static class (or directly in the `SDL3` namespace if they are complex types used by the static class members), also using PascalCase.
  - `SDL_WindowFlags` becomes `SDL.WindowFlags` (enum)
  - `SDL_Event` becomes `SDL.Event` (struct)
  - `SDL_Keycode` becomes `SDL.Keycode` (enum)
- **Constants:** Native SDL3 `#define` constants (e.g., `SDL_INIT_VIDEO`) are usually mapped to enum members or `public const int` fields within the relevant C# enum or static class.
  - `SDL_INIT_VIDEO` becomes `SDL.InitFlags.Video`

**2. File Structure of `lib/SDL3-CS/SDL3-CS/SDL/`:**

The C# source files for the core SDL3 bindings are primarily located under `lib/SDL3-CS/SDL3-CS/SDL/`. This directory is further organized into subdirectories that often mirror SDL3's own categorization of its API (e.g., `Basics`, `Video`, `Audio`, `Input Events`, `GPU`).

- **P/Invoke Declarations:** The actual `[LibraryImport]` or `[DllImport]` attributes for native functions are often found in files named `PInvoke.cs` within the relevant subdirectory (e.g., [`lib/SDL3-CS/SDL3-CS/SDL/Basics/init/PInvoke.cs`](lib/SDL3-CS/SDL3-CS/SDL/Basics/init/PInvoke.cs:1) for `SDL_Init`, [`lib/SDL3-CS/SDL3-CS/SDL/Video/video/PInvoke.cs`](lib/SDL3-CS/SDL3-CS/SDL/Video/video/PInvoke.cs:1) for windowing functions).
- **Enum and Struct Definitions:** These are typically in their own dedicated `.cs` files, named after the type (e.g., [`lib/SDL3-CS/SDL3-CS/SDL/Video/video/WindowFlags.cs`](lib/SDL3-CS/SDL3-CS/SDL/Video/video/WindowFlags.cs:1), [`lib/SDL3-CS/SDL3-CS/SDL/Input Events/events/Event.cs`](lib/SDL3-CS/SDL3-CS/SDL/Input%20Events/events/Event.cs:1), [`lib/SDL3-CS/SDL3-CS/SDL/Input Events/keycode/Keycode.cs`](lib/SDL3-CS/SDL3-CS/SDL/Input%20Events/keycode/Keycode.cs:1)).
- **Partial Class `SDL`:** The main C# static class `SDL` (in the `SDL3` namespace) is defined as a `partial class`. This means its members (P/Invoke methods, nested enums/structs, helper functions) are spread across multiple files within these subdirectories but are all part of the single `SDL3.SDL` static class from the perspective of an API consumer.

**3. Strategy for Finding C# Equivalents:**

- **Identify the Native SDL3 Element:** Start with the name of the native C function, enum, struct, or constant you need (e.g., `SDL_GetWindowFlags`, `SDL_EventType`, `SDL_SCANCODE_A`).
- **Apply C# Naming Conventions:**
  - Remove `SDL_` prefix.
  - Convert to `PascalCase` (e.g., `GetWindowFlags`, `EventType`, `ScancodeA`). Note that for constants like scancodes, the C# enum member might be simpler (e.g. `SDL.Scancode.A`).
- **Determine the SDL Subsystem:** Understand which part of SDL the function belongs to (e.g., Video, Events, Keyboard, Mouse). This will guide you to the likely subdirectory in `lib/SDL3-CS/SDL3-CS/SDL/`.
  - Example: `SDL_CreateWindow` is a Video function. Look in `lib/SDL3-CS/SDL3-CS/SDL/Video/video/`.
  - Example: `SDL_PollEvent` is an Event function. Look in `lib/SDL3-CS/SDL3-CS/SDL/Input Events/events/`.
- **Search within the Subsystem Directory:**
  - For functions, check `PInvoke.cs` files first.
  - For enums/structs, look for a `.cs` file matching the PascalCase name (e.g., `EventType.cs`, `WindowFlags.cs`).
  - The C# equivalent will typically be a static method or nested type of `SDL3.SDL` (e.g., `SDL.CreateWindow()`, `SDL.EventType`, `SDL.WindowFlags`).
- **Use Code Search:** If the location isn't immediately obvious:
  - Use your IDE's search functionality (or a command-line tool like `grep` or `rg`) within the `lib/SDL3-CS/SDL3-CS/SDL/` directory.
  - Search for the PascalCase C# name (e.g., `CreateWindow`).
  - Search for the native C name (e.g., `SDL_CreateWindow`) as it often appears in comments or `EntryPoint` attributes of P/Invoke declarations (e.g., `[LibraryImport(SDLLibrary, EntryPoint = "SDL_CreateWindow")]`).
- **Consult SDL3 Wiki & SDL3-CS Examples:**
  - The official [SDL Wiki](https://wiki.libsdl.org/SDL3/FrontPage) provides documentation for the native SDL3 API. Understanding the native function's purpose and parameters helps.
  - The `SDL3-CS` repository includes examples in `lib/SDL3-CS/SDL3-CS.Examples/` which demonstrate common usage patterns.

**4. Key C# Idioms and Marshalling in SDL3-CS:**

Be aware of common C# idioms used in the bindings:

- **Return Values for Success/Failure:** Many SDL C functions return `0` for success and a negative value for error. In SDL3-CS, these are often converted to `bool`, where `true` indicates success and `false` indicates failure. Use `SDL.GetError()` to get detailed error information. (e.g., `SDL.Init()` returns `bool`).
- **String Marshalling:**
  - `const char*` input parameters in C are often marshalled as `string` in C#.
  - `char*` (for output strings from SDL) or `const char*` return values from SDL might be marshalled as `string`, or sometimes as `IntPtr` requiring manual marshalling (e.g., using `Marshal.PtrToStringUTF8()` or `SDL.PtrToStringUTF8()` if available). SDL3-CS aims for direct `string` usage where idiomatic.
- **Pointer Parameters (`*`, `**`):**
  - Pointers to simple types or structs passed by value to C functions might become `ref` or `out` parameters in C# for structs, or direct value types (`int`, `float`).
  - `SDL_Event*` in C (like in `SDL_PollEvent(SDL_Event* event)`) becomes `out SDL.Event e` or `ref SDL.Event e` in C#.
  - Opaque pointers (handles like `SDL_Window*`, `SDL_Renderer*`) are typically represented as `IntPtr` in C# or wrapped in dedicated C# classes/structs if the binding provides higher-level abstractions. SDL3-CS often uses `IntPtr` for these handles.
- **Enums:** C enums are mapped to C# enums, often with the `[Flags]` attribute if they are bitmasks.
- **Callbacks:** C function pointers for callbacks are mapped to C# delegates.
- **Helper Methods:** The `SDL` static class in SDL3-CS includes various helper methods for marshalling and pointer manipulation (e.g., `SDL.PointerToStructure<T>()`, `SDL.StructureToPointer<T>()`, `SDL.StringToPointer()`). These can be useful if you need to interact with more complex native patterns not fully abstracted by a direct C# method.

- **Troubleshooting SDL Extension Libraries (e.g., SDL3_image, SDL3_ttf) with SDL3-CS:**
  - SDL extension libraries (like `SDL3_image` for image loading or `SDL3_ttf` for font rendering) provide specialized functionality on top of the core SDL3 library. While `SDL3-CS` provides bindings for these, their interaction with core SDL3 features (like the properties system for `SDL_Texture`) might not always be straightforward or fully documented externally.
  - **Problem Identification:** If a function from an SDL extension library (e.g., `SDL3.Image.LoadTexture()`) returns an SDL object (like an `SDL_Texture`), but subsequent attempts to use standard SDL3 mechanisms on that object (e.g., `SDL.GetTextureProperties()` to get dimensions) fail or don't yield expected results, it might indicate that the extension library handles or exposes information differently.
  - **Investigation Strategy:**
        1. **Consult Official SDL Wiki:** First, check the official SDL Wiki (or the specific extension library's documentation, if available and linked) for guidance on the function in question and how it interacts with core SDL types. However, be aware that C# binding specifics might not be covered.
        2. **Examine SDL3-CS Bindings Directly:** If official documentation is insufficient or doesn't clarify the C# binding behavior, the most reliable source of truth is the `SDL3-CS` library's source code itself (located in `lib/SDL3-CS/SDL3-CS/`).
            - Look for the C# wrapper function corresponding to the native SDL extension library function you're using (e.g., in `lib/SDL3-CS/SDL3-CS/Image/PInvoke.cs` for `SDL3_image` functions).
            - See if the extension library offers alternative C# functions within its own namespace (e.g., `SDL3.Image.Load()` to load to an `SDL_Surface` first, from which dimensions can be reliably obtained before converting to an `SDL_Texture`).
            - Check how the C# structs for relevant types (e.g., `SDL.Surface` in `lib/SDL3-CS/SDL3-CS/SDL/Video/surface/Surface.cs`) are defined to understand how to access their members (like `Width`, `Height`) after marshalling an `IntPtr`.
        3. **Consider Intermediate Steps:** Sometimes, an extension library might require or work more reliably with an intermediate step. For example, instead of directly loading an image to an `SDL_Texture`, loading it to an `SDL_Surface` first (using a function from the image extension library), then getting information from the `SDL_Surface` (which is a well-defined core SDL structure), and finally creating the `SDL_Texture` from the `SDL_Surface` using a core SDL function (e.g., `SDL.CreateTextureFromSurface()`) can be a more robust approach. Remember to manage the lifecycle of intermediate objects (like freeing the `SDL_Surface` after the texture is created).
        4. **Error Checking:** Always check return values from SDL functions. For functions from extension libraries, use the standard `SDL.GetError()` to retrieve error messages, as specific `Extension.GetError()` functions may not exist or be necessary.
