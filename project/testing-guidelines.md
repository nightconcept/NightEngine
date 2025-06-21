# Night Engine Testing Guidelines

This document provides guidelines for writing tests for the Night Engine, specifically for the `Night.Framework` and future `Night.Engine` components. Our testing framework is built using xUnit and a custom set of base classes. There are three primary types of test cases: `GameTestCase` for automated tests running within the game environment, `ManualTestCase` for tests requiring user validation within the game environment, and `ModTestCase` for module-level tests that run in isolation without a full game instance.

## Table of Contents

- [Night Engine Testing Guidelines](#night-engine-testing-guidelines)
  - [Table of Contents](#table-of-contents)
  - [1. Overview](#1-overview)
  - [2. Core Concepts](#2-core-concepts)
    - [Test Groups](#test-groups)
    - [Test Cases](#test-cases)
      - [`GameTestCase`](#gametestcase)
      - [`ManualTestCase`](#manualtestcase)
      - [`ModTestCase`](#modtestcase)
    - [Automated vs. Manual Tests](#automated-vs-manual-tests)
  - [3. Directory Structure](#3-directory-structure)
  - [4. Naming Conventions](#4-naming-conventions)
    - [Files](#files)
    - [Classes](#classes)
    - [Methods in Test Groups](#methods-in-test-groups)
    - [Test Case `Name` Property](#test-case-name-property)
  - [5. Creating Tests: Step-by-Step](#5-creating-tests-step-by-step)
    - [Step 1: Identify the Module and Feature](#step-1-identify-the-module-and-feature)
    - [Step 2: Choose the Appropriate Test Case Type](#step-2-choose-the-appropriate-test-case-type)
    - [Step 3: Create a Test Case File and Class](#step-3-create-a-test-case-file-and-class)
      - [`GameTestCase` Example](#gametestcase-example)
      - [`ManualTestCase` Example](#manualtestcase-example)
      - [`ModTestCase` Example](#modtestcase-example)
    - [Step 4: Implement Test Logic](#step-4-implement-test-logic)
      - [For `GameTestCase` and `ManualTestCase` (`IGame` Methods)](#for-gametestcase-and-manualtestcase-igame-methods)
      - [For `ModTestCase`](#for-modtestcase)
    - [Step 5: Manage Test Lifecycle and Status (`GameTestCase`/`ManualTestCase`)](#step-5-manage-test-lifecycle-and-status-gametestcasemanualtestcase)
    - [Step 6: Create or Update the Test Group](#step-6-create-or-update-the-test-group)
  - [6. Key Base Classes and Interfaces](#6-key-base-classes-and-interfaces)
  - [7. Best Practices](#7-best-practices)

## 1. Overview

The primary goal of our testing strategy is to ensure the reliability and correctness of the `Night Engine`'s features.
Tests are organized into `TestGroup` classes, which are discovered and run by xUnit.
There are three main types of test cases:

- **`GameTestCase`**: These tests implement [`Night.IGame`](../src/Night/IGame.cs) and run within the engine's main loop, allowing for automated testing of features that require a game context (e.g., graphics, input, windowing).
- **`ManualTestCase`**: A specialization of `GameTestCase`, these also run as `IGame` instances but are designed for scenarios requiring manual user confirmation (e.g., visual verification of rendering).
- **`ModTestCase`**: These tests are designed for testing individual functions, classes, or modules in isolation, much like traditional unit tests. They do not run as `IGame` instances and are suitable for testing logic that doesn't require a game window or full engine loop.

## 2. Core Concepts

### Test Groups

- **Purpose:** To group related test cases for a specific module or major feature (e.g., `Filesystem`, `Graphics`, `Configuration`).
- **Implementation:** Create a C# class that inherits from [`NightTest.Core.TestGroup`](../tests/Core/TestGroup.cs).
- **Location:** `tests/Groups/[ModuleName]/[ModuleName]Group.cs`.
- **Example:** [`ConfigurationGroup.cs`](../tests/Groups/Configuration/ConfigurationGroup.cs)

### Test Cases

#### `GameTestCase`

- **Purpose:** An individual automated test scenario that verifies specific functionality requiring the engine's runtime environment (e.g., interacting with the game window, graphics, or input systems).
- **Implementation:** Create a C# class that:
  - Implements [`Night.IGame`](../src/Night/IGame.cs).
  - Inherits from [`NightTest.Core.GameTestCase`](../tests/Core/GameTestCase.cs).
- **Logic:** Test assertions and status updates (`CurrentStatus`, `Details`) are typically handled within the `Update(double deltaTime)` method. The test concludes by calling `EndTest()`.
- **Location:** Typically in `tests/Groups/[ModuleName]/[FeatureName]Tests.cs`.
- **`Type` Property:** `TestType.Automated`.

#### `ManualTestCase`

- **Purpose:** An individual test scenario that requires user interaction or visual confirmation to determine pass/fail status. These tests also run within the engine's runtime environment.
- **Implementation:** Create a C# class that:
  - Implements [`Night.IGame`](../src/Night/IGame.cs).
  - Inherits from [`NightTest.Core.ManualTestCase`](../tests/Core/ManualTestCase.cs) (which itself inherits from `GameTestCase`).
- **Logic:** Uses the `RequestManualConfirmation("Prompt message")` method, typically within `Update(double deltaTime)`, to prompt the user. The base `ManualTestCase` handles UI for pass/fail and timeout.
- **Location:** Typically in `tests/Groups/[ModuleName]/[FeatureName]Tests.cs`.
- **`Type` Property:** `TestType.Manual`.
- **Example:** [`GraphicsClearColorTest`](../tests/Groups/Graphics/GraphicsClearTest.cs)

#### `ModTestCase`

- **Purpose:** An individual automated test scenario for testing specific functions, methods, or classes in isolation, without needing a full game loop or window. Ideal for unit-testing modules.
- **Implementation:** Create a C# class that:
  - Inherits from [`NightTest.Core.ModTestCase`](../tests/Core/ModTestCase.cs).
- **Logic:** Test logic and assertions (using xUnit's `Assert` class) are placed within the overridden `Run()` method. A `SuccessMessage` property defines the message upon successful completion without exceptions.
- **Location:** Typically in `tests/Groups/[ModuleName]/[FeatureName]Tests.cs` or `[SpecificTestName]Test.cs`.
- **`Type` Property:** `TestType.Automated` (as set by the underlying `BaseTestCase`).
- **Example:** [`ConfigurationGameConfig_GetSet`](../tests/Groups/Configuration/GameConfigTest.cs) (from `GameConfigTest.cs`)

### Automated vs. Manual Tests

- **Automated Tests:**
  - These tests run and determine their pass/fail status programmatically.
  - Includes all tests inheriting from `GameTestCase` (that are not `ManualTestCase`) and all tests inheriting from `ModTestCase`.
  - Marked with `[Trait("TestType", "Automated")]` in the `TestGroup`.
- **Manual Tests:**
  - These tests require human intervention to verify the outcome.
  - Implemented by inheriting from `ManualTestCase`.
  - Marked with `[Trait("TestType", "Manual")]` in the `TestGroup`.

## 3. Directory Structure

```plaintext
tests/
├── Core/                 # Base classes, interfaces, and core testing utilities
│   ├── BaseTestCase.cs
│   ├── GameTestCase.cs
│   ├── ManualTestCase.cs
│   ├── ModTestCase.cs
│   ├── ITestCase.cs
│   ├── TestGroup.cs
│   ├── TestTypes.cs      # Contains TestStatus, TestType enums
│   └── ...
└── Groups/               # Contains groups of tests, organized by engine module
    └── [ModuleName]/     # e.g., Filesystem, Graphics, Configuration, Window
        ├── [ModuleName]Group.cs  # xUnit test class for the group
        ├── [Feature1]Tests.cs    # Contains GameTestCase/ManualTestCase/ModTestCase classes for Feature1
        └── [Feature2]Tests.cs    # Contains GameTestCase/ManualTestCase/ModTestCase classes for Feature2
```

## 4. Naming Conventions

Consistency in naming is crucial for maintainability.

### Files

- **Test Group Files:** `[ModuleName]Group.cs` (e.g., `FilesystemGroup.cs`, `ConfigurationGroup.cs`)
- **Test Case Files:** `[FeatureName]Tests.cs` or `[SpecificTestName]Test.cs`. Generally, group related test cases for a sub-feature into one file. (e.g., `LinesTests.cs`, `GraphicsClearTest.cs`, `GameConfigTest.cs`).

### Classes

- **Test Group Classes:** `[ModuleName]Group` (e.g., `FilesystemGroup`, `ConfigurationGroup`).
  - Example: `public class FilesystemGroup : TestGroup`
- **`GameTestCase` / `ManualTestCase` Classes:** `[ModuleName][FeatureName]_[SpecificBehavior]Test` or a descriptive name if it covers a broader feature.
  - Example: `public class FilesystemLines_ReadStandardFileTest : GameTestCase`
  - Example: `public class GraphicsClearColorTest : ManualTestCase` (as in [`GraphicsClearTest.cs`](../tests/Groups/Graphics/GraphicsClearTest.cs))
- **`ModTestCase` Classes:** `[ModuleName][ClassName]_[MethodOrBehavior]Test` or `[ModuleName][FeatureName]_[SpecificBehavior]Test`.
  - Example: `public class ConfigurationGameConfig_GetSet : ModTestCase` (as in [`GameConfigTest.cs`](../tests/Groups/Configuration/GameConfigTest.cs))

### Methods in Test Groups

- Related `GameTestCase` instances (automated tests requiring the game environment) should be grouped into a single `[Fact]` method.
- `ManualTestCase` instances can either be run by a dedicated `[Fact]` method or grouped similarly if appropriate.
- For `ModTestCase` instances, related tests (e.g., all tests for a specific function or a closely related set of behaviors within a module) should also be grouped into a single `[Fact]` method.
- **Method Name for consolidated `GameTestCase` group:** `Run_[ModuleName][FeatureOrConcept]_GameTests()` (e.g., `public void Run_Timer_GameTests()`).
  - This method will contain multiple calls to `this.Run_GameTestCase(new ...Test());`.
- **Method Name for individual `ManualTestCase`:** `Run_[TestCaseClassName]()` (e.g., `public void Run_GraphicsClearColorTest()`). If grouped, follow a similar pattern to `GameTestCase` or `ModTestCase` consolidated groups.
- **Method Name for consolidated `ModTestCase` group:** `Run_[ModuleName][FeatureOrFunction]_ModTests()` (e.g., `public void Run_ConfigurationManager_ModTests()`).
  - This method will then contain multiple calls to `this.Run_ModTestCase(new ...Test());` for each individual test case class belonging to that group.

### Test Case `Name` Property

- This property is part of the `ITestCase` interface (implemented by `BaseTestCase`) and is used for logging and identification.
- **Format:** `"[ModuleName].[FeatureNameOrClass].[SpecificBehaviorOrConcept]"`
  - `GameTestCase`/`ManualTestCase` Example: `public override string Name => "Graphics.Clear";` (from [`GraphicsClearTest.cs`](../tests/Groups/Graphics/GraphicsClearTest.cs))
  - `ModTestCase` Example: `public override string Name => "Configuration.GameConfig";` (from [`GameConfigTest.cs`](../tests/Groups/Configuration/GameConfigTest.cs))

## 5. Creating Tests: Step-by-Step

### Step 1: Identify the Module and Feature

Determine which part of the `Night Engine` you are testing. This will dictate the directory and naming. Refer to [`project/PRD.md`](../project/PRD.md) for module names (e.g., `Night.Window`, `Night.Graphics`, `Night.Filesystem`, `Night.Configuration`).

### Step 2: Choose the Appropriate Test Case Type

- **`GameTestCase`**: For automated tests that need the game loop, graphics context, input handling, or other engine systems that require an active `IGame` instance.
- **`ManualTestCase`**: For tests requiring visual confirmation or specific user interactions within the game window.
- **`ModTestCase`**: For testing specific C# classes, methods, or logic in isolation, where a game window or the full engine loop is unnecessary (similar to traditional unit tests).

### Step 3: Create a Test Case File and Class

1. Navigate to `tests/Groups/[ModuleName]/`.
2. Create a new C# file, e.g., `[FeatureName]Tests.cs` or `[SpecificTestName]Test.cs`.
3. Inside this file, define your test case class, inheriting from the chosen base class.

#### `GameTestCase` Example

```csharp
// In tests/Groups/MyModule/MyFeatureGameTests.cs
using NightTest.Core;
using Night; // For IGame, engine APIs

namespace NightTest.Groups.MyModule
{
    public class MyModuleMyFeature_ExpectedBehaviorGameTest : GameTestCase
    {
        public override string Name => "MyModule.MyFeature.ExpectedBehaviorGame";
        public override string Description => "Tests that MyFeature exhibits expected behavior within the game loop.";
        // Type defaults to TestType.Automated via BaseTestCase

        protected override void Load()
        {
            base.Load(); // Important for base class setup
            // Your test setup logic here (e.g., initialize variables, load assets)
            this.Details = "Setting up MyFeature test...";
        }

        protected override void Update(double deltaTime)
        {
            if (this.IsDone) return;

            // Your test assertion logic here
            bool conditionMet = Night.MyModule.MyFeature.DoSomething(); // Example call
            if (conditionMet)
            {
                this.CurrentStatus = TestStatus.Passed;
                this.Details = "MyFeature.DoSomething() returned true as expected.";
            }
            else
            {
                this.CurrentStatus = TestStatus.Failed;
                this.Details = "MyFeature.DoSomething() returned false, expected true.";
            }
            this.EndTest(); // Call when test logic is complete
        }

        protected override void Draw()
        {
            // Optional: drawing for debugging or if the test involves visuals
            // Night.Graphics.Clear(Color.Black);
            // Night.Graphics.DrawString("Testing MyFeature...", 10, 10);
        }
    }
}
```

#### `ManualTestCase` Example

(See [`GraphicsClearTest.cs`](../tests/Groups/Graphics/GraphicsClearTest.cs) for a concrete example.)

```csharp
// In tests/Groups/MyModule/MyVisualFeatureTests.cs
using NightTest.Core;
using Night; // For IGame, engine APIs

namespace NightTest.Groups.MyModule
{
    public class MyModuleMyVisualFeature_UserConfirmationTest : ManualTestCase
    {
        public override string Name => "MyModule.MyVisualFeature.UserConfirmation";
        public override string Description => "User must visually confirm that MyVisualFeature renders correctly.";
        // Type is TestType.Manual via ManualTestCase

        protected override void Load()
        {
            base.Load();
            // Setup for visual output
            this.Details = "Test running, observe the visual output.";
        }

        protected override void Update(double deltaTime)
        {
            if (this.IsDone) return;

            // Logic to trigger the visual effect if needed

            // Request confirmation after a short delay or when ready
            // ManualTestPromptDelayMilliseconds can be used from base class
            if (this.TestStopwatch.ElapsedMilliseconds > this.ManualTestPromptDelayMilliseconds)
            {
                this.RequestManualConfirmation("Does the screen display [expected visual outcome] correctly?");
            }
        }

        protected override void Draw()
        {
            // Your drawing logic here
            // Night.Graphics.Clear(Color.CornflowerBlue);
            // Night.MyModule.MyVisualFeature.Render();
        }
    }
}
```

#### `ModTestCase` Example

(See [`ConfigurationGameConfig_GetSet`](../tests/Groups/Configuration/GameConfigTest.cs) from `GameConfigTest.cs` for a concrete example.)

```csharp
// In tests/Groups/MyModule/MyLogicTests.cs
using NightTest.Core;
using Night; // For engine classes being tested, e.g., Night.Configuration.GameConfig
using Xunit; // For Assertions

namespace NightTest.Groups.MyModule
{
    public class MyModuleMyLogic_SpecificFunctionTest : ModTestCase
    {
        public override string Name => "MyModule.MyLogic.SpecificFunction";
        public override string Description => "Tests the SpecificFunction of MyLogic class.";
        public override string SuccessMessage => "MyLogic.SpecificFunction tested successfully.";
        // Type defaults to TestType.Automated via BaseTestCase

        public override void Run()
        {
            // Arrange
            var myObject = new Night.MyModule.MyClassToTest();
            var input = "someInput";
            var expectedOutput = "expectedResult";

            // Act
            var actualResult = myObject.SpecificFunction(input);

            // Assert
            Assert.Equal(expectedOutput, actualResult);
            // Add more assertions as needed
        }
    }
}
```

### Step 4: Implement Test Logic

#### For `GameTestCase` and `ManualTestCase` ([`IGame`](../src/Night/IGame.cs) Methods)

Override methods from `GameTestCase` or `ManualTestCase` as needed.

- **`Load()`**:
  - Called once when the test case starts. Use for one-time setup.
  - **Always call `base.Load();`** if you override it.
  - If setup fails, set `this.CurrentStatus = TestStatus.Failed;`, provide `this.Details`, and call `this.EndTest();`.
- **`Update(double deltaTime)`**:
  - Called every frame. Main test logic and assertions reside here.
  - Check `if (this.IsDone) return;` at the beginning.
  - For automated `GameTestCase`: Perform actions, check conditions, set `this.CurrentStatus` and `this.Details`, then call `this.EndTest();`.
  - For `ManualTestCase`: Call `this.RequestManualConfirmation("Your question");` when ready for user input. The base class handles `EndTest()`.
- **`Draw()`**:
  - Called every frame, after `Update()`. Use for rendering required by the test, especially for `ManualTestCase`.
  - For `ManualTestCase`, UI buttons and `Night.Graphics.Present()` are handled by the base class after your `Draw()` logic.
- **Input Handling (`KeyPressed`, `MousePressed`, etc.)**:
  - Override if your test needs to react to input.
  - If overriding in a `ManualTestCase`, ensure you call the `base` method (e.g., `base.KeyPressed(...)`) or replicate necessary base logic for UI interaction.

#### For `ModTestCase`

- **`Run()`**:
  - This is the primary method where all test logic is implemented.
  - Follow the Arrange-Act-Assert pattern.
  - Use xUnit's `Assert` static class methods (e.g., `Assert.Equal()`, `Assert.True()`, `Assert.Throws<Exception>()`) for verifications.
  - The test passes if `Run()` completes without throwing an unhandled exception and all assertions pass. It fails if an assertion fails (which throws an exception) or any other unhandled exception occurs.
- **`SuccessMessage` (property)**:
  - Define a string that will be used as the `Details` if the test's `Run()` method completes successfully.
- **`Name` and `Description` (properties)**:
  - Provide clear and concise information about the test.

### Step 5: Manage Test Lifecycle and Status (`GameTestCase`/`ManualTestCase`)

This section primarily applies to `GameTestCase` and `ManualTestCase`. `ModTestCase` lifecycle is simpler and managed more directly by its `Run()` method and xUnit assertions.

- **`this.CurrentStatus`**: Set to `TestStatus.Passed` or `TestStatus.Failed`. Defaults to `TestStatus.NotRun`.
- **`this.Details`**: Provide a clear string explaining the test outcome or failure reason.
- **`this.EndTest()`**: Call this method when the test logic is complete to stop the test, close the window (if applicable), and record results.
- **Helper methods in `GameTestCase`**:
  - `CheckCompletionAfterDuration(...)`: End test after a certain time.
  - `CheckCompletionAfterFrames(...)`: End test after a certain number of frames.
- **Error Handling**: Wrap potentially problematic code in `try-catch` blocks. On exception, set status and details, then call `EndTest()`.

    ```csharp
    catch (System.Exception e)
    {
        this.RecordFailure($"An unexpected error occurred: {e.GetType().Name} - {e.Message}", e);
        // EndTest() is called by RecordFailure if the test is not already done.
    }
    ```

- **Cleanup**: Clean up resources (e.g., temporary files) in a `finally` block or ensure cleanup before `EndTest()`.

### Step 6: Create or Update the Test Group

1. Open or create `tests/Groups/[ModuleName]/[ModuleName]Group.cs`.
2. Ensure it inherits from `NightTest.Core.TestGroup` and has a constructor accepting `ITestOutputHelper`.

    ```csharp
    // In tests/Groups/MyModule/MyModuleGroup.cs
    using Xunit;
    using Xunit.Abstractions;
    using NightTest.Core;
    // Potentially: using NightTest.Groups.MyModule; // If test cases are in the same namespace

    namespace NightTest.Groups.MyModule
    {
        [Collection("SequentialTests")] // Important for tests that interact with the game window
        public class MyModuleGroup : TestGroup
        {
            public MyModuleGroup(ITestOutputHelper outputHelper)
                : base(outputHelper)
            {
            }

            // Add [Fact] methods for each test case here
        }
    }
    ```

3. Add a new xUnit `[Fact]` method for each `GameTestCase`/`ManualTestCase`, or for each group of related `ModTestCase` instances:

    - **For a consolidated group of `GameTestCase` instances:**

        ```csharp
        [Fact]
        [Trait("TestType", "Automated")] // GameTestCases are automated
        public void Run_MyModule_GameTests() // Consolidated method name for GameTestCases
        {
            this.Run_GameTestCase(new MyModuleMyFeature_ExpectedBehaviorGameTest());
            this.Run_GameTestCase(new MyModuleAnotherFeature_SomeConditionGameTest());
            // ... and so on for all GameTestCases related to MyModule
        }
        ```

    - **For an individual `ManualTestCase` (or a consolidated group):**
        `ManualTestCase` instances can still be run individually:

        ```csharp
        [Fact]
        [Trait("TestType", "Manual")]
        public void Run_MyModuleMyVisualFeature_UserConfirmationTest() // Matches test case class name
        {
            this.Run_GameTestCase(new MyModuleMyVisualFeature_UserConfirmationTest()); // Note: Run_GameTestCase is used for ManualTestCase as well
        }
        ```

        Alternatively, if multiple `ManualTestCase`s for a module are grouped, follow a similar consolidated structure as `GameTestCase` or `ModTestCase` groups, using `[Trait("TestType", "Manual")]`.

    - **For a group of `ModTestCase` instances:**
        The `TestGroup` class needs a method to execute individual `ModTestCase` instances. This method (`Run_ModTestCase(ModTestCase testCase)`) is responsible for handling the execution, logging, and status recording of a single `ModTestCase`.

        ```csharp
        // This method should exist in your NightTest.Core.TestGroup class
        protected void Run_ModTestCase(ModTestCase testCase)
        {
            _outputHelper.WriteLine($"--- Starting Mod Test: {testCase.Name} ---");
            _outputHelper.WriteLine($"Description: {testCase.Description}");
            testCase.PrepareForRun(); // Sets up stopwatch etc.
            try
            {
                testCase.Run(); // Execute the core test logic and assertions
                testCase.RecordSuccess(testCase.SuccessMessage); // If Run() completes, it's a pass
                _outputHelper.WriteLine($"Mod Test {testCase.Name}: PASSED. Details: {testCase.Details}");
            }
            catch (System.Exception ex)
            {
                // Assertions in xUnit throw exceptions on failure. Other exceptions also indicate failure.
                testCase.RecordFailure($"Test failed: {ex.Message}", ex);
                _outputHelper.WriteLine($"Mod Test {testCase.Name}: FAILED. Details: {testCase.Details}\n{ex.StackTrace}");
                throw; // Re-throw to ensure xUnit marks the test as failed
            }
            finally
            {
                testCase.FinalizeRun(); // Stops stopwatch
                _outputHelper.WriteLine($"--- Finished Mod Test: {testCase.Name} --- Duration: {testCase.TestStopwatch.ElapsedMilliseconds}ms");
            }
            Assert.Equal(TestStatus.Passed, testCase.CurrentStatus); // Final assertion on the status
        }
        ```

        Then, in your specific `[ModuleName]Group.cs`, create a single `[Fact]` method to run all related `ModTestCase`s:

        ```csharp
        [Fact]
        [Trait("TestType", "Automated")] // ModTests are inherently automated
        public void Run_MyModuleMyLogic_ModTests() // Consolidated method name
        {
            this.Run_ModTestCase(new MyModuleMyLogic_SpecificFunctionTest());
            this.Run_ModTestCase(new MyModuleMyLogic_AnotherFunctionTest());
            this.Run_ModTestCase(new MyModuleMyLogic_EdgeCaseTest());
            // ... and so on for all ModTestCases related to MyModuleMyLogic
        }
        ```

    - The `[Trait("TestType", ...)]` attribute helps in filtering tests.
    - For `ModTestCase` groups, instantiate each test case class and pass it to `this.Run_ModTestCase()` within the consolidated `[Fact]` method.

## 6. Key Base Classes and Interfaces

- **[`Night.IGame`](../src/Night/IGame.cs)**: Interface from the core engine that `GameTestCase` (and thus `ManualTestCase`) implement to hook into the game loop (`Load`, `Update`, `Draw`, input methods).
- **[`NightTest.Core.ITestCase`](../tests/Core/ITestCase.cs)**: Defines properties for test identification (`Name`, `Type`, `Description`, `CurrentStatus`, `Details`, `TestStopwatch`). Implemented by `BaseTestCase`.
- **[`NightTest.Core.BaseTestCase`](../tests/Core/BaseTestCase.cs)**: Common abstract base class for all test cases. Provides shared properties from `ITestCase` and basic methods like `RecordFailure`, `RecordSuccess`. `Type` defaults to `TestType.Automated`.
- **[`NightTest.Core.GameTestCase`](../tests/Core/GameTestCase.cs)**:
  - Inherits from `BaseTestCase` and implements `IGame`.
  - Base class for automated tests that run within the game loop.
  - Provides common game loop integration, frame counting, `EndTest()`, and helper methods like `CheckCompletionAfterDuration()`, `CheckCompletionAfterFrames()`.
- **[`NightTest.Core.ManualTestCase`](../tests/Core/ManualTestCase.cs)**:
  - Inherits from `GameTestCase`.
  - Overrides `Type` to `TestType.Manual`.
  - Provides UI for manual pass/fail confirmation (buttons, ESC key), timeout logic, and the `RequestManualConfirmation()` method.
- **[`NightTest.Core.ModTestCase`](../tests/Core/ModTestCase.cs)**:
  - Inherits from `BaseTestCase`.
  - Base class for automated tests that run in isolation (unit/module tests).
  - Requires implementation of an abstract `Run()` method for test logic and an abstract `SuccessMessage` property.
  - Includes `PrepareForRun()` and `FinalizeRun()` methods called by the test group.
- **[`NightTest.Core.TestGroup`](../tests/Core/TestGroup.cs)**:
  - Base class for xUnit test classes (the classes containing `[Fact]` methods).
  - Takes `ITestOutputHelper` for logging.
  - Provides `Run_GameTestCase(GameTestCase testCase)` to execute `IGame`-based tests.
  - Provides `Run_ModTestCase(ModTestCase testCase)` to execute individual `ModTestCase` instances. `[Fact]` methods in the `TestGroup` will call this multiple times if consolidating several `ModTestCase`s.
- **[`NightTest.Core.TestType`](../tests/Core/TestTypes.cs)**: Enum (`Automated`, `Manual`).
- **[`NightTest.Core.TestStatus`](../tests/Core/TestTypes.cs)**: Enum (`NotRun`, `Passed`, `Failed`, `Skipped`).

## 7. Best Practices

- **Choose the Right Test Type:** Carefully consider if your test needs the game environment (`GameTestCase`/`ManualTestCase`) or can be done in isolation (`ModTestCase`). Prefer `ModTestCase` for unit-level logic testing for speed and simplicity.
- **Clear Naming and Descriptions:** Ensure `ITestCase.Name` and `ITestCase.Description` clearly state what the test is verifying. For `GameTestCase`/`ManualTestCase`, `Details` property should provide context on pass/fail. For `ModTestCase`, `SuccessMessage` is key.
- **Focused Tests:** Each test case should verify a single, specific piece of functionality or behavior.
- **Idempotency (for automated tests):** Automated tests (`GameTestCase`, `ModTestCase`) should produce the same result every time they are run, assuming no code changes. Avoid dependencies on external state not controlled by the test's setup (`Load()` or `Run()`).
- **Resource Management:** Clean up any resources created during a test (e.g., temporary files), typically in a `finally` block or before `EndTest()`.
- **Readability:** Write clean, understandable test code. Comments should explain *why* something is being done if it's not obvious.
- **Use `[Collection("SequentialTests")]`:** For `TestGroup` classes that contain `GameTestCase` or `ManualTestCase` instances, especially those involving UI or window manipulation, to ensure tests run one after another and don't interfere. This may not be strictly necessary for groups containing only `ModTestCase` instances if they are fully isolated.
- **Test Both Success and Failure Cases:** For a given feature, consider creating tests for expected successful outcomes and expected failure conditions (e.g., invalid input, file not found, exceptions).
- **Keep Manual Tests for Visuals/Interaction:** Reserve `ManualTestCase` for scenarios that genuinely require human observation (e.g., "is this color correct?", "does this animation look right?") or complex interactions not easily automated.
- **Assertions in `ModTestCase`:** Use xUnit's `Assert` methods directly within the `Run()` method of your `ModTestCase`.
- **`TestGroup` Responsibility:** The `TestGroup`'s `Run_GameTestCase` and `Run_ModTestCase` methods are responsible for the overall execution flow, logging, and final assertion on the test case's `CurrentStatus`.
- **ALWAYS** write XML summaries for public API or use inheritdoc where appropriate.
- Order tests correctly:
  1. Fields
  2. Constructors
  3. Finalizers
  4. Delegates
  5. Events
  6. Enums
  7. Interfaces
  8. Properties
  9. Indexers
  10. Methods
      1. Public
      2. Internal
      3. Protected
      4. Private
  11. Structs
  12. Classes (Nested)
- Static members should appear before instance members.
- **Running Tests**: Test must be run using this command while doing development (except for manual tests): `SDL_VIDEODRIVER=dummy dotnet test --filter TestType=Automated` instead of `dotnet test`. This is because headless testing is the preferred method.
