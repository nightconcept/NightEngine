# Epic: NightTest Framework Enhancements and CI Integration

**Goal:** To enhance the `NightTest` framework for better modularity, usability, and maintainability, and to integrate its execution into the GitHub Actions CI pipeline for automated validation of the `Night` framework.

## User Stories

- As a Developer, I want `NightTest` to dynamically discover test groups so that adding new tests is more modular and requires no orchestrator modification.
- As a Developer, I want a clear and standardized way to interact with manual tests so that they are easier to execute and less prone to specific keybinding issues.
- As a Developer, I want individual test cases or groups to be able to manage their own configurations if needed, for greater flexibility.
- As a Developer, I want test cases to report failures robustly, even in case of crashes, so that xUnit test results are always informative.
- As a Developer, I want a well-defined `GameTestCase` to reduce boilerplate and provide common utilities for writing tests compatible with xUnit.
- As a Developer, I want the `NightTest` project structure to be clean and consistently named (e.g., `Groups/` instead of `Modules/`).
- As a CI System, I want to build and run `NightTest` automated tests using `dotnet test` and fail the build if any tests fail.
- As a Developer, I want the CI system to archive standard xUnit test result artifacts (e.g., TRX files) for inspection.
- As a Developer, I want `dotnet test` to return an appropriate exit code based on test results for CI integration.

## Tasks

### Phase 0: xUnit Integration & Core Refactoring

- **Task 0.1: Integrate xUnit Framework**
  - **Description:** Add xUnit (xunit, xunit.runner.visualstudio) NuGet packages to the `NightTest.csproj`. This will establish xUnit as the primary test runner.
  - **Implementation:**
    - [x] Add `xunit` NuGet package.
    - [x] Add `xunit.runner.visualstudio` NuGet package to enable test discovery and execution in Visual Studio and by `dotnet test`.
    - [x] Add `Microsoft.NET.Test.Sdk` NuGet package.
  - **Acceptance Criteria:** The `NightTest` project can compile with xUnit dependencies. Tests can be discovered by the xUnit runner. The `NightTest.csproj` OutputType is `Library`.
  - **Status:** Done

- **Task 0.2: Remove Custom Test Orchestration and Reporting**
  - **Description:** Remove the custom `TestRunner` and `Program.cs` test orchestration logic, as xUnit will now handle test discovery, execution, and reporting.
  - **Implementation:**
    - [x] Delete `tests/Core/TestRunner.cs`.
    - [x] Delete `tests/Program.cs` or strip it down if it contains other essential non-test-running logic (unlikely for a test project).
    - [x] Remove any command-line argument parsing related to the old runner.
  - **Acceptance Criteria:** Custom test execution and reporting code is removed. The project relies on xUnit for these functions.
  - **Status:** Done

- **Task 0.3: Adapt `ITestCase` and `GameTestCase` for xUnit**
  - **Description:** Modify `ITestCase` and `GameTestCase` to align with xUnit's execution model. Tests will be xUnit test methods that instantiate and run `IGame` instances.
  - **Implementation:**
    - [x] Review `ITestCase`: Determine if the interface is still fully needed or if its properties (Name, Type, Description) can be primarily managed by xUnit attributes (`[Fact]`, `[Trait]`) on test methods. (Decision: `ITestCase` as is remains useful for defining core metadata contract for test classes.)
    - [x] Modify `GameTestCase`:
      - [x] Remove `SetTestRunner(TestRunner runner)` method and `TestRunner` dependency.
      - [x] `RecordPass`/`RecordFail` logic is removed from `QuitSelf`. `GameTestCase` now sets `CurrentStatus` and `Details` (which are public get, protected set) that the xUnit test method wrapper will assert.
      - [x] `Name`, `Type`, `Description` properties in `GameTestCase` are now `abstract public get`.
      - [x] For failures, `GameTestCase` (or the `IGame` test itself) will either let unhandled exceptions propagate (failing the xUnit test) or set `CurrentStatus` to `Failed` for logical failures, which will be asserted by the xUnit wrapper.
      - [x] The `TestStopwatch` for duration is available in `GameTestCase`; its result will be logged using `ITestOutputHelper` in xUnit wrapper methods (as part of Task 0.4).
  - **Acceptance Criteria:** `GameTestCase` no longer relies on the custom `TestRunner`. `IGame` test outcomes (pass/fail/error) can be effectively translated into xUnit test results. Derived classes are forced to implement `Name`, `Type`, `Description`. `ITestCase` interface is confirmed suitable.
  - **Status:** Done

- **Task 0.4: Create Initial xUnit Test Wrappers**
  - **Description:** Create example xUnit test methods that instantiate and run existing or new `IGame`-based test cases.
  - **Implementation:**
    - [x] Create a new C# test class (e.g., `GraphicsTests.cs`) in the `tests/Groups/Graphics/` directory.
    - [x] Inside this class, define xUnit test methods (e.g., `[Fact] public void Run_GraphicsClearColorTest()`).
    - [x] Each xUnit test method will:
      - Instantiate the corresponding `IGame` test case (e.g., `var myTest = new GraphicsClearColorTest();`).
      - (If needed) Inject `ITestOutputHelper` for logging.
      - Call `Night.Framework.Run(myTest);`.
      - After `Run` completes, use xUnit assertions (`Assert.True()`, `Assert.Equal()`, etc.) based on the outcome of the `IGame` test (e.g., checking a status property set by `GameTestCase` or the `IGame` itself).
  - **Acceptance Criteria:** At least one existing `IGame` test can be successfully executed via an xUnit test method. Test results are reported by xUnit.
  - **Status:** Done

- **Task 0.5: Create xUnit Test Wrappers for Timer Tests**
  - **Description:** Create xUnit test methods for the `IGame`-based test cases defined in `tests/Groups/TimerGroup.cs`.
  - **Implementation:**
    - [x] Create a new C# test class (e.g., `TimerTests.cs`) in the `tests/Groups/` directory.
    - [x] Inside this class, define xUnit test methods for each test case in `TimerGroup.cs` (e.g., `Run_GetTimeTest`, `Run_GetFPSTest`, etc.).
    - [x] Each xUnit test method will:
      - Instantiate the corresponding `IGame` test case (e.g., `var getTimeTest = new GetTimeTest();`).
      - Inject `ITestOutputHelper` for logging.
      - Call `Night.Framework.Run(getTimeTest);`.
      - After `Run` completes, use xUnit assertions to check `CurrentStatus` and log details.
  - **Acceptance Criteria:** All `IGame` tests from `TimerGroup.cs` can be successfully executed via xUnit test methods. Test results are reported by xUnit.
  - **Status:** Done

### Phase 1: NightTest Architectural Enhancements

- **Task 1.1: Verify xUnit Test Discovery and Organization**
  - **Description:** Ensure xUnit correctly discovers all `IGame`-based tests wrapped by xUnit test methods. `ITestGroup` may no longer be necessary for discovery, as xUnit uses attributes and class/method structure. Test organization will rely on xUnit's features (e.g., test classes, `[Trait]` attributes).
  - **Implementation:**
    - [ ] Confirm that all `[Fact]` or `[Theory]` methods that wrap `IGame` instances are discovered by `dotnet test` and/or the Test Explorer in Visual Studio.
    - [ ] Evaluate the role of `ITestGroup`. If its primary purpose was discovery and grouping for the custom runner, it might be deprecated or removed. Test organization can be achieved by structuring xUnit test classes within namespaces and using `[Trait]` attributes for categorization (e.g., `[Trait("Category", "Graphics")]`, `[Trait("TestType", "Manual")]`).
  - **Acceptance Criteria:** All `IGame` tests are discoverable and runnable via xUnit. A clear strategy for organizing tests using xUnit conventions is in place. `ITestGroup`'s role is clarified or it's removed if redundant.
  - **Status:** To Do (was Done, but needs re-evaluation with xUnit)

- **Task 1.2: Standardize Manual Test Interaction**
  - **Description:** Develop a standardized mechanism for manual test pass/fail input, as per the future consideration in [`project/night-test-prd.md:31`](project/night-test-prd.md:31) and [`project/night-test-prd.md:249`](project/night-test-prd.md:249). This moves away from hardcoded keys specific to each manual test.
  - **Implementation:**
    - [x] Design a simple UI overlay (e.g., using `Night.Graphics` to draw "Pass"/"Fail" text/buttons and listen for mouse clicks on them) or a dedicated input phase managed by `GameTestCase`. (Implemented in `GameTestCase` with clickable rectangles for Pass/Fail - `TestRunner` part is no longer applicable).
    - [x] Update `GameTestCase` (Task 0.3, formerly 1.3) to include logic for this standardized interaction. (Done as part of this task for the UI elements and input handling).
    - [x] Refactor existing manual tests (e.g., `ConcreteDummyManualTest` if it exists) to use the new mechanism. (A new test `GraphicsClearColorTest` was created using this mechanism, and `ConcreteDummyManualTest` was removed).
  - **Acceptance Criteria:** Manual tests use a consistent and clear method for user input to signal pass or fail. A sample test demonstrates this. Existing dummy manual tests are removed or refactored.
  - **Status:** Done

- **Task 1.3: Implement and Utilize `GameTestCase`**
  - **Description:** Create and flesh out the `tests/Core/GameTestCase.cs` as per PRD ([`project/night-test-prd.md:165`](project/night-test-prd.md:165), [`project/night-test-prd.md:197`](project/night-test-prd.md:197)) and `project/testing-plan.md`.
  - **Implementation:**
    - [x] Create `tests/Core/GameTestCase.cs` inheriting from `Night.IGame` and implementing `NightTest.Core.ITestCase`.
    - [x] Provide default (virtual) implementations for all `Night.IGame` methods.
    - [x] Implement common `ITestCase` properties (e.g., `Name`, `Type`, `Description` can be abstract or virtual with default).
    - [x] Include a `System.Diagnostics.Stopwatch` (`TestStopwatch`) for measuring total test duration, started in `Load()` and stopped before reporting.
    - [x] Implement `QuitSelf()` method that signals the test is done, records the duration, and calls `Night.Window.Close()`.
    - [x] `GameTestCase` will need methods that allow the `IGame` to signal its outcome (e.g., setting internal status properties like `ActualTestStatus` and `FailureDetails`). The xUnit test method wrapping the `IGame` will then use these properties to make assertions.
    - [x] Remove `SetTestRunner()` and any `TestRunner` dependency.
    - [x] Refactor existing test cases to inherit from `GameTestCase`.
  - **Acceptance Criteria:** `GameTestCase.cs` is implemented and provides useful common functionality. Existing and new test cases inherit from it, reducing boilerplate.
  - **Status:** Complete

- **Task 1.4: Refactor Directory Structure: `Modules` to `Groups`**
  - **Description:** Rename the `tests/Modules/` directory to `tests/Groups/` to align with `ITestGroup` terminology, as suggested in [`project/night-test-prd.md:33`](project/night-test-prd.md:33) and [`project/night-test-prd.md:166`](project/night-test-prd.md:166). Update all relevant namespaces and references.
  - **Implementation:**
    - [x] Rename directory `tests/Modules` to `tests/Groups`.
    - [x] Update namespaces in all test group and test case files previously within `tests/Groups/`.
    - [x] Update `tests/NightTest.csproj` if needed to reflect new paths or if `Program.cs` is removed.
    - [ ] Update documentation (`project/night-test-prd.md`, `project/testing-plan.md`) to consistently refer to `tests/Groups/` and reflect the xUnit changes.
  - **Acceptance Criteria:** Directory is renamed. All code, documentation, and xUnit discovery mechanisms reflect the new `Groups` naming.
  - **Status:** Complete

- **Task 1.5: Clarify and Refactor `tests/Game.cs`**
  - **Description:** Determine the precise role of the existing [`tests/Game.cs`](tests/Game.cs:1). If its functionality is better suited for `GameTestCase` or it's a redundant example, refactor or remove it.
  - **Implementation:**
    - [x] Analyze the functionality provided by [`tests/Game.cs`](tests/Game.cs:1). (Analysis complete: File is unused by the test orchestrator and not referenced elsewhere. It appears to be a redundant example.)
    - [ ] If it provides generic game loop or input handling logic useful for all tests, merge this functionality into `GameTestCase` (Task 1.3). (N/A as file is unused and basic shell)
    - [ ] If it's intended as a very simple, standalone example of an `IGame` for testing the host, ensure it's minimal and perhaps rename it (e.g., `ExampleHostedTest.cs`) and include it in a relevant `ITestGroup` if it's meant to be run as a test. (N/A as it's not used)
    - [x] If its functionality becomes entirely redundant after `GameTestCase` implementation and other refactorings, remove [`tests/Game.cs`](tests/Game.cs:1) and update any references. (Decision: Remove the file.)
  - **Acceptance Criteria:** The role of [`tests/Game.cs`](tests/Game.cs:1) is clarified. The codebase is cleaner, and any useful generic logic is consolidated in `GameTestCase`.
  - **Status:** Complete

- **Task 1.6: Enhance Error Handling within Test Cases and `GameTestCase`**
  - **Description:** Ensure that test cases, particularly through `GameTestCase`, robustly report failures to xUnit, especially in the event of unexpected exceptions during test logic or from `Night.Framework` calls.
  - **Implementation:**
    - [x] `GameTestCase` (or the xUnit test method wrapping it) should implement a top-level try-catch mechanism around the execution of the `IGame`'s `Load`, `Update`, and `Draw` methods (or a central test execution method if applicable). (Implemented in xUnit wrapper methods)
    - [x] Any unhandled exception caught should cause the xUnit test to fail. xUnit automatically captures exception details. (Ensured by rethrowing from wrapper)
    - [x] For non-exception failures determined by the `IGame`'s logic, `GameTestCase` should set status properties that the xUnit test method can assert, causing an `Assert.Fail()` or similar if the status is `Failed`. (Existing functionality)
  - **Acceptance Criteria:** xUnit test results accurately reflect test failures caused by unhandled exceptions or assertion failures within the test case execution, providing error messages and stack traces via standard xUnit reporting.
  - **Status:** Done

- **Task 1.7: Test-Specific Configuration (Optional Enhancement)**
  - **Description:** Investigate and optionally implement a mechanism allowing individual test cases or test groups to load specific configuration files (e.g., a `test_config.json` alongside the test case file).
  - **Implementation:**
    - [ ] Design an approach (e.g., `ITestCase` property for a config file path, convention-based loading like `TestName.config.json`).
    - [ ] If implemented, add helper methods in `GameTestCase` to load and parse such configuration files.
  - **Acceptance Criteria:** (If implemented) Test cases can load their own configurations, allowing for more varied and isolated test scenarios.
  - **Status:** To Do

- **Task 1.8: Asset Management for Tests (Optional Enhancement)**
  - **Description:** Improve the organization of assets used by test cases. Consider allowing per-group or per-test asset subdirectories under `tests/assets/`.
  - **Implementation:**
    - [ ] Define a clear convention for test asset subdirectories (e.g., `tests/assets/GroupName/AssetName`).
    - [ ] Provide a helper method in `GameTestCase` to easily resolve paths to these specific assets.
  - **Acceptance Criteria:** (If implemented) Test assets are organized more logically, making it easier to manage assets for specific tests or test groups.
  - **Status:** To Do

### Phase 2: CI Integration for NightTest

- **Task 2.1: Ensure CI Uses `dotnet test` for Exit Codes**
  - **Description:** The `dotnet test` command, used by xUnit, inherently returns appropriate exit codes (0 for success, non-zero for failures). This task is to ensure the CI script correctly uses `dotnet test` and relies on its standard exit codes.
  - **Implementation:**
    - [x] Verify that the CI workflow (Task 2.2) uses `dotnet test tests/NightTest.csproj ...`.
    - [x] No custom exit code logic is needed in `NightTest` itself.
  - **Acceptance Criteria:** The CI job step running `dotnet test` will correctly pass or fail based on the exit code from `dotnet test`.
  - **Status:** Done

- **Task 2.2: Update `.github/workflows/ci.yml` to Run NightTest via `dotnet test`**
  - **Description:** Modify the existing GitHub Actions CI workflow ([`.github/workflows/ci.yml`](.github/workflows/ci.yml:1)) to build the `NightTest` project and execute its tests using `dotnet test`.
  - **Implementation:**
    - [x] Ensure the `NightTest.csproj` is built as part of the solution build or with a separate `dotnet build tests/NightTest.csproj --configuration Release --no-restore` step.
    - [x] Modify the "Run Tests" step to use `dotnet test tests/NightTest.csproj --configuration Release --no-build`.
    - [x] Filtering for "automated" tests will be done using xUnit's mechanisms, e.g., `dotnet test --filter "TestType=Automated"` if `[Trait("TestType", "Automated")]` is used.
  - **Acceptance Criteria:** The CI workflow successfully builds and runs tests in the `NightTest` project using `dotnet test`. The workflow step correctly fails if `dotnet test` indicates test failures.
  - **Status:** Done

### Phase 3: Code Coverage and Reporting

- **Task 3.1: Implement Code Coverage and Codecov Integration**
  - **Description:** Add code coverage collection to the `NightTest` project and integrate with Codecov.io for reporting coverage metrics in the CI pipeline.
  - **Implementation:**
    - [x] Add `coverlet.collector` and `coverlet.msbuild` NuGet packages to `tests/NightTest.csproj`.
    - [x] Update the `dotnet test` command in `.github/workflows/ci.yml` to collect coverage information (e.g., using `/p:CollectCoverage=true /p:CoverletOutputFormat=opencover`).
    - [x] Add a step in `.github/workflows/ci.yml` to upload coverage reports to Codecov.io using the `codecov/codecov-action`.
    - [ ] Ensure the Codecov step is configured with the appropriate `CODECOV_TOKEN` secret.
  - **Acceptance Criteria:** Code coverage is collected during CI test runs. Coverage reports are successfully uploaded to and viewable on Codecov.io. The `NightTest.csproj` and `.github/workflows/ci.yml` files are updated accordingly.
  - **Status:** To Do
