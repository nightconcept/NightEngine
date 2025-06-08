# Epic: Implement Flexible Logging Module for Night Library

**User Story:** As a Night Engine developer, I want a flexible and extensible logging module so that I can easily output diagnostic messages to various configurable destinations (file, in-game console, memory, system console) with control over log levels and sink activation, replacing the current `System.Console.WriteLine` usage.

**Status:** In-Progress

**Priority:** High

**Assignee:** Night Agent (AI)

**Reporter:** User

**Estimated Effort:** TBD

**Start Date:** YYYY-MM-DD
**Due Date:** YYYY-MM-DD

---

## Background

The current diagnostic output in the `Night` library relies on `System.Console.WriteLine`. This is inflexible for a game engine requiring configurable log outputs, varying log levels, and the ability to direct logs to different sinks like files or an in-game display. This epic aims to create a robust logging system internal to the `Night` namespace.

## Requirements

-   **Core Interfaces:** Define `ILogger`, `LogLevel` enum, `ILogSink`, and `LogEntry` record/class.
-   **Log Levels:** Support Trace, Debug, Information, Warning, Error, Fatal.
-   **`ILogger` Methods:** Include methods like `Debug(string message)`, `Error(string message, Exception exception)`, and `IsEnabled(LogLevel level)`.
-   **`LogEntry` Structure:** Must contain Timestamp (UTC), LogLevel, formatted Message, optional Exception, and CategoryName/Source string.
-   **`LogManager`:** Static class for obtaining `ILogger` instances (e.g., `LogManager.GetLogger("Renderer")`), managing active `ILogSink`s, global configuration (e.g., minimum log level), and enabling/disabling sinks.
-   **Sink Implementations:**
    -   `FileSink`: Writes to a configurable file path; opt-in.
    -   `InGameConsoleSink`: Buffers messages for in-game UI display.
    -   `MemorySink`: Stores recent log entries in memory.
    -   `SystemConsoleSink`: Writes to `System.Console`; *must be opt-in and not active by default*.
-   **Thread Safety:** Ensure logging operations are thread-safe.
-   **Error Handling:** Failures in one sink should not affect others or crash the application.
-   **Extensibility:** Design should allow adding new custom sinks.
-   **Namespace:** All logging components should be within the `Night` namespace and initially internal to the library.
-   **Configuration:** Provide clear mechanisms to enable/disable and configure sinks, especially `FileSink` (path) and `SystemConsoleSink`.
-   **Internal Documentation:** Add /// comments for all public types and methods within the logging module.

## Acceptance Criteria

1.  All `System.Console.WriteLine` calls within the `Night` library (specifically `Framework.cs` as a starting point) are replaced with the new logging system.
2.  Logging to `System.Console` is disabled by default and can be explicitly enabled via configuration.
3.  Logging to a file can be enabled and the output path configured.
4.  The `MemorySink` correctly captures recent log entries.
5.  The `InGameConsoleSink` provides a mechanism to retrieve buffered log messages.
6.  Log filtering based on global minimum log level works as expected.
7.  Categorized loggers (e.g., `LogManager.GetLogger("Framework")`) correctly identify the source in `LogEntry`.
8.  The system is thread-safe under typical engine usage patterns.
9.  Basic error handling for sink operations is implemented and observable (e.g., logs an internal error if a sink fails).
10. All new logging code is documented with /// comments.

---

## Tasks

### Phase 1: Core Logging Infrastructure

-   [x] **Task 1.1:** Define `LogLevel` enum (Trace, Debug, Information, Warning, Error, Fatal).
    -   *File(s):* `Night/Log/LogLevel.cs`
-   [x] **Task 1.2:** Define `LogEntry` record/class.
    -   *Details:* Include `DateTime TimestampUtc`, `LogLevel Level`, `string Message`, `Exception? Exception`, `string CategoryName`.
    -   *File(s):* `Night/Log/LogEntry.cs`
-   [x] **Task 1.3:** Define `ILogSink` interface.
    -   *Methods:* `void Write(LogEntry entry);`
    -   *File(s):* `Night/Log/ILogSink.cs`
-   [x] **Task 1.4:** Define `ILogger` interface.
    -   *Methods:* `void Log(LogLevel level, string message, Exception? exception = null);`, `bool IsEnabled(LogLevel level);`
    -   *Convenience Methods:* `Trace(string message)`, `Debug(string message)`, `Info(string message)`, `Warn(string message)`, `Error(string message, Exception? exception = null)`, `Fatal(string message, Exception? exception = null)`.
    -   *File(s):* `Night/Log/ILogger.cs`
-   [x] **Task 1.5:** Implement `Logger` class (implements `ILogger`).
    -   *Details:* Takes `LogManager` and `categoryName` in constructor. Forwards logs to `LogManager`.
    -   *File(s):* `Night/Log/Logger.cs`
-   [x] **Task 1.6:** Write manual tests/verification steps for Core Logging Infrastructure.

#### Manual Verification Steps for Phase 1

1.  **Compilation Check:** Ensure the project compiles successfully after creating all files in this phase (`Night/Log/LogLevel.cs`, `Night/Log/LogEntry.cs`, `Night/Log/ILogSink.cs`, `Night/Log/ILogger.cs`, `Night/Log/Logger.cs`).
2.  **API Review:**
    *   Open `LogLevel.cs` and verify all enum values (Trace, Debug, Information, Warning, Error, Fatal) are present.
    *   Open `LogEntry.cs` and verify all specified properties (`DateTime TimestampUtc`, `LogLevel Level`, `string Message`, `Exception? Exception`, `string CategoryName`) are present with correct types.
    *   Open `ILogSink.cs` and verify the `Write(LogEntry entry)` method signature.
    *   Open `ILogger.cs` and verify the `Log(LogLevel level, string message, Exception? exception = null)` and `IsEnabled(LogLevel level)` method signatures, as well as all convenience method signatures (`Trace`, `Debug`, `Info`, `Warn`, `Error`, `Fatal`).
    *   Open `Logger.cs` and verify it implements `ILogger` and has a constructor accepting `LogManager` (or a reference to its dispatching mechanism) and a `categoryName`.
3.  **(Conceptual) Instantiation Check:** Mentally (or with a temporary, isolated code snippet if feasible, though `LogManager` is not yet built), confirm that `Logger` could be instantiated if a `LogManager` instance/surrogate were available.

### Phase 2: LogManager and Basic Sinks

-   [x] **Task 2.1:** Implement `LogManager` static class.
    -   *Responsibilities:* Manage list of `ILogSink`s, provide `GetLogger(string categoryName)`, global minimum log level (`MinLevel`), methods `AddSink(ILogSink sink)`, `RemoveSink(ILogSink sink)`, `ClearSinks()`.
    -   *Details:* `GetLogger` should return an `ILogger` instance (e.g., `Logger` class from Task 1.5). Internal method to dispatch `LogEntry` to all active sinks if `entry.Level >= MinLevel`.
    -   *File(s):* `Night/Log/LogManager.cs`
-   [x] **Task 2.2:** Implement `SystemConsoleSink` (implements `ILogSink`).
    -   *Details:* Writes formatted `LogEntry` to `System.Console`. Ensure it can be added/removed via `LogManager`. *Crucially, this sink should not be added by default.*
    -   *File(s):* `Night/Log/Sinks/SystemConsoleSink.cs`
-   [x] **Task 2.3:** Implement `MemorySink` (implements `ILogSink`).
    -   *Details:* Stores a configurable number of recent `LogEntry` objects in a thread-safe collection (e.g., `ConcurrentQueue` with size limit). Provides a method to retrieve buffered entries.
    -   *File(s):* `Night/Log/Sinks/MemorySink.cs`
-   [ ] **Task 2.4:** Write manual tests/verification steps for LogManager and Basic Sinks.

#### Manual Verification Steps for Phase 2

*Prerequisites: A simple test harness or a temporary console application where you can call `LogManager` methods.*

1.  **`LogManager.GetLogger()`:**
    *   Call `LogManager.GetLogger("TestCategory1")`. Verify it returns a non-null `ILogger` instance.
    *   Call `LogManager.GetLogger("TestCategory1")` again. Verify it returns the same instance (or an equivalent one for the same category).
    *   Call `LogManager.GetLogger("TestCategory2")`. Verify it returns a different instance (or an equivalent one for a new category).
2.  **`SystemConsoleSink` (Opt-In Behavior):**
    *   Without adding any sinks, use an `ILogger` instance to log a message (e.g., `logger.Info("Test message")`). Verify *nothing* is printed to the console.
    *   Create an instance of `SystemConsoleSink`.
    *   Call `LogManager.AddSink(systemConsoleSinkInstance)`.
    *   Log a message (e.g., `logger.Info("Hello Console!")`). Verify the message appears on `System.Console`, formatted appropriately (e.g., showing timestamp, level, category, message).
    *   Call `LogManager.RemoveSink(systemConsoleSinkInstance)`.
    *   Log another message. Verify it no longer appears on the console.
3.  **`MemorySink`:**
    *   Create an instance of `MemorySink` (e.g., `var memorySink = new MemorySink(capacity: 5);`).
    *   Call `LogManager.AddSink(memorySink)`.
    *   Log several messages (e.g., 6 messages: "Msg1" to "Msg6").
    *   Call the method on `memorySink` to retrieve buffered entries (e.g., `memorySink.GetEntries()`).
    *   Verify the retrieved collection contains the last 5 messages ("Msg2" to "Msg6").
    *   Verify each `LogEntry` in the buffer has correct Timestamp, Level, Message, and CategoryName.
4.  **Global Minimum Log Level (`LogManager.MinLevel`):**
    *   Ensure `SystemConsoleSink` is added.
    *   Set `LogManager.MinLevel = LogLevel.Warning;`.
    *   Log messages: `logger.Debug("Debug A")`, `logger.Info("Info A")`, `logger.Warn("Warning A")`.
    *   Verify only "Warning A" (and any higher level messages) appear on the console.
    *   Set `LogManager.MinLevel = LogLevel.Trace;` (or the lowest level).
    *   Log `logger.Debug("Debug B")`. Verify "Debug B" now appears.
5.  **`ClearSinks()`:**
    *   Add `SystemConsoleSink` and `MemorySink`. Log a message and verify output/capture.
    *   Call `LogManager.ClearSinks()`.
    *   Log another message. Verify no output to console and no new entries in the `MemorySink` (its existing entries might still be there until new ones push them out or it's cleared/recreated).

### Phase 3: Advanced Sinks

-   [x] **Task 3.1:** Implement `FileSink` (implements `ILogSink`).
    -   *Details:* Writes formatted `LogEntry` to a specified file. Path should be configurable. Handles file I/O safely. Ensure it can be added/removed.
    -   *File(s):* `Night/Log/Sinks/FileSink.cs`
-   [x] **Task 3.2:** Implement `InGameConsoleSink` (implements `ILogSink`).
    -   *Details:* Buffers `LogEntry` objects in a thread-safe collection. Provides a way for a UI to access these messages (e.g., a public method to retrieve the buffer or an event).
    -   *File(s):* `Night/Log/Sinks/InGameConsoleSink.cs`
-   [x] **Task 3.3:** Write manual tests/verification steps for Advanced Sinks.

#### Manual Verification Steps for Phase 3

*Prerequisites: Test harness from Phase 2. Ensure `SystemConsoleSink` can be optionally active for comparison if needed.*

1.  **`FileSink`:**
    *   Create an instance of `FileSink`, providing a valid, writable file path (e.g., `"./test_log.txt"`).
    *   Call `LogManager.AddSink(fileSinkInstance)`.
    *   Log several messages with different levels and categories (e.g., `LogManager.GetLogger("FileTest").Info("Log to file");`, `LogManager.GetLogger("FileTest").Error("File error!", new System.Exception("dummy exception"));`).
    *   Open `test_log.txt`. Verify:
        *   The file was created.
        *   All logged messages are present.
        *   Each log entry is formatted correctly (timestamp, level, category, message, exception details if any).
    *   Log more messages. Verify they are appended to the file.
    *   (Optional) Test with an invalid/unwritable path if error handling for sink construction/write is part of this phase's scope. Observe behavior (e.g., internal error logged if a fallback exists, no crash).
    *   Call `LogManager.RemoveSink(fileSinkInstance)`. Log a message. Verify it's not written to `test_log.txt`.
2.  **`InGameConsoleSink`:**
    *   Create an instance of `InGameConsoleSink` (e.g., `var gameConsoleSink = new InGameConsoleSink(capacity: 10);`).
    *   Call `LogManager.AddSink(gameConsoleSink)`.
    *   Log several messages (e.g., 12 messages).
    *   Call the method on `gameConsoleSink` designed to retrieve/expose its buffered `LogEntry` objects.
    *   Verify the collection contains the last 10 messages.
    *   Verify the format and content of these `LogEntry` objects (Timestamp, Level, Message, Category).
    *   If an event-based notification is implemented for new messages, try to (conceptually or with a simple handler) verify it fires.

### Phase 4: Thread Safety, Error Handling, and Configuration

-   [x] **Task 4.1:** Ensure thread safety for `LogManager` and all sink operations.
    -   *Details:* Review and use appropriate synchronization primitives (`lock`, `ConcurrentDictionary`, `ConcurrentQueue`, etc.) where necessary.
-   [x] **Task 4.2:** Implement error handling within sinks.
    -   *Details:* A failure in one sink (e.g., file I/O error in `FileSink`) should not stop other sinks or crash the application. Consider logging such errors to `System.Diagnostics.Trace` or a fallback mechanism.
-   [ ] **Task 4.3:** Design and implement sink configuration mechanism.
    -   *Details:* Determine how sinks like `FileSink` (path) and `SystemConsoleSink` (enable/disable) will be configured. This could be through methods on `LogManager` or by reading from `ConfigurationManager` if appropriate for internal use.
    -   *Example:* `LogManager.EnableSystemConsoleSink()`, `LogManager.ConfigureFileSink(string path, LogLevel minLevelForFile)`.
-   [x] **Task 4.4:** Write manual tests/verification steps for Thread Safety, Error Handling, and Configuration.

#### Manual Verification Steps for Phase 4

*Prerequisites: A simple test harness or a temporary console application where you can call `LogManager` methods and simulate multi-threading.*

1.  **Thread Safety (`LogManager` and Sinks):**
    *   Initialize `LogManager`.
    *   Call `LogManager.EnableSystemConsoleSink(true)`.
    *   Call `LogManager.ConfigureFileSink("thread_test_log.txt", LogLevel.Trace)`.
    *   Create an instance of `MemorySink` (e.g., `var memorySink = new MemorySink(capacity: 2000);`) and add it using `LogManager.AddSink(memorySink)`.
    *   Spawn several threads (e.g., 5-10 threads).
    *   In each thread:
        *   Get a logger instance: `var logger = LogManager.GetLogger($"Thread-{Thread.CurrentThread.ManagedThreadId}");`
        *   Log a significant number of messages (e.g., 100-200 messages) in a loop with varying log levels: `logger.Info($"Message {i} from thread.");`, `logger.Debug($"Debug message {i}");`
    *   Wait for all threads to complete.
    *   *Observe & Verify:*
        *   **Application Stability:** The application completes without deadlocks or exceptions related to concurrent access.
        *   **Console Output (`SystemConsoleSink`):**
            *   Messages from different threads should appear. Some interleaving of *complete log messages* is expected and acceptable.
            *   Individual log messages should not be garbled (e.g., parts of one message mixed with parts of another within the same line).
        *   **File Output (`FileSink` - `thread_test_log.txt`):**
            *   Open `thread_test_log.txt`.
            *   Verify that the file is not corrupted.
            *   Verify that messages from all threads are present and correctly formatted.
            *   The total number of messages should roughly correspond to (num_threads * num_messages_per_thread) that meet the `LogLevel.Trace` criteria.
        *   **`MemorySink` Output:**
            *   Retrieve entries: `var entries = memorySink.GetEntries();`
            *   Verify that the number of entries is as expected (up to its capacity).
            *   Check a sample of entries for consistency (correct timestamp, level, category, message).
    *   Clean up: `LogManager.DisableFileSink(); LogManager.ClearSinks(); File.Delete("thread_test_log.txt");`

2.  **Sink Error Handling (Focus on `FileSink`):**
    *   Initialize `LogManager`.
    *   Call `LogManager.EnableSystemConsoleSink(true)`.
    *   Configure `FileSink` to a valid path: `LogManager.ConfigureFileSink("error_test_log.txt", LogLevel.Info);`
    *   Log a message: `LogManager.GetLogger("ErrorTest").Info("Initial message - should go to console and file.");`
    *   Verify the message appears on the console and in `error_test_log.txt`.
    *   **Simulate `FileSink` Write Error:**
        *   Make `error_test_log.txt` read-only (e.g., using file system permissions or `File.SetAttributes("error_test_log.txt", FileAttributes.ReadOnly)`).
        *   Alternatively, if testing `FileSink`'s initialization error: try `LogManager.ConfigureFileSink("Z:\\non_existent_drive\\error_init_test.txt");` (assuming Z: is not a valid writable drive).
    *   Log another message: `LogManager.GetLogger("ErrorTest").Warn("Second message - should go to console, FileSink might fail.");`
    *   *Verify:*
        *   The second message *still appears* in the `SystemConsoleSink` output.
        *   The application *does not crash*.
        *   Check `System.Diagnostics.Trace` output (e.g., in IDE's debug output window or a configured trace listener). An error message indicating the `FileSink` failure (e.g., "Access to the path... is denied" or "Could not find a part of the path...") should be present.
    *   **Restore `FileSink` Operation:**
        *   If read-only was set: `File.SetAttributes("error_test_log.txt", FileAttributes.Normal);`
        *   If a bad path was used for init, reconfigure to a valid one: `LogManager.ConfigureFileSink("error_test_log_restored.txt", LogLevel.Info);`
    *   Log a third message: `LogManager.GetLogger("ErrorTest").Info("Third message - should go to console and new/restored file.");`
    *   Verify the third message appears on the console and in the (newly configured or now writable) file log.
    *   Clean up: `LogManager.DisableFileSink(); LogManager.EnableSystemConsoleSink(false); File.Delete("error_test_log.txt"); File.Delete("error_test_log_restored.txt");` (handle potential delete errors if files didn't exist).

3.  **Sink Configuration Mechanism:**
    *   **`SystemConsoleSink` Enable/Disable:**
        *   `LogManager.EnableSystemConsoleSink(true);`
        *   `LogManager.GetLogger("ConsoleTest").Info("Message 1: Console On");` -> Verify console output.
        *   `LogManager.IsSystemConsoleSinkEnabled()` -> Verify returns `true`.
        *   `LogManager.EnableSystemConsoleSink(false);`
        *   `LogManager.GetLogger("ConsoleTest").Info("Message 2: Console Off");` -> Verify *no* console output for this message.
        *   `LogManager.IsSystemConsoleSinkEnabled()` -> Verify returns `false`.
        *   `LogManager.EnableSystemConsoleSink(true);`
        *   `LogManager.GetLogger("ConsoleTest").Info("Message 3: Console On Again");` -> Verify console output.
    *   **`FileSink` Configuration (Path, Level) & Disable:**
        *   `LogManager.ConfigureFileSink("file_config_A.log", LogLevel.Information);`
        *   `var logger = LogManager.GetLogger("FileConfigTest");`
        *   `logger.Debug("File A - Debug (should not appear)");`
        *   `logger.Info("File A - Info (should appear)");`
        *   `logger.Warn("File A - Warn (should appear)");`
        *   Verify `file_config_A.log` contains only Info and Warn messages.
        *   `LogManager.ConfigureFileSink("file_config_B.log", LogLevel.Debug);` (This implicitly disables/replaces the sink for `file_config_A.log`)
        *   `logger.Debug("File B - Debug (should appear)");`
        *   `logger.Info("File B - Info (should appear)");`
        *   Verify `file_config_B.log` contains Debug and Info messages. Verify `file_config_A.log` is no longer being written to.
        *   `LogManager.DisableFileSink();`
        *   `logger.Error("File B - Error (should not appear in file after disable)");`
        *   Verify `file_config_B.log` does not contain the error message.
        *   Clean up: `File.Delete("file_config_A.log"); File.Delete("file_config_B.log");`
    *   **Global `LogManager.MinLevel` vs. `FileSink` Specific Level:**
        *   `LogManager.EnableSystemConsoleSink(true);` // For easy observation
        *   `LogManager.MinLevel = LogLevel.Warning;`
        *   `LogManager.ConfigureFileSink("file_level_test.log", LogLevel.Debug);` // FileSink wants Debug and up
        *   `var levelLogger = LogManager.GetLogger("LevelTest");`
        *   `levelLogger.Debug("Global Warn, File Debug - Debug Msg (File: No, Global: No)");`
        *   `levelLogger.Info("Global Warn, File Debug - Info Msg (File: No, Global: No)");`
        *   `levelLogger.Warn("Global Warn, File Debug - Warn Msg (File: Yes, Global: Yes)");`
        *   `levelLogger.Error("Global Warn, File Debug - Error Msg (File: Yes, Global: Yes)");`
        *   Verify `file_level_test.log` contains only Warn and Error messages (because global `MinLevel` is `Warning`).
        *   Verify console also only shows Warn and Error messages.
        *   `LogManager.MinLevel = LogLevel.Trace;`
        *   `LogManager.ConfigureFileSink("file_level_test2.log", LogLevel.Information);` // FileSink wants Info and up
        *   `levelLogger.Debug("Global Trace, File Info - Debug Msg (File: No, Global: Yes)");`
        *   `levelLogger.Info("Global Trace, File Info - Info Msg (File: Yes, Global: Yes)");`
        *   `levelLogger.Warn("Global Trace, File Info - Warn Msg (File: Yes, Global: Yes)");`
        *   Verify `file_level_test2.log` contains Info and Warn messages (FileSink filters Debug, global allows all).
        *   Verify console shows Debug, Info, and Warn messages.
        *   Clean up: `LogManager.DisableFileSink(); LogManager.EnableSystemConsoleSink(false); File.Delete("file_level_test.log"); File.Delete("file_level_test2.log");`

### Phase 5: Integration and Refactoring

-   [In-Progress] **Task 5.1:** Integrate `LogManager` into `Night.Framework`.
    -   *Details:* Initialize `LogManager` (e.g., set default `MinLevel`). Potentially expose a way for the game to add its own sinks if ever needed in the future (though initially internal).
-   [In-Progress] **Task 5.2:** Refactor `Framework.cs` to use the new logging system.
    -   *Details:* Replace all `Console.WriteLine` calls with `LogManager.GetLogger("Framework").Info(...)` or other appropriate levels/methods.
    -   *Note:* Exclude "Night Engine: v...", "SDL: v...", "Platform: ...", "Framework: ..." from conversion; these should remain `System.Console.WriteLine`.
-   [ ] **Task 5.3:** (TODO, DO LATER) Identify and refactor other `Console.WriteLine` calls in the `Night` library.
-   [In-Progress] **Task 5.4:** Write manual tests/verification steps for Integration and Refactoring.

#### Manual Verification Steps for Phase 5

*Prerequisites: The `Night` engine/framework should be runnable, at least to the point where `Framework.cs` executes its initialization and main loop logic.*

1.  **Configuration for Test:**
    *   Ensure your `SampleGame` (or test harness) configures the `LogManager` appropriately. For thorough testing of `Framework.cs` logs:
        *   Enable `SystemConsoleSink` (e.g., `LogManager.EnableSystemConsoleSink(true);`).
        *   Set global minimum log level to `LogLevel.Trace` or `LogLevel.Debug` (e.g., `LogManager.MinLevel = LogLevel.Trace;`). This will ensure all messages from `Framework.cs` are captured.
        *   (Optional) Enable `FileSink` to a known file path for persistent log checking (e.g., `LogManager.ConfigureFileSink("framework_test.log", LogLevel.Trace);`).
2.  **Run the Application:** Execute `SampleGame` or a simple test case that uses `Night.Framework.Run()`.
3.  **Observe Console Output - Excluded Lines:**
    *   Verify that the following lines are printed directly to the console, **without** logger formatting (i.e., no timestamp, level, or category prefix):
        *   `Night Engine: v...` (with the correct version)
        *   `SDL: v...` (with the correct version)
        *   `Platform: ...` (with correct platform details)
        *   `Framework: ...` (with correct framework description)
4.  **Observe Logs from `Framework.cs` (Logger Formatted):**
    *   Check the console output (and file log, if configured) for messages originating from `Framework.cs`.
    *   Verify that all messages *other than the excluded lines above* are now formatted by the logging system. This means they should include:
        *   Timestamp (UTC)
        *   Log Level (e.g., INF, DBG, ERR, FTL)
        *   Category Name (should be "Framework")
        *   The log message itself.
    *   **Examples of messages to look for (now logger-formatted):**
        *   "Testing environment detected. Setting SDL video driver to 'dummy'." (if applicable)
        *   "Global isSdlInitialized is false. Attempting SDL.Init()."
        *   "SDL_Init failed: [SDL Error Message]" (if an error is forced/occurs)
        *   "SDL.Init() successful."
        *   "IsInputInitialized set to [true/false]."
        *   "Calling Window.SetMode with Width=[w], Height=[h], Flags=[f]"
        *   "Window.SetMode returned [true/false]."
        *   "Window title set to '[Actual Title]'."
        *   "Proceeding to game.Load()..."
        *   "game.Load() completed."
        *   "Starting main loop."
        *   "Main loop ended."
        *   Messages from `HandleGameException` and `DefaultErrorHandler` (e.g., "HandleGameException: Error: [Message]", "--- Night Engine: Default Error Handler ---").
5.  **Verify Log Levels and Categories:**
    *   Ensure the logged messages from `Framework.cs` (excluding the direct `Console.WriteLine` calls) use appropriate log levels:
        *   General operational information: `Info`
        *   Detailed diagnostic information: `Debug`
        *   Recoverable errors/warnings: `Warn` or `Error`
        *   Critical/unrecoverable errors: `Error` or `Fatal`
    *   Confirm the `CategoryName` for these logs is consistently "Framework".
6.  **Test Specific Scenarios (Error Handling):**
    *   If feasible, try to induce an error that would be caught by `HandleGameException` or `DefaultErrorHandler` in `Framework.cs`.
        *   Example: Throw an exception within `game.Load()` or `game.Update()` in your test game.
    *   Verify that the error messages are logged by the "Framework" logger with an appropriate error level (e.g., `Error`, `Fatal`) and include exception details.
    *   Verify that the `DefaultErrorHandler`'s own diagnostic messages (e.g., "Window or Graphics not initialized", "Failed to set window mode") are also logged via the "Framework" logger.
7.  **(If Task 5.3 was done) Verify Other Refactored Areas:** (This step remains for future completion of 5.3) If other parts of the `Night` library were refactored, run tests or application parts that exercise those areas and check for expected log outputs.
8.  **Cleanup (if `FileSink` was used):** Delete any log files created during the test (e.g., `framework_test.log`).

### Phase 6: Documentation and Review

-   [ ] **Task 6.1:** Add /// XML documentation comments to all new public types and methods in the logging module.
-   [ ] **Task 6.2:** Perform a self-review of the implemented logging system against all requirements and acceptance criteria.
-   [ ] **Task 6.3:** Prepare the changes for user review.
-   [ ] **Task 6.4:** Write manual tests/verification steps for Documentation and Review.

#### Manual Verification Steps for Phase 6

1.  **XML Documentation Review (IDE / Generated File):**
    *   In your C# IDE (e.g., Visual Studio, Rider), hover over class names, method names, properties, and enum members within the `Night.Log` namespace (e.g., `LogManager`, `ILogger.Info`, `LogEntry.TimestampUtc`, `LogLevel.Debug`, `FileSink`).
    *   Verify that descriptive /// XML comments appear as tooltips, explaining their purpose, parameters, and return values where applicable.
    *   If your build process generates an XML documentation file, locate it and open it. Browse through the documentation for the logging module components and check for completeness and correctness.
2.  **Code Review (Self-Review):**
    *   Re-read the "Requirements" and "Acceptance Criteria" sections of this epic document (`logger-tasks.md`).
    *   Go through each implemented file in the `Night.Log` namespace (`LogLevel.cs`, `LogEntry.cs`, `ILogSink.cs`, `ILogger.cs`, `Logger.cs`, `LogManager.cs`, and all sink implementations).
    *   For each requirement and acceptance criterion, confirm that the implemented code meets it. For example:
        *   Is `SystemConsoleSink` truly opt-in and not active by default?
        *   Can `FileSink` path be configured?
        *   Is `LogManager.MinLevel` respected?
        *   Is `LogEntry.CategoryName` populated correctly?
3.  **Final Functionality Check (Spot Check):**
    *   Perform a small subset of the manual verification steps from Phases 2-5 to ensure key functionalities are still working as expected after any final tweaks or documentation-related code changes.
    *   For example:
        *   Enable `SystemConsoleSink`, log a message, verify output.
        *   Enable `FileSink`, log a message, verify file output.
        *   Test `LogManager.MinLevel` filtering briefly.
        *   Run a simple `Night.Framework` application and check if `Framework.cs` logs are appearing.
4.  **Epic Document Review:**
    *   Read through the "Notes & Decisions" section. Ensure all documented decisions were followed or updated.
    *   Read through the "Questions for User" section. Ensure any questions raised during development were addressed or are noted as pending.
    *   Update the "Status" of tasks and the main epic status (e.g., to "Ready for Review" or similar).
    *   Fill in "Actual Effort" or "Completion Date" if applicable.

---

## Notes & Decisions

*(This section will be updated as development progresses)*

-   Initial decision: Logging module will reside entirely within the `Night` namespace and its components marked as `internal` where appropriate if they are not meant to be part of the engine's public API yet.
-   Configuration for opt-in sinks (`SystemConsoleSink`, `FileSink`) needs careful design to be user-friendly for developers using the Night engine, even if initially for internal Night library use.
-   2025-06-03: Identified missing sink configuration methods in `LogManager` (`EnableSystemConsoleSink`, `IsSystemConsoleSinkEnabled`, `ConfigureFileSink`, `DisableFileSink`) despite Task 4.3 being marked complete. These methods are required by `SampleGame` and manual tests. Planning to implement them.
