# Epic 10: Achieving Roadmap Version 0.1.0

**Goal:** Implement the remaining core features and functionalities outlined for Version 0.1.0 in `project/love2d-api/roadmap.md`. This epic focuses on API completion for the initial public feature set of Night.Framework.

**User Stories:**

- As a game developer using Night Engine, I want a `love.filesystem`-like API to manage game assets and data.
- As a game developer, I want to draw basic 2D shapes (rectangles, circles, lines) using `Night.Graphics`.
- As a game developer, I need to query timing information like FPS and total elapsed time via a `love.timer`-like API.
- As a game developer, I want more complete keyboard and mouse event callbacks (`KeyReleased`, `MousePressed`, `MouseReleased`).
- As a game developer, I need to manage window properties like dimensions, desktop info, and fullscreen modes via a `love.window`-like API.
- As a project maintainer, I want automated documentation generation and a basic CI setup.
- As a game developer, I want a way to handle errors originating from my game code gracefully via a `love.errorhandler`-like mechanism.
- As a game developer, I want to be able to configure basic game settings through a configuration file.

**Version 0.1.0 Roadmap Items to Address:**
(Reference: `project/love2d-api/roadmap.md`)

## Tasks

### Phase 1: Core Framework Enhancements

- [x] **Task 10.1: Implement `Night.Filesystem` (Basic)**
  - **Description:** Create the `Night.Filesystem` static class. Implement core functions needed for 0.1.0, focusing on reading files (e.g., for `Graphics.NewImage`), checking file/directory existence. Refer to `project/love2d-api/modules/filesystem.md` for API inspiration, but scope to essential read operations.
  - **Implementation:**
    - [x] Create `Night.Filesystem` static class
    - [-] ~~Implement `Exists(string path)` - Check if a file or directory exists~~ (Replaced by `GetInfo`)
    - [-] ~~Implement `IsFile(string path)` - Check if path is a file~~ (Replaced by `GetInfo`)
    - [-] ~~Implement `IsDirectory(string path)` - Check if path is a directory~~ (Replaced by `GetInfo`)
    - [x] Implement `ReadBytes(string path)` - Read file as byte array
    - [x] Implement `ReadText(string path)` - Read file as text
    - [x] **Task 10.1.1: Refactor File/Directory Checks to `GetInfo`**
      - **Description:** Replace `Exists`, `IsFile`, and `IsDirectory` with a new `GetInfo(string path, FileType? filterType = null, FileSystemInfo? existingInfo = null)` function, based on `love.filesystem.getInfo`. This new function will provide comprehensive file/directory attributes.
      - **Implementation:**
        - [x] Define `Night.FileType` enum (`File`, `Directory`, `Symlink`, `Other`, `None`).
        - [x] Define `Night.FileSystemInfo` class (with `Type`, `Size`, `ModTime`).
        - [x] Remove `Night.Filesystem.Exists(string path)`.
        - [x] Remove `Night.Filesystem.IsFile(string path)`.
        - [x] Remove `Night.Filesystem.IsDirectory(string path)`.
        - [x] Implement `Night.Filesystem.GetInfo(...)` and its overloads.
        - [x] Update `Night.SampleGame` to use `GetInfo` instead of the removed methods.
      - **Acceptance Criteria:** `Night.Filesystem.GetInfo` correctly returns information for files and directories. `Night.SampleGame` is updated and functions correctly with the new API. The old methods are removed.
      - **Status:** Done
  - **Acceptance Criteria:** Basic file operations are available and usable by other modules (e.g., `Night.Graphics.NewImage` can use it). Sample game can demonstrate reading a simple text file.
  - **Status:** In-Progress

- [x] **Task 10.2: Extend `Night.Graphics` with Basic Shape Drawing**
  - **Description:** Add methods to `Night.Graphics` for drawing 2D primitives.
  - **Implementation:**
    - [x] Define `Night.DrawMode` enum with values:
      - `Fill` - Filled shapes
      - `Line` - Outlined shapes
    - [x] Implement `Rectangle(DrawMode mode, float x, float y, float width, float height)`
    - [x] Implement `Circle(DrawMode mode, float x, float y, float radius, int segments = 12)` (Note: `Color color` param removed to rely on `SetColor`)
    - [x] Implement `Line(float x1, float y1, float x2, float y2)`
    - [x] Implement `Line(DrawMode mode, PointF[] points)` (Note: `List<PointF>` changed to `PointF[]` for consistency)
    - [x] Implement `Polygon(DrawMode mode, PointF[] vertices)`
    - [x] Implement `SetColor(Night.Color color)`
    - [x] Implement `SetColor(byte r, byte g, byte b, byte a = 255)`
    - [x] Update `Night.SampleGame` to demonstrate drawing these shapes
  - **Acceptance Criteria:** Rectangles, circles, and lines can be drawn with specified colors and modes. Sample game showcases this.
  - **Status:** Review

- [x] **Task 10.3: Implement `Night.Timer` Module**
  - **Description:** Create the `Night.Timer` static class.
  - [x] Implement `GetFPS()` and `GetTime()` (time since game start in seconds). Refer to `project/love2d-api/modules/timer.md`.
  - [x] `GetDeltaTime()` is already available implicitly via `IGame.Update()`, but should be renamed to `GetDelta()`. (Implemented as `Night.Timer.GetDelta()`)
  - [x] Implement `GetAverageDelta()`.
  - [x] Implement `Sleep()` to pause the current thread for the specified amount of time. This function causes the entire thread to pause for the duration of the sleep. Graphics will not draw, input events will not trigger, code will not run, and the window will be unresponsive if you use this as "wait()" in the main thread.
  - [x] Implement `Step()` to measures the time between two frames.
  - **Acceptance Criteria:** `Night.Timer.GetFPS()` returns current FPS. `Night.Timer.GetTime()` returns elapsed game time. Sample game can display these values.
  - **Status:** Done

- [x] **Task 10.4: Implement Remaining Input Event Callbacks**
  - **Description:** Add remaining input event callbacks to handle keyboard and mouse interactions.
  - **Implementation:**
    - [x] Add to `IGame` interface:
      - [x] `KeyReleased(KeySymbol key, KeyCode scancode)`
      - [x] `MousePressed(int x, int y, MouseButton button, bool istouch, int presses)`. Stub istouch for now as a TODO.
      - [x] `MouseReleased(int x, int y, MouseButton button,bool istouch, int presses)`. Stub istouch for now as a TODO.`
    - [x] Update `FrameworkLoop.cs` to handle and dispatch:
      - [x] `SDL.EventType.KeyUp` events
      - [x] `SDL.EventType.MouseButtonDown` events
      - [x] `SDL.EventType.MouseButtonUp` events
  - **Acceptance Criteria:** Sample game can react to key releases, mouse button presses, and mouse button releases, triggering the appropriate `IGame` callbacks.
  - **Status:** Done

- [x] **Task 10.5: Extend `Night.Window` Functionality**
  - **Description:** Implement window management functionality based on `project/love2d-api/modules/window.md` "In Scope" items.
  - **Implementation:**
    - [x] Core Methods:
      - [x] `GetDesktopDimensions(int displayIndex = 0)` - Get desktop dimensions [cite: 1280]
      - [x] `GetDisplayCount()` - Get number of displays [cite: 1283]
    - [x] Fullscreen Management:
      - [x] `GetFullscreen()` - Check if window is fullscreen. Returns bool fullscreen and FullscreenType fstype. FullscreenType is enumeration `desktop` and `exclusive`. `desktop` is sometimes known as borderless fullscreen windowed mode. A borderless screen-sized window is created which sits on top of all desktop UI elements. The window is automatically resized to match the dimensions of the desktop, and its size cannot be changed. `exclusive` is standard exclusive-fullscreen mode. Changes the display mode (actual resolution) of the monitor.
      - [x] `SetFullscreen(bool fullscreen, FullscreenType type = Desktop)` - Toggle fullscreen. Returns bool success.
      - [x] `GetFullscreenModes(int displayIndex = 0)` - Get available fullscreen modes. Returns table modes. A table of width/height pairs. (Note that this may not be in order.)
      - [x] Define `Night.FullscreenType` struct/class
    - [x] Window State:
      - [x] `GetMode()` - Get current window mode (width, height, flags) [cite: 1295]
    - [x] (Optional Stretch) High DPI Support:
      - [x] `FromPixels` - Converts a number from pixels to density-independent units.
      - [x] `ToPixels` - Converts a number from density-independent units to pixels.
      - [x] `GetDPIScale` - Gets the DPI scale factor associated with the window.
  - **Acceptance Criteria:** Window dimension and mode queries work. Fullscreen can be toggled. Sample game can demonstrate some of these (e.g., printing dimensions).
  - **Status:** Review

### Phase 2: Project Infrastructure & Polish

- [x] **Task 10.6: Implement User-Definable Error Handler**
  - **Description:** Design and implement a mechanism similar to `love.errorhandler`. Allow the user to register a custom error handling function/delegate that `FrameworkLoop.cs` will call when an unhandled exception occurs in `IGame.Load`, `IGame.Update`, `IGame.Draw`, or input callbacks.
  - The handler should receive error details (exception object, message, stack trace).
  - If no custom handler is set, implement the following equivalent from Love2D as the default error handling.
  - **Notes:**
    - The default error handler logs to console, ensures the window is open (or attempts to reopen to 800x600), clears it to a blue color, and resets mouse state (visible, not grabbed, not relative).
    - It then enters a loop allowing the user to quit (Esc key or closing the window) or copy the full error message and stack trace to the clipboard (Ctrl+C).
    - Clipboard functionality uses `Night.System.SetClipboardText(string)`.
    - Due to `Night.Font` not being part of the 0.1.0 scope, the default error handler does *not* render the error text directly into the game window. Users must check the console for the detailed error message.
    - Implemented `Night.Error.SetHandler(ErrorHandlerDelegate)` for users to provide their custom delegate.
    - Added `Night.Mouse.SetVisible(bool)`, `Night.Mouse.SetGrabbed(bool)`, and `Night.Mouse.SetRelativeMode(bool)`.

```lua
local utf8 = require("utf8")

local function error_printer(msg, layer)
 print((debug.traceback("Error: " .. tostring(msg), 1+(layer or 1)):gsub("\n[^\n]+$", "")))
end

function love.errorhandler(msg)
 msg = tostring(msg)

 error_printer(msg, 2)

 if not love.window or not love.graphics or not love.event then
  return
 end

 if not love.graphics.isCreated() or not love.window.isOpen() then
  local success, status = pcall(love.window.setMode, 800, 600)
  if not success or not status then
   return
  end
 end

 -- Reset state.
 if love.mouse then
  love.mouse.setVisible(true)
  love.mouse.setGrabbed(false)
  love.mouse.setRelativeMode(false)
  if love.mouse.isCursorSupported() then
   love.mouse.setCursor()
  end
 end
 if love.joystick then
  -- Stop all joystick vibrations.
  for i,v in ipairs(love.joystick.getJoysticks()) do
   v:setVibration()
  end
 end
 if love.audio then love.audio.stop() end

 love.graphics.reset()
 local font = love.graphics.setNewFont(14)

 love.graphics.setColor(1, 1, 1)

 local trace = debug.traceback()

 love.graphics.origin()

 local sanitizedmsg = {}
 for char in msg:gmatch(utf8.charpattern) do
  table.insert(sanitizedmsg, char)
 end
 sanitizedmsg = table.concat(sanitizedmsg)

 local err = {}

 table.insert(err, "Error\n")
 table.insert(err, sanitizedmsg)

 if #sanitizedmsg ~= #msg then
  table.insert(err, "Invalid UTF-8 string in error message.")
 end

 table.insert(err, "\n")

 for l in trace:gmatch("(.-)\n") do
  if not l:match("boot.lua") then
   l = l:gsub("stack traceback:", "Traceback\n")
   table.insert(err, l)
  end
 end

 local p = table.concat(err, "\n")

 p = p:gsub("\t", "")
 p = p:gsub("%[string \"(.-)\"%]", "%1")

 local function draw()
  if not love.graphics.isActive() then return end
  local pos = 70
  love.graphics.clear(89/255, 157/255, 220/255)
  love.graphics.printf(p, pos, pos, love.graphics.getWidth() - pos)
  love.graphics.present()
 end

 local fullErrorText = p
 local function copyToClipboard()
  if not love.system then return end
  love.system.setClipboardText(fullErrorText)
  p = p .. "\nCopied to clipboard!"
 end

 if love.system then
  p = p .. "\n\nPress Ctrl+C or tap to copy this error"
 end

 return function()
  love.event.pump()

  for e, a, b, c in love.event.poll() do
   if e == "quit" then
    return 1
   elseif e == "keypressed" and a == "escape" then
    return 1
   elseif e == "keypressed" and a == "c" and love.keyboard.isDown("lctrl", "rctrl") then
    copyToClipboard()
   elseif e == "touchpressed" then
    local name = love.window.getTitle()
    if #name == 0 or name == "Untitled" then name = "Game" end
    local buttons = {"OK", "Cancel"}
    if love.system then
     buttons[3] = "Copy to clipboard"
    end
    local pressed = love.window.showMessageBox("Quit "..name.."?", "", buttons)
    if pressed == 1 then
     return 1
    elseif pressed == 3 then
     copyToClipboard()
    end
   end
  end

  draw()

  if love.timer then
   love.timer.sleep(0.1)
  end
 end

end
```

- **Acceptance Criteria:** A user can provide a custom function to `Night.Framework` that gets called on unhandled game code exceptions, allowing custom display or logging.
- **Status:** Done

- [x] **Task 10.7: Basic Game Configuration File Support**
  - **Description:** Implement functionality to load basic game settings from a configuration file (e.g., `config.json`) at startup.
  - Focus on settings like default window width, height, title, vsync toggle.
  - `Night.Framework.Run` or `IGame.Load` could access these.
  - **Implementation:**
    - [x] Create `Night.Configuration.GameConfig` class with nested `WindowConfig`, `AudioConfig`, `ModulesConfig` to define configuration structure and defaults. (`src/Night.Engine/Framework/Configuration/GameConfig.cs`)
    - [x] Create `Night.Configuration.ConfigurationManager` static class to load `config.json` and provide access to `GameConfig`. (`src/Night.Engine/Framework/Configuration/ConfigurationManager.cs`)
    - [x] Modify `Night.FrameworkLoop.Run()` to call `ConfigurationManager.LoadConfig()` before `game.Load()`.
    - [x] Modify `Night.FrameworkLoop.Run()` to initialize the window using `ConfigurationManager.CurrentConfig.Window` settings if `game.Load()` does not create a window. This includes:
      - Window dimensions (width, height)
      - Window title
      - Window flags (Resizable, Borderless, HighDPI)
      - Fullscreen mode (Fullscreen, FullscreenType)
      - VSync
      - Initial window position (X, Y)
    - [x] Update `Night.SampleGame` to demonstrate overriding initial window settings via `config.json`.
    - [ ] TODO: Add handling for `t.window.icon` (requires `Night.Window.SetIcon` to be implemented first, which is out of scope for 0.1.0 according to Task 10.10 notes, but config option should exist).
    - [ ] TODO: Add console message for `t.console = true` on Windows (actual console attachment is a larger task).
    - [ ] TODO: Consider `t.identity` and `t.appendidentity` for `Night.Filesystem` initialization.
    - [ ] TODO: Implement logic for `t.modules.*` flags to actually enable/disable modules (currently placeholder flags).
  - **Acceptance Criteria:** The engine loads `config.json`. If `game.Load()` doesn't open a window, the engine uses `config.json` values (or defaults) for window width, height, title, resizable, borderless, fullscreen, fullscreen type, VSync, and initial position. The sample game can have its initial window settings overridden by a `config.json` file (once sample game is updated).
  - **Status:** In-Progress

- [x] **Task 10.8: Setup `docfx` Documentation Generation**
  - **Description:** Integrate `docfx` into the project. Configure it to generate API documentation from C# XML comments for `Night`. Setup a GitHub Actions workflow to build and deploy this documentation to GitHub Pages.
  - **Acceptance Criteria:** API documentation is automatically generated and published to a GitHub Pages site.
  - **Status:** Review

- [ ] **Task 10.9: Formalize Basic Test Suite**
  - **Description:** Review current testing strategy (`Night.SampleGame` as integration test [cite: 209]). Establish a basic structure for more formal tests if deemed necessary (e.g., a separate test project). Add minimal unit tests for any new complex, non-SDL-dependent logic introduced in this epic (e.g., utility functions in `Night.Filesystem` or `Night.Timer`).
  - **Implementation:**
    - [x] Created `Night.Tests` project (`tests/Night.Tests/Night.Tests.csproj`) for xUnit tests. (Path corrected)
    - [x] Added `tests/Night.Tests/Graphics/GraphicsTests.cs` with unit tests for `Night.Graphics` methods, focusing on parameter validation and behavior with null `Window.RendererPtr`. Tests cover:
      - `NewImage()`: Null/non-existent file paths.
      - `Draw()`: Null sprite, sprite with null texture.
      - `Rectangle()`: Invalid dimensions.
      - `Line(PointF[])`: Null/insufficient points.
      - `Polygon()`: Null/insufficient vertices.
      - `Circle()`: Invalid segments/radius.
      - General graphics operations (`SetColor`, `Clear`, `Present`, shape drawing) when `Window.RendererPtr` is null.
    - [x] Added XML documentation comments to all public members in `tests/Night.Tests/Graphics/GraphicsTests.cs` to resolve build warnings.
    - [x] Added `tests/Night.Tests/Keyboard/KeyboardTests.cs` with unit tests for `Night.Keyboard.IsDown()` method, focusing on C# logic paths:
      - Input system not initialized.
      - Unknown `KeyCode`.
      - `KeyCode` (scancode) out of bounds.
    - [x] Modified `Framework.IsInputInitialized` in `src/Night/Framework.cs` to have an `internal` setter.
    - [x] Added `InternalsVisibleTo("Night.Tests")` to `src/Night/Night.csproj` to allow test project access for setting `Framework.IsInputInitialized`.
    - [x] Added MSBuild `Content` items to `tests/Night.Tests/Night.Tests.csproj` to copy SDL3 and SDL3_image native binaries to the output directory, resolving `DllNotFoundException` for `KeyboardTests`.
    - [ ] Added `tests/Night.Tests/Mouse/MouseTests.cs` with unit tests for `Night.Mouse` methods (`IsDown`, `GetPosition`, `SetVisible`, `SetGrabbed`, `SetRelativeMode`). Tests focus on C# logic paths, parameter validation (e.g., `MouseButton.Unknown`), and behavior when `Framework.IsInputInitialized` is false or `Window.Handle` is `nint.Zero`. Console warnings are also checked.
    - [x] Create `tests/Night.Tests/Window/WindowTests.cs` with unit tests for `Night.Window` methods, focusing on C# logic paths, parameter validation, and behavior when `Framework.IsInputInitialized` is false or `Window.Handle` is `nint.Zero`.
    - [x] Added `tests/Night.Tests/SDL/NightSDLTests.cs` with unit tests for `Night.NightSDL.ParseVersion()` method, focusing on C# version parsing logic.
    - [x] Added `tests/Night.Tests/Timer/TimerTests.cs` with unit tests for `Night.Timer` methods, focusing on C# logic, parameter validation, and internal state management.
  - **Acceptance Criteria:** A clear testing strategy for 0.1.0 is in place. Critical utility functions and core framework modules like `Night.Graphics`, `Night.Keyboard`, `Night.Mouse`, `Night.Timer`, and `Night.Window` have basic unit tests covering C# logic and parameter validation. Test files are documented to avoid build warnings. Tests for `Night.Keyboard` can correctly manipulate necessary framework state for testing and can locate native SDL binaries.
  - **Status:** In-Progress // Added Window.cs tests, Mouse.cs tests, Keyboard.cs tests, Documented GraphicsTests.cs, Fixed KeyboardTests build issues, Added SDL native copy to Night.Tests

- [x] **Task 10.10: Create Project Logo and Icon**
  - **Description:** Design or procure a logo for Night Engine. Prepare application icon files (e.g., .ico, .icns) and integrate them so the `Night.SampleGame` window uses the icon. `Night.Window.SetIcon()` would be needed if not already planned. (Roadmap `love.window` has `setIcon` [cite: 1291, 1327] as out of scope, may need to be scoped in for this).
  - **Implementation Details:**
    - [x] Design/Procure Night Engine logo and create `.ico` and `.icns` files. (Responsibility of User)
    - [x] Add `private static string? currentIconPath;` to `Night.Window`.
    - [x] Implement `public static bool Night.Window.SetIcon(string imagePath)`:
      - Takes a file path string (e.g., ".ico", ".bmp"). Uses `SDL.LoadBMP` for loading.
      - Sets the window icon using `SDL.SetWindowIcon`.
      - Converts loaded surface to RGBA8888 format, extracts pixel data into a `Night.ImageData` object, and stores it.
    - [x] Implement `public static Night.ImageData? Night.Window.GetIcon()`:
      - Returns the stored `Night.ImageData` object (or null).
    - [x] Update `Night.Configuration.GameConfig` to include `IconPath` in `WindowConfig` (and split config classes to separate files).
    - [x] Update `Night.FrameworkLoop.Run()` (now `Framework.Run()`) to load icon from `config.json` if specified.
    - [x] Update `Night.SampleGame` to call `SetIcon` (commented out, driven by config) and include a sample icon file (user to provide actual file, config updated).
    - [x] Update `project/love2d-api/modules/window.md` for `SetIcon` and `GetIcon`.
  - **Acceptance Criteria:** Project has a logo. Sample game displays a custom window icon. `Night.Window.GetIcon()` returns the path of the set icon.
  - **Status:** In-Progress

- [x] **Task 10.11: Establish Basic CI Workflow**
  - **Description:** Review deactivated CI workflows. Create a new, active GitHub Actions workflow that, at a minimum, builds `Night` and `SampleGame` on push/PR to main branch for Windows, Linux, and macOS. Run any automated tests established in Task 10.9.
  - **Acceptance Criteria:** CI workflow successfully builds and (if applicable) tests the project on all target OS upon code changes.
  - **Status:** Done (Basic requirements met by existing `.github/workflows/ci.yml`. Further enhancements can be added.)

- [x] **Task 10.12: Create API Documentation Script**
  - **Description:** Write a new Python script `scripts/get_api.py`. This script will parse all C# files in `src/Night/` and its subdirectories. It will generate a markdown file listing all public static classes and their public static functions (including overloads). The script should attempt to derive an equivalent Love2D API call for each function.
  - **Output Format:**
    - Modules (classes) should be Header Level 2.
    - Functions should be an unordered list item: `FunctionName() - love.module.functionName`
    - Overloaded functions should have a nested unordered list detailing each overload with parameters:
      - `FunctionName(paramType1 paramName1, paramType2 paramName2)`
  - **Example:**

    ```markdown
    ## filesystem
    - GetInfo() - love.filesystem.getInfo
      - GetInfo(string path, FileSystemInfo info)
      - GetInfo(string path, FileType filterType, FileSystemInfo info)
    ```

  - **Acceptance Criteria:** The script `scripts/get_api.py` is created and generates a markdown file as specified. The markdown file accurately reflects the public API of `src/Night.Engine/Framework`.
  - **Status:** To Do

[x] **Task 10.13: Refactor Project Structure, Naming, and Documentation**

- **Description:** Restructure the project's directories and C# project files to align with the "Night" and "Night.Engine" namespace strategy. Update all relevant documentation to reflect these changes. The goal is to have a primary assembly named `Night.dll` which contains both the `Night` (framework) and `Night.Engine` (engine extensions) namespaces.

- **Implementation Details:**
  - **1. Directory Renaming and Restructuring:**
    - [x] Rename the main C# project directory from `src/Night.Engine/` to `src/Night/`.
    - [x] Move all contents of the former `src/Night.Engine/Framework/` directory (e.g., `Graphics/`, `Window/`, etc.) directly into the new `src/Night/` directory.

      - Example: `src/Night.Engine/Framework/Graphics/Graphics.cs` becomes `src/Night/Graphics/Graphics.cs`.

    - [x] Move the contents of the former `src/Night.Engine/Engine/` directory into a new `Engine` subdirectory within `src/Night/`.

      - Example: `src/Night.Engine/Engine/.gitkeep` becomes `src/Night/Engine/.gitkeep`.

      - Future engine components like `SceneManager.cs` would go into `src/Night/Engine/`.

  - **2. C# Project File (.csproj) Adjustments:**

    - [x] Rename the C# project file from `src/Night.Engine/Night.Engine.csproj` to `src/Night/Night.csproj`.

    - [x] Update the `src/Night/Night.csproj` file:

      - [x] Modify the `<AssemblyName>` property (or add it if not present) to ensure the output assembly is named `Night`. (e.g., `<AssemblyName>Night</AssemblyName>`)

      - [x] Verify that all source file paths (`<Compile Include="..." />` if explicit, or implicit globbing patterns) are correct after the directory restructuring.

  - **3. Solution File (`.sln`) Update:**

    - [x] Edit the `Night.sln` file to reflect the new path and name of the C# project file.

      - Example: Change `Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Night.Engine", "src\Night.Engine\Night.Engine.csproj", "{...}"`

      - To: `Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "Night", "src\Night\Night.csproj", "{...}"` (The project GUID `{...}` should remain the same).

      - [x] Also update `Night.Engine.Tests` to `Night.Tests` and its path in the solution.

  - **4. Namespace Verification (Conceptual - No Code Change unless inconsistencies found):**

    - [x] Conceptually verify that all files moved from `src/Night.Engine/Framework/*` are already in the `Night` namespace (or sub-namespaces like `Night.Graphics`).

    - [x] Conceptually verify that any files moved to `src/Night/Engine/*` will use the `Night.Engine` namespace.

  - **5. Documentation Updates:**

    - [x] **`project/PRD.md`:**

      - [x] Update Section 3 "Technical Specifications"

        - Change primary library name from `Night.Engine` to `Night`.

        - Reflect that the output DLL is `Night.dll`.

        - Clarify that this `Night.dll` contains both `Night` (framework) and `Night.Engine` (engine extensions) namespaces.

      - [x] Update Section 4 "Project Structure"

        - Modify the Mermaid diagram and textual descriptions to show the new `src/Night/` top-level directory for the main library.

        - Show module directories (e.g., `Graphics`, `Window`) directly under `src/Night/`.

        - Show the `Engine` subdirectory as `src/Night/Engine/`.

      - [x] Update Section 5 "File Descriptions"

        - Change `src/Night.Engine/Night.Engine.csproj` to `src/Night/Night.csproj`.

        - Describe `Night.csproj` as the project file for the main `Night.dll` library.

    - [x] **`README.md`:**

      - [x] Review and update any mentions of `Night.Engine` as the primary library name or `Night.Engine.dll` if they exist.

      - [x] Ensure any "Getting Started" or API usage examples correctly reflect `using Night;` and `using Night.Engine;` and the concept of a single `Night.dll`.

    - [x] **`project/epics/*.md` (especially `epic10.md` and any active epics):**

      - [x] Review all task descriptions and implementation details.

      - [x] Update file paths (e.g., references to `src/Night.Engine/Framework/...` should become `src/Night/...`).

      - [x] Update references to `Night.Engine.csproj` to `Night.csproj`.

    - [x] **`project/guidelines.md`:**

      - [x] Review for any path or project name specifics that might need updating.

    - [x] **`.github/workflows/ci.yml` (and any other relevant workflows):**

      - [x] Update paths to the solution file (`Night.sln` - likely no change here unless solution name changes).

      - [x] Update paths to the C# project file if explicitly referenced (e.g., `dotnet build src/Night/Night.csproj`).

      - [x] Ensure build steps correctly produce `Night.dll`.

    - [x] **`scripts/update_api_doc.py` (Task 10.12 in `epic10.md`):**

      - [x] Ensure the script's `framework_dir` points to `src/Night/` (or its relevant subdirectories like `src/Night/Graphics`, etc., if it iterates that way) instead of `src/Night.Engine/Framework/`.

- **Acceptance Criteria:**

  - The project directory structure is updated as specified.

  - The C# project file is renamed to `Night.csproj`, located in `src/Night/`, and configured to output `Night.dll`.

  - The `Night.sln` file correctly references the renamed and relocated project.

  - The solution builds successfully, producing `Night.dll`.

  - The `Night.SampleGame` project still builds and runs correctly, referencing the new `Night.dll` and using the `Night` and `Night.Engine` namespaces as intended.

  - All specified documentation files (`PRD.md`, `README.md`, relevant epics, CI workflows, API doc script) are updated to accurately reflect the new structure, naming, and API organization.

- **Status:** Review

- [x] **Task 10.14: Fix Linting Errors**
  - **Description:** Address StyleCop linting errors reported in the codebase.
  - **Errors to fix:**
    - `src/Night/Filesystem/Types.cs(33,12): error SA1201: A constructor should not follow a property`
    - `src/Night/Graphics/Types.cs(35,12): error SA1201: A constructor should not follow a property`
    - `src/Night/Timer/Timer.cs(54,26): error SA1202: 'public' members should come before 'internal' members`
    - `src/Night/FrameworkLoop.cs(101,24): error SA1202: 'public' members should come before 'private' members`
    - `src/Night/FrameworkLoop.cs(29,23): error SA1203: Constant fields should appear before non-constant fields`
    - `src/Night/Window/Window.cs(152,23): error SA1202: 'public' members should come before 'internal' members`
    - `src/Night/Graphics/Structs.cs(30,34): error SA1201: A field should not follow a constructor`
    - `src/Night/Configuration/GameConfig.cs(42,16): error SA1402: File may only contain a single type`
    - `src/Night/Configuration/GameConfig.cs(51,16): error SA1402: File may only contain a single type`
    - `src/Night/Configuration/GameConfig.cs(111,16): error SA1402: File may only contain a single type`
    - `src/Night/ErrorHandler.cs(19,23): error SA1649: File name should match first type name`
    - `src/Night/Filesystem/Types.cs(12,16): error SA1649: File name should match first type name`
    - `src/Night/FrameworkLoop.cs(21,23): error SA1649: File name should match first type name`
    - `src/Night/Timer/Timer.cs(36,25): error SA1201: A property should not follow a method`
    - `src/Night/Timer/Timer.cs(69,23): error SA1202: 'public' members should come before 'internal' members`
    - `src/Night/FrameworkLoop.cs(32,23): error SA1203: Constant fields should appear before non-constant fields`
    - `src/Night/Window/Window.cs(173,43): error SA1202: 'public' members should come before 'internal' members`
    - `src/Night/Graphics/Enums.cs(12,15): error SA1649: File name should match first type name`
    - `src/Night/Window/Window.cs(50,32): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Window/Window.cs(203,25): error SA1316: Tuple element names should use correct casing`
    - `src/Night/Window/Window.cs(203,54): error SA1316: Tuple element names should use correct casing`
    - `src/Night/Mouse/Mouse.cs(64,24): error SA1316: Tuple element names should use correct casing`
    - `src/Night/Mouse/Mouse.cs(64,31): error SA1316: Tuple element names should use correct casing`
    - `src/Night/Timer/Timer.cs(121,57): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Timer/Timer.cs(128,38): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Window/Window.cs(340,48): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Window/Window.cs(257,14): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Window/Window.cs(289,12): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Graphics/Structs.cs(14,17): error SA1649: File name should match first type name`
    - `src/Night/Graphics/Graphics.cs(130,12): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Graphics/Types.cs(12,16): error SA1649: File name should match first type name`
    - `src/Night/Configuration/ConfigurationManager.cs(66,30): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/FrameworkLoop.cs(438,68): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/FrameworkLoop.cs(460,33): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Graphics/Graphics.cs(233,12): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Graphics/Graphics.cs(281,81): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Graphics/Graphics.cs(277,38): error SA1117: The parameters should all be placed on the same line or each parameter should be placed on its own line`
    - `src/Night/Mouse/Enums.cs(14,15): error SA1649: File name should match first type name`
    - `src/Night/FrameworkLoop.cs(264,29): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/FrameworkLoop.cs(274,30): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/FrameworkLoop.cs(291,30): error SA1108: Block statements should not contain embedded comments`
    - `src/Night/Types.cs(13,20): error SA1649: File name should match first type name`
    - `src/Night/Window/Enums.cs(12,15): error SA1649: File name should match first type name`
    - `src/Night/Window/Structs.cs(12,17): error SA1649: File name should match first type name`
    - `CSC : error SA0001: XML comment analysis is disabled due to project configuration`
    - `src/Night/Window/Window.cs(306,11): error CS1501: No overload for method 'GetFullscreenDisplayModes' takes 3 arguments`
  - **Acceptance Criteria:** All listed StyleCop errors are resolved.
  - **Status:** In-Progress
