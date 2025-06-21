# Epic: Joystick and Gamepad Callbacks

**User Story:** As a game developer using Night.Framework, I want to be able to respond to joystick and gamepad events (axis movement, button presses/releases, device connections/disconnections) via Love2D-style callbacks, so I can easily implement input handling for these devices.

**Status:** In-Progress (Phase 4 Ready for Review)

## Requirements

Implement the following Love2D equivalent callback functions in `Night.Framework`:

- `love.gamepadaxis(joystick, axis, value)`: Called when a Joystick's virtual gamepad axis is moved.
- `love.gamepadpressed(joystick, button)`: Called when a Joystick's virtual gamepad button is pressed.
- `love.gamepadreleased(joystick, button)`: Called when a Joystick's virtual gamepad button is released.
- `love.joystickadded(joystick)`: Called when a Joystick is connected.
- `love.joystickaxis(joystick, axis, value)`: Called when a joystick axis moves.
- `love.joystickhat(joystick, hat, direction)`: Called when a joystick hat direction changes.
- `love.joystickpressed(joystick, button)`: Called when a joystick button is pressed.
- `love.joystickreleased(joystick, button)`: Called when a joystick button is released.
- `love.joystickremoved(joystick)`: Called when a Joystick is disconnected.

## Overall Design Considerations

- **Namespace:** All Joystick related classes and enums (e.g., `Joystick`, `Joysticks`, `GamepadAxis`, `GamepadButton`, `JoystickHat`) will reside directly in the `Night` namespace.
- **SDL Subsystem Initialization:** The `SDL.InitFlags.Joystick` and `SDL.InitFlags.Gamepad` flags must be added during `SDL.Init()` in `Framework.Run.cs`.
- **Error Handling:** Robust error checking for SDL function calls and clear logging for joystick operations.
- **`Night.Game` Base Class:** The `Night.Game` base class will be updated to provide default (empty) virtual implementations for all new joystick-related `IGame` callbacks. This makes it optional for developers to implement them.

## Phase 0: Refactor `Framework.Run.cs` (Prerequisite)

**Goal:** Improve maintainability of `Framework.Run.cs` by separating event polling and processing logic. This file is currently very large.

- [x] **P0.T1: Analyze `Framework.Run.cs` for Refactoring Points.**
  - [x] Identify sections related to SDL event polling and the main `while (SDL.PollEvent...){}` loop.
- [x] **P0.T2: Create `Framework.Events.cs` (Partial Class).**
  - [x] Define `Framework` as a `partial class` in a new file `src/Night/Framework.Events.cs`.
  - [x] Move the main SDL event polling loop and event handling switch/if-else statements into a new private static method within this partial class (e.g., `private static void ProcessSdlEvents(IGame game)`).
  - [x] The existing `Run` method will call this new `ProcessSdlEvents` method.
- [ ] **P0.T3: (Optional) Create `Framework.InputEventHandlers.cs` (Partial Class).**
  - [ ] If further granularity is desired, specific event handling logic (e.g., for `KeyDown`, `MouseButtonDown`) could be moved into separate private static methods, potentially in another partial class file like `Framework.InputEventHandlers.cs`. These would be called from `ProcessSdlEvents`.
  - *Decision for now: Start with `Framework.Events.cs` and evaluate if further splitting is necessary after joystick events are added.*
- [x] **P0.V1: Verification:**
  - [x] Compile the project successfully.
  - [x] Run `SampleGame` and verify existing input (keyboard/mouse) and window events function as before.

## Phase 1: Core Joystick and Gamepad Infrastructure

**Goal:** Establish the foundational classes and interface changes for joystick support.

- [x] **P1.T1: Update `Night.IGame.cs`.**
  - [x] Add all 9 new joystick/gamepad callback method signatures as defined in "Requirements".
  - [x] Ensure parameters use types from the `Night` namespace (e.g., `Night.Joystick`, `Night.GamepadAxis`).
- [x] **P1.T2: Update `Night.Game.cs`.**
  - [x] Add `virtual void` empty implementations for all 9 new joystick/gamepad callbacks from `IGame`.
- [ ] **P1.T3: Enhance `Night.Joystick.cs`.** (Partially Done - known SDL binding issues)
  - [x] Ensure the class `Joystick` is in the `Night` namespace.
  - [ ] Implement all placeholder methods (`GetAxisCount`, `GetButtonCount`, `GetHatCount`, `GetName`, `GetGuid`, `GetId`, `IsConnected`, `IsDown`, `IsGamepad`, `GetGamepadAxis`, `IsGamepadDown`, `SetVibration`, `IsVibrationSupported`, etc.) using appropriate SDL3-CS calls.
    - [x] `GetId()` should return the SDL Joystick Instance ID.
    - [x] `IsConnected()` will be managed by the `Joysticks` class. Add an internal `SetConnectedState(bool connected)` method.
    - [ ] Handle potential missing SDL3-CS bindings gracefully (e.g., for mapping strings or advanced rumble features, return default/null values and log a warning). **(Known issue for `GetGamepadMappingString` and `IsVibrationSupported`)**
  - [x] Add necessary private fields (e.g., `_gamepadDevicePtr` for `SDL.OpenGamepad`).
  - [x] Ensure `Dispose()` correctly closes both joystick and gamepad pointers if open.
- [x] **P1.T4: Implement `Night.Joysticks.cs` (Static Class `Night.Joysticks`).**
  - [x] Ensure the static class `Joysticks` is in the `Night` namespace.
  - [x] Implement a static dictionary `private static readonly Dictionary<uint, Joystick> ActiveJoysticks`.
  - [x] Implement `public static List<Joystick> GetJoysticks()`.
  - [x] Implement `public static int GetJoystickCount()`.
  - [x] Implement `internal static Joystick? AddJoystick(uint instanceId)`.
  - [x] Implement `internal static Joystick? RemoveJoystick(uint instanceId)`.
  - [x] Implement `internal static Joystick? GetJoystickByInstanceId(uint instanceId)`.
  - [x] Implement `internal static void ClearJoysticks()` for shutdown.
- [x] **P1.V1: Verification (Code Review & Compilation):**
  - [x] All new and modified classes compile without errors.
  - [x] Code adheres to project guidelines.
  - [x] Namespace usage is correct (`Night` for public types).

## Phase 2: SDL Initialization and Basic Joystick Connection/Disconnection Events

**Goal:** Initialize SDL joystick subsystems and handle add/remove events.

- [x] **P2.T1: Modify `Framework.Run.cs` (or `Framework.Events.cs` after P0).**
  - [x] In the SDL initialization block, add `SDL.InitFlags.Joystick | SDL.InitFlags.Gamepad` to `initializedSubsystemsFlags`.
  - [x] In the `finally` block of the `Run` method, add a call to `Night.Joysticks.ClearJoysticks()`.
- [x] **P2.T2: Implement `JoystickAdded` and `JoystickRemoved` Event Handling.**
  - [x] In `ProcessSdlEvents` (or equivalent after P0):
    - [x] Handle `SDL.EventType.JoystickAdded`:
      - [x] Call `Night.Joysticks.AddJoystick(e.JDevice.Which)`.
      - [x] If successful, call `game.JoystickAdded(newJoystick)`.
    - [x] Handle `SDL.EventType.JoystickRemoved`:
      - [x] Call `Night.Joysticks.RemoveJoystick(e.JDevice.Which)`.
      - [x] If successful, call `game.JoystickRemoved(removedJoystick)`, then `removedJoystick.Dispose()`.
- [x] **P2.V1: Verification (SampleGame):**
  - [x] Modify `src/SampleGame/Program.cs` (or a specific sample game class).
  - [x] Override `JoystickAdded` and `JoystickRemoved` in the sample game.
  - [x] Log messages to the console when a joystick is connected or disconnected.
  - [x] Test by plugging and unplugging a controller. Verify `Night.Joysticks.GetJoystickCount()` and `Night.Joysticks.GetJoysticks()` reflect the changes.

## Phase 3: Implement Raw Joystick Input Events (Buttons, Axes, Hats)

**Goal:** Handle raw inputs from any connected joystick device.

- [x] **P3.T1: Implement `JoystickAxis`, `JoystickPressed`, `JoystickReleased`, `JoystickHat` Event Handling.**
  - [x] In `ProcessSdlEvents` (or equivalent):
    - [x] Handle `SDL.EventType.JoystickAxisMotion`:
      - [x] Get `Joystick` instance using `e.JAxis.Which`.
      - [x] Normalize axis value from `e.JAxis.Value` (-32768 to 32767) to -1.0f to 1.0f (create a private static helper `NormalizeSdlAxisValue` in `Framework.Events.cs`).
      - [x] Call `game.JoystickAxis(joystick, (int)e.JAxis.Axis, normalizedValue)`.
    - [x] Handle `SDL.EventType.JoystickButtonDown`:
      - [x] Get `Joystick` instance. Call `game.JoystickPressed(joystick, (int)e.JButton.Button)`.
    - [x] Handle `SDL.EventType.JoystickButtonUp`:
      - [x] Get `Joystick` instance. Call `game.JoystickReleased(joystick, (int)e.JButton.Button)`.
    - [x] Handle `SDL.EventType.JoystickHatMotion`:
      - [x] Get `Joystick` instance. Cast `e.JHat.Value` (which is `SDL.Hat` enum) to `Night.JoystickHat`.
      - [x] Call `game.JoystickHat(joystick, (int)e.JHat.Hat, (Night.JoystickHat)e.JHat.Value)`.
- [x] **P3.V1: Verification (SampleGame):**
  - [x] Expand `SampleGame` to override and log all raw joystick callbacks.
  - [x] Test with a controller: move axes, press buttons, use D-Pad/hats.
  - [x] Verify correct joystick instance, axis/button/hat indices, and values are reported.

## Phase 4: Implement Gamepad-Specific Input Events (Virtual Axes/Buttons)

**Goal:** Handle inputs from joysticks that are recognized as standard gamepads, using SDL's gamepad API.

**Status:** P4.V1 Complete (2025-06-15)

- [x] **P4.T1: Implement `GamepadAxis`, `GamepadPressed`, `GamepadReleased` Event Handling.**
  - [x] In `ProcessSdlEvents` (or equivalent):
    - [x] Handle `SDL.EventType.GamepadAxisMotion`:
      - [x] Get `Joystick` instance using `e.GAxis.Which`. Check `joystick.IsGamepad()`.
      - [x] Normalize axis value from `e.GAxis.Value`.
      - [x] Map `e.GAxis.Axis` (an `SDL.GamepadAxis`) to `Night.GamepadAxis` (create `private static Night.GamepadAxis MapSdlGamepadAxisToNight(SDL.GamepadAxis sdlAxis)` helper).
      - [x] Call `game.GamepadAxis(joystick, nightAxis, normalizedValue)`.
    - [x] Handle `SDL.EventType.GamepadButtonDown`:
      - [x] Get `Joystick` instance. Check `joystick.IsGamepad()`.
      - [x] Map `e.GButton.Button` (an `SDL.GamepadButton`) to `Night.GamepadButton` (create `private static Night.GamepadButton MapSdlGamepadButtonToNight(SDL.GamepadButton sdlButton)` helper).
      - [x] Call `game.GamepadPressed(joystick, nightButton)`.
    - [x] Handle `SDL.EventType.GamepadButtonUp`:
      - [x] Get `Joystick` instance. Check `joystick.IsGamepad()`.
      - [x] Map `e.GButton.Button` to `Night.GamepadButton`.
      - [x] Call `game.GamepadReleased(joystick, nightButton)`.
- [x] **P4.V1: Verification (SampleGame - Full Controller Test):**
  - [x] Expand `SampleGame` to specifically test gamepad events.
  - [x] Use a standard gamepad (e.g., Xbox controller).
  - [x] Verify that both raw joystick events AND specific gamepad events are triggered appropriately.
  - [x] Log output from `joystick.GetGamepadAxis()`, `joystick.IsGamepadDown()` in the sample game's `Update` loop to cross-verify with event callbacks.
  - [x] Test basic player movement or actions in `SampleGame` controlled by gamepad inputs.

## Phase 5: Documentation and Final Review

- [x] **P5.T1: Add XML Documentation.**
  - [x] Ensure all new public APIs in `IGame.cs`, `Game.cs`, `Joystick.cs`, `Joysticks.cs` (and related enums) have comprehensive XML documentation comments.
- [x] **P5.T2: Update `project/README.md` or relevant docs.**
  - [x] If necessary, add a section on Joystick and Gamepad usage.
- [x] **P5.T3: Code Cleanup and Final Review.**
  - [x] Review all changes for adherence to `guidelines.md`.
  - [x] Check for any remaining TODOs or potential issues.
  - [x] Ensure logging is appropriate (not too verbose for release, but helpful for debugging).

## Notes & Decisions (To be updated during development)

- The refactoring of `Framework.Run.cs` (Phase 0) is crucial for managing complexity.
- The `Night.Joystick` class will attempt to open the device as an `SDL_Gamepad` if `SDL_IsGamepad` returns true. This allows it to serve data for both raw joystick functions and mapped gamepad functions.
- Mapping SDL event data (raw axis values, SDL-specific enums) to the Love2D-style API (normalized floats, Night-specific enums) will be handled by helper methods within `Framework.Run.cs` (or its partial classes).
- If specific SDL3-CS bindings are missing (e.g., for `SDL_GetJoystickMappingForID` or `SDL_JoystickHasRumble`), the corresponding `Night.Joystick` methods will return default "not supported" values (e.g., `null` for mapping string, `false` for rumble support) and log a warning. **(Current known issue)**

- Addressing 14 build warnings (StyleCop &amp; IDE) as of 2025-06-15 to maintain code quality.

## Dependencies

- SDL3-CS bindings for joystick and gamepad events.

## Risks

- Complexity in mapping SDL event details to Love2D-style callback parameters.
- Ensuring correct `Night.Joystick` instance management during hot-plugging.
- Potential missing or differently named functions in the specific version of SDL3-CS being used. **(Currently impacting `Joystick.cs`)**
- The large size of `Framework.Run.cs` might make refactoring and adding new event logic error-prone if not handled carefully in phases.
