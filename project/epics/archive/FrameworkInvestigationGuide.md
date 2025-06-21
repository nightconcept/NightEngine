# Night.Framework.Run() Investigation Guide for CI Test Failures

## 1. Summary of Findings from Test Suite Debugging

Efforts to stabilize the test suite, particularly for `Timer` tests and the `Run_GetErrorTest` on CI environments (Linux/Windows), have revealed the following:

- **Initial State:** Tests were failing with `Actual: NotRun` and `Details: Test has not started.` or `Details: Test is running...`. This indicated that the test logic was not completing or `CurrentStatus` was not being updated correctly.
- **Sequential Execution Implemented:**
  - A `SequentialTestCollection` was created in [`tests/Core/SequentialTestCollection.cs`](tests/Core/SequentialTestCollection.cs:1).
  - Test group classes (e.g., `SDLGroup`, `TimerGroup`) were decorated with `[Collection("SequentialTests")]` to prevent race conditions, which were a known issue with SDL resource management as documented in [`project/race-condition.md`](project/race-condition.md:1).
- **Robust Error Handling in `GameTestCase`:**
  - The `IGame.Load()` method in [`tests/Core/GameTestCase.cs`](tests/Core/GameTestCase.cs:81) now correctly initializes `CurrentStatus` to `TestStatus.NotRun` and `Details` to "Test is running...".
  - The `IGame.Update()` method in [`tests/Core/GameTestCase.cs`](tests/Core/GameTestCase.cs:99) now includes a `try-catch-finally` block:
    - The `catch` block records any unhandled exception from a test's `Update` logic and sets `CurrentStatus = TestStatus.Failed`.
    - The `finally` block ensures `EndTest()` is called if `!IsDone`. Crucially, if `CurrentStatus` is still `TestStatus.NotRun` when this safety net `EndTest()` is invoked, `CurrentStatus` is set to `TestStatus.Failed` with the detail "Test did not complete its logic and was ended by the framework's safety net. Status was still NotRun."
- **Current Test Outcome:**
  - With these changes, the `Timer` tests (and likely any other test that terminates prematurely) now consistently report `Actual: Failed` with the aforementioned detail message.
  - The test durations are extremely short (e.g., 0-7ms) for these failing tests.
  - This indicates that `Night.Framework.Run(testCase)` (called from [`tests/Core/TestGroup.cs`](tests/Core/TestGroup.cs:65)) is terminating before the test's own logic (e.g., `CheckCompletionAfterDuration` in timer tests) can complete and set a `Passed` status.

## 2. Hypothesis for Premature Termination

The primary hypothesis is that the `Night.Framework.Run()` main loop is exiting prematurely on CI environments for tests that do not inherently maintain an active SDL window or event queue. This could be due to:

- **SDL Event Loop Behavior:** If the SDL event loop within `Night.Framework.Run()` doesn't receive events or if it detects the window is no longer active/valid (especially if one was expected but isn't properly managed for "headless" tests), it might decide to terminate.
- **Window Management:** For tests that don't require a visible window (like many `Timer` tests or the `Run_GetErrorTest`), the way the framework initializes or manages (or doesn't manage) an SDL window could lead to an early exit. SDL applications typically require an active window and event processing to keep running.
- **Platform-Specific SDL Behavior:** SDL's behavior, or the behavior of the underlying graphics/windowing system, might differ on Linux/Windows CI environments compared to local macOS/Windows, especially in headless or minimal CI setups.

## 3. Next Actions for Investigating `Night.Framework.Run()`

The goal is to understand why the main loop in `Night.Framework.Run()` is not persisting long enough for tests (especially duration-based timer tests) to complete their logic on CI platforms.

**Location of `Night.Framework.Run()`:** Based on the PRD ([`project/PRD.md`](project/PRD.md:235)), this is likely in `src/Night/FrameworkLoop.cs` or a similar central framework file.

**Suggested Investigation Steps:**

1. **Review `Night.Framework.Run()` Logic:**
    - Thoroughly examine the main loop structure.
    - Identify the conditions under which the loop terminates.
    - How are SDL events polled and handled? (e.g., `SDL.PollEvent()`)
    - Is there a specific check for `SDL.EventType.Quit` or window close events that might be triggered unexpectedly?
2. **Logging within `Night.Framework.Run()`:**
    - Add detailed logging (using `Night.Log.LogManager`) at the beginning and end of the `Run` method.
    - Log each iteration of the main loop.
    - Log the type of SDL events being received, if any.
    - Log the status of `Night.Window.IsOpen()` within the loop.
    - Log any explicit calls to `Night.Window.Close()` or SDL shutdown functions from within the framework itself (outside of `GameTestCase.EndTest()`).
3. **SDL Initialization and Window Handling for Tests:**
    - How and when is `SDL.Init()` called relative to `Night.Framework.Run()`?
    - Is an SDL window explicitly created by `Night.Framework.Run()` or assumed to be created by the `IGame` instance (e.g., in its `Load` method via `Night.Window.SetMode()`)?
    - If tests like `Timer` tests don't call `Night.Window.SetMode()`, does the framework attempt to run without a window, and how does SDL behave in that scenario?
    - Consider if a minimal, perhaps even hidden, SDL window needs to be consistently created and managed by `Night.Framework.Run()` for the duration of any `IGame` execution to ensure the SDL event loop remains active.
4. **Examine `IGame` Callbacks:**
    - Log entry and exit for `game.Load()`, `game.Update(deltaTime)`, and `game.Draw()` within the `Night.Framework.Run()` loop to ensure they are being called as expected.
5. **Platform-Specific Conditional Logic:**
    - Check for any platform-specific code (`#if WINDOWS`, `#if LINUX`, etc.) within `Night.Framework.Run()` or related SDL handling that might behave differently on CI.
6. **Minimal Test Case on CI:**
    - If possible, create an extremely simple `IGame` implementation that does nothing but try to stay alive for a set duration (e.g., 1 second) by having its `Update` method do nothing and `EndTest()` called only after that duration. Run this minimal test on CI to see if even that terminates early. This can help isolate whether the issue is with complex test logic or the basic framework loop persistence.
    - Example minimal test:

        ```csharp
        public class MinimalDurationTest : GameTestCase
        {
            public override string Name => "MinimalDurationTest";
            public override string Description => "Tests if framework can run for a minimal duration.";
            protected override void Update(double deltaTime)
            {
                // Try to pass after 1 second
                if (this.CheckCompletionAfterDuration(1000, () => true, () => "Minimal duration passed."))
                {
                    // Completion handled by CheckCompletionAfterDuration
                }
            }
        }
        ```

7. **SDL Error Checking:**
    - Ensure `SDL.GetError()` is checked after critical SDL operations within `Night.Framework.Run()` (e.g., `SDL.Init()`, window creation, renderer creation, event polling) and log any reported SDL errors. This might reveal underlying SDL issues specific to the CI environment.
8. **Evaluate and Refine CI Environment Configuration:**
    - While `Framework.cs` currently attempts to set `SDL_VIDEODRIVER=dummy` and `SDL_RENDER_DRIVER=software` for testing environments, the persistent failures suggest these might be insufficient or not behaving as expected across all CI platforms. Consider the following:
    - **Headless Display Server (Linux):**
        - Investigate using Xvfb (X virtual framebuffer) on Linux runners. This simulates an X11 display server in memory.
        - Example GitHub Actions setup:

          ```yaml
          jobs:
            test:
              runs-on: ubuntu-latest
              steps:
              - name: Set up Xvfb
                run: |
                  sudo apt-get update
                  sudo apt-get install -y xvfb
              - name: Run tests with Xvfb
                run: xvfb-run your-test-command
          ```

        - If using Xvfb, `SDL_VIDEODRIVER` might need to be set to `x11`.
    - **Alternative SDL Environment Variables:**
        - Experiment with `SDL_VIDEODRIVER=offscreen` as an alternative to `dummy`. This driver is designed for rendering without a visible window and might offer different stability.
        - Re-confirm that `SDL_RENDER_DRIVER=software` is consistently applied and effective in CI. While already set in `Framework.cs`, its interaction with different video drivers or CI environments should be verified.
    - **Containerization (Docker):**
        - Explore running tests inside a Docker container. This provides a consistent environment with pre-configured graphics libraries (e.g., Mesa for OpenGL software rendering) and Xvfb if needed.
        - Example: `docker run -e SDL_VIDEODRIVER=x11 your-docker-image xvfb-run your-test-command`

By systematically investigating these areas, the aim is to identify the exact point of failure or the reason for the premature termination of the `Night.Framework.Run()` loop on the CI platforms, and to establish a more robust CI testing environment.
