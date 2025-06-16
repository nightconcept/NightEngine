# Epic: Implement Mouse Module (Night.Mouse)

**User Story:** As a game developer, I want a comprehensive mouse input interface (`Night.Mouse`), so I can effectively manage mouse position, button states, cursor appearance, visibility, and input modes (e.g., relative mode, grabbed mode) within my game, similar to the capabilities offered by Love2D's `love.mouse` module.

**Overall Requirements:**

*   Implement the `Night.Mouse` static class to provide an interface to the user's mouse.
*   All public APIs should reside within the `Night.Mouse` class or related types within the `Night` namespace (e.g., `Night.Mouse.Cursor`, `Night.Mouse.CursorType`).
*   The implementation should primarily use SDL3 functions via `SDL3-CS` bindings.
*   All functions and types must be documented with XML comments explaining their purpose, parameters, and return values, adhering to [`project/guidelines.md`](project/guidelines.md:1).
*   The module code will primarily reside in `src/Night/Mouse/Mouse.cs`.
*   Associated types like `Cursor` and `CursorType` will be defined appropriately (e.g., within `Mouse.cs` or `src/Night/Types.cs` if more general, though `CursorType` is specific).
*   Mouse-related events (mouse moved, wheel moved) should be integrated into the `Night.IGame` interface or a similar event handling mechanism as established in the project (e.g., like `IGame.KeyPressed`).

**Overall Acceptance Criteria:**

*   The `Night.Mouse` static class is available and provides all specified functionalities.
*   Developers can reliably get mouse position, check button states, manage cursor visibility and appearance, and control mouse grab and relative modes.
*   The API is intuitive and follows C# best practices while mirroring Love2D's `love.mouse` module structure.
*   Automated tests for each function and callback exist within the `NightTest` framework (likely in `tests/Groups/Mouse/`), verifying correct behavior under various conditions.
*   The module integrates seamlessly with the existing `Night.Framework`.

**Status:** To Do
**Assigned Agent:** AI Dev Agent
**Date Started:** 2025-06-16
**Date Completed:** TBD

**Implementation Notes & Log:**

*   2025-06-16: Task received. Epic drafted for `Night.Mouse` module.

**Dependencies:**

*   Standard C# libraries.
*   `SDL3-CS` bindings for SDL3.
*   Existing `Night.Framework` project structure and conventions.
*   SDL3 native libraries (for mouse input handling, cursor creation).

**Questions for User:**

*   None at this time.

---

## Detailed Module Breakdown

### Types

#### 1. [ ] `Night.Mouse.Cursor`
*   **Love2D Equivalent:** `love.mouse.Cursor`
*   **Description:** Represents a hardware cursor. Instances are created via `Night.Mouse.NewCursor` or `Night.Mouse.GetSystemCursor`.
*   **C# Definition Idea:**
    ```csharp
    namespace Night.Mouse
    {
        public class Cursor : IDisposable
        {
            // Internal handle to the SDL_Cursor
            internal IntPtr SdlCursorHandle { get; private set; }
            // Potentially other properties like source ImageData, hotX, hotY if needed for recreation or info

            internal Cursor(IntPtr sdlCursorHandle);
            public void Dispose(); // To free the SDL_Cursor
        }
    }
    ```
*   **Requirements:**
    *   Define a `Cursor` class within the `Night.Mouse` namespace (or `Night` if preferred for types).
    *   The class should encapsulate an SDL cursor resource (`SDL_Cursor*`).
    *   Implement `IDisposable` to manage the lifecycle of the native SDL cursor resource.
*   **Acceptance Criteria:**
    *   `Night.Mouse.Cursor` class exists and can be instantiated by `NewCursor` and `GetSystemCursor`.
    *   `Dispose()` method correctly releases the underlying SDL cursor resource.
    *   Instances of `Cursor` can be successfully used with `Night.Mouse.SetCursor()`.
*   **Test Scenarios/Cases:**
    *   `Cursor_CreationAndDisposal`: Verify a cursor can be created (via `NewCursor`) and disposed of without errors.
    *   `Cursor_SetCurrent`: Verify a created `Cursor` object can be set as the current cursor.

---

### Enums

#### 1. [ ] `Night.Mouse.CursorType`
*   **Love2D Equivalent:** `love.mouse.CursorType`
*   **Description:** Standard system cursor types.
*   **C# Definition Idea:**
    ```csharp
    namespace Night.Mouse // Or Night.Types
    {
        public enum CursorType
        {
            Arrow,
            IBeam,
            Wait,
            Crosshair,
            WaitArrow,  // Also known as AppStarting in some systems
            SizeNWSE,   // Diagonal resize 1 (top-left to bottom-right)
            SizeNESW,   // Diagonal resize 2 (top-right to bottom-left)
            SizeWE,     // Horizontal resize
            SizeNS,     // Vertical resize
            SizeAll,    // Omni-directional resize
            No,         // Not allowed / No cursor
            Hand        // Pointing hand
        }
    }
    ```
*   **Requirements:**
    *   Define a `CursorType` enum.
    *   Include members corresponding to standard Love2D system cursor types.
    *   Map these enum values to the appropriate `SDL_SystemCursor` values.
*   **Acceptance Criteria:**
    *   `Night.Mouse.CursorType` enum exists with all specified standard cursor types.
    *   Each `CursorType` member can be successfully used with `Night.Mouse.GetSystemCursor()`.
*   **Test Scenarios/Cases:**
    *   `CursorType_GetSystemCursors`: Iterate through all `CursorType` values, call `GetSystemCursor` for each, and ensure a non-null `Cursor` object is returned (if supported by the system).
    *   `CursorType_SetSystemCursors`: For each `CursorType`, get the system cursor and attempt to set it, verifying visibility and appearance if possible (manual check might be needed for appearance).

---

### Functions

#### 1. [ ] `Night.Mouse.GetCursor()`
*   **Love2D Equivalent:** `love.mouse.getCursor()`
*   **Description:** Gets the current custom `Cursor` object. Returns `null` if the current cursor is the system cursor or if cursor functionality is not supported.
*   **C# Signature:** `public static Night.Mouse.Cursor? GetCursor()`
*   **Requirements:**
    *   Return the currently active custom `Cursor` object set by `SetCursor(Cursor customCursor)`.
    *   Return `null` if the system cursor is active (i.e., `SetCursor()` was called with `null`, or `SetCursor(systemCursorFromGetSystemCursor)` was called, or no custom cursor has been set).
    *   Return `null` if `IsCursorSupported()` is false.
*   **Acceptance Criteria:**
    *   Method returns the correct `Cursor` object after `SetCursor(customCursor)` is called.
    *   Method returns `null` after `SetCursor(null)` is called.
    *   Method returns `null` if a system cursor (obtained via `GetSystemCursor`) is set.
    *   Method returns `null` by default before any custom cursor is set.
*   **Test Scenarios/Cases:**
    *   `GetCursor_DefaultIsNull`: Verify returns `null` initially.
    *   `GetCursor_AfterSetCustomCursor`: Verify returns the set custom cursor.
    *   `GetCursor_AfterSetSystemCursorIsNull`: Verify returns `null` after setting a system cursor.
    *   `GetCursor_AfterSetNullCursorIsNull`: Verify returns `null` after `SetCursor(null)`.

#### 2. [x] `Night.Mouse.GetPosition()`
*   **Love2D Equivalent:** `love.mouse.getPosition()`
*   **Description:** Returns the current position of the mouse in window coordinates.
*   **C# Signature:** `public static (float X, float Y) GetPosition()`
*   **Requirements:**
    *   Return the current x and y coordinates of the mouse cursor relative to the window's client area.
    *   Coordinates should be floating-point numbers.
*   **Acceptance Criteria:**
    *   Method returns accurate x and y coordinates of the mouse.
    *   Coordinates update correctly as the mouse moves.
    *   If the mouse is outside the window, behavior should align with SDL (typically clamps or gives last known position if not grabbed).
*   **Test Scenarios/Cases:**
    *   `GetPosition_Initial`: Check initial position (might be 0,0 or last known).
    *   `GetPosition_AfterMove`: Simulate mouse move (if possible in test env) or check after manual move.
    *   `GetPosition_AfterSetPosition`: Verify returns coordinates set by `SetPosition`.
    *   `GetPosition_RelativeToWindow`: Ensure coordinates are relative to the game window.

#### 3. [ ] `Night.Mouse.GetRelativeMode()`
*   **Love2D Equivalent:** `love.mouse.getRelativeMode()`
*   **Description:** Gets whether relative mode is enabled for the mouse.
*   **C# Signature:** `public static bool GetRelativeMode()`
*   **Requirements:**
    *   Return `true` if relative mouse mode is enabled, `false` otherwise.
*   **Acceptance Criteria:**
    *   Method returns `false` by default.
    *   Method returns `true` after `SetRelativeMode(true)` is called.
    *   Method returns `false` after `SetRelativeMode(false)` is called.
*   **Test Scenarios/Cases:**
    *   `GetRelativeMode_DefaultIsFalse`: Verify returns `false` initially.
    *   `GetRelativeMode_AfterSetTrue`: Verify returns `true` after enabling.
    *   `GetRelativeMode_AfterSetFalse`: Verify returns `false` after disabling.

#### 4. [ ] `Night.Mouse.GetSystemCursor()`
*   **Love2D Equivalent:** `love.mouse.getSystemCursor(cursortype)`
*   **Description:** Gets a `Cursor` object representing a system-native hardware cursor.
*   **C# Signature:** `public static Night.Mouse.Cursor GetSystemCursor(Night.Mouse.CursorType cursorType)`
*   **Requirements:**
    *   Create and return a `Cursor` object for the specified system `CursorType`.
    *   The returned `Cursor` can be used with `SetCursor()`.
    *   Handle cases where a specific system cursor might not be available (SDL might return a default).
*   **Acceptance Criteria:**
    *   Method returns a non-null `Cursor` object for valid `CursorType` values.
    *   The returned `Cursor` can be successfully used with `SetCursor()`.
*   **Test Scenarios/Cases:**
    *   `GetSystemCursor_AllTypes`: For each `CursorType`, get the cursor and verify it's not null.
    *   `GetSystemCursor_SetAndVerify`: Get a system cursor, set it, and (manually or programmatically if possible) verify the cursor changes.

#### 5. [ ] `Night.Mouse.GetX()`
*   **Love2D Equivalent:** `love.mouse.getX()`
*   **Description:** Returns the current x-position of the mouse.
*   **C# Signature:** `public static float GetX()`
*   **Requirements:**
    *   Return the x-coordinate of `GetPosition()`.
*   **Acceptance Criteria:**
    *   Method returns the same x-coordinate as `GetPosition().X`.
*   **Test Scenarios/Cases:**
    *   `GetX_MatchesGetPosition`: Verify `GetX()` equals `GetPosition().X`.
    *   `GetX_AfterSetPosition`: Verify `GetX()` after `SetPosition`.

#### 6. [ ] `Night.Mouse.GetY()`
*   **Love2D Equivalent:** `love.mouse.getY()`
*   **Description:** Returns the current y-position of the mouse.
*   **C# Signature:** `public static float GetY()`
*   **Requirements:**
    *   Return the y-coordinate of `GetPosition()`.
*   **Acceptance Criteria:**
    *   Method returns the same y-coordinate as `GetPosition().Y`.
*   **Test Scenarios/Cases:**
    *   `GetY_MatchesGetPosition`: Verify `GetY()` equals `GetPosition().Y`.
    *   `GetY_AfterSetPosition`: Verify `GetY()` after `SetPosition`.

#### 7. [ ] `Night.Mouse.IsCursorSupported()`
*   **Love2D Equivalent:** `love.mouse.isCursorSupported()` (Added since 11.0)
*   **Description:** Gets whether custom cursor functionality is supported by the system.
*   **C# Signature:** `public static bool IsCursorSupported()`
*   **Requirements:**
    *   Return `true` if the system can create and set custom hardware cursors.
    *   Return `false` otherwise (e.g., on platforms without such support or if SDL fails to initialize cursor system).
*   **Acceptance Criteria:**
    *   Method returns a boolean indicating system support for custom cursors.
    *   If `false`, `NewCursor` might fail or `SetCursor` with a custom cursor might not work as expected.
*   **Test Scenarios/Cases:**
    *   `IsCursorSupported_ReturnsBool`: Verify the method returns a boolean value. (Actual value depends on test environment).
    *   `IsCursorSupported_BehaviorOfNewCursor`: If `false`, test behavior of `NewCursor` (e.g., throws exception or returns null).

#### 8. [x] `Night.Mouse.IsDown()`
*   **Love2D Equivalent:** `love.mouse.isDown(button, ...)`
*   **Description:** Checks whether a certain mouse button is currently pressed.
*   **C# Signature:** `public static bool IsDown(Night.MouseButton button)` (Assuming `Night.MouseButton` enum exists from `Types.cs` for left, right, middle, x1, x2, etc.)
*   **Requirements:**
    *   Return `true` if the specified `button` is currently held down, `false` otherwise.
    *   Support multiple buttons (left, right, middle, and extended buttons if available).
*   **Acceptance Criteria:**
    *   Method returns `true` only when the specified button is pressed.
    *   Method returns `false` when the button is not pressed.
    *   Correctly identifies different mouse buttons.
*   **Test Scenarios/Cases:**
    *   `IsDown_NotPressed`: Verify returns `false` for all buttons when none are pressed.
    *   `IsDown_LeftButtonPressed`: Verify returns `true` for left button when pressed, `false` for others.
    *   `IsDown_RightButtonPressed`: Verify returns `true` for right button when pressed.
    *   `IsDown_MiddleButtonPressed`: Verify returns `true` for middle button when pressed.
    *   `IsDown_MultipleButtons`: (If Love2D supports checking multiple, adapt. Signature implies one button at a time).

#### 9. [ ] `Night.Mouse.IsGrabbed()`
*   **Love2D Equivalent:** `love.mouse.isGrabbed()`
*   **Description:** Checks if the mouse is grabbed.
*   **C# Signature:** `public static bool IsGrabbed()`
*   **Requirements:**
    *   Return `true` if the mouse is currently grabbed (confined to the window), `false` otherwise.
*   **Acceptance Criteria:**
    *   Method returns `false` by default.
    *   Method returns `true` after `SetGrabbed(true)` is called.
    *   Method returns `false` after `SetGrabbed(false)` is called.
*   **Test Scenarios/Cases:**
    *   `IsGrabbed_DefaultIsFalse`: Verify returns `false` initially.
    *   `IsGrabbed_AfterSetTrue`: Verify returns `true` after enabling grab.
    *   `IsGrabbed_AfterSetFalse`: Verify returns `false` after disabling grab.

#### 10. [ ] `Night.Mouse.IsVisible()`
*   **Love2D Equivalent:** `love.mouse.isVisible()`
*   **Description:** Checks if the cursor is visible.
*   **C# Signature:** `public static bool IsVisible()`
*   **Requirements:**
    *   Return `true` if the mouse cursor is currently visible, `false` otherwise.
*   **Acceptance Criteria:**
    *   Method returns `true` by default (or based on SDL's default).
    *   Method returns `false` after `SetVisible(false)` is called.
    *   Method returns `true` after `SetVisible(true)` is called.
*   **Test Scenarios/Cases:**
    *   `IsVisible_Default`: Verify initial visibility state.
    *   `IsVisible_AfterSetFalse`: Verify returns `false` after hiding.
    *   `IsVisible_AfterSetTrue`: Verify returns `true` after showing.

#### 11. [ ] `Night.Mouse.NewCursor()`
*   **Love2D Equivalent:** `love.mouse.newCursor(imageData, hotx, hoty)`
*   **Description:** Creates a new hardware `Cursor` object from image data.
*   **C# Signature:** `public static Night.Mouse.Cursor? NewCursor(Night.Graphics.ImageData imageData, int hotX, int hotY)` (Assuming `Night.Graphics.ImageData` exists)
*   **Requirements:**
    *   Create a custom hardware cursor from the provided `ImageData`.
    *   `hotX` and `hotY` define the cursor's hot spot (the point of the cursor that interacts).
    *   Return the new `Cursor` object.
    *   Return `null` or throw an exception if cursor creation fails (e.g., `IsCursorSupported()` is false, invalid image data, system limits).
*   **Acceptance Criteria:**
    *   Method returns a valid `Cursor` object when given valid `ImageData`, `hotX`, and `hotY`.
    *   The created cursor can be used with `SetCursor()`.
    *   Method handles failure cases gracefully (returns `null` or throws documented exception).
    *   Hotspot is correctly applied to the created cursor.
*   **Test Scenarios/Cases:**
    *   `NewCursor_ValidData`: Create cursor with valid image and hotspot, verify non-null `Cursor`.
    *   `NewCursor_InvalidData`: Test with invalid `ImageData` (e.g., null, unsupported format if applicable).
    *   `NewCursor_Hotspot`: Verify hotspot functionality (might require visual check or specific SDL query if available).
    *   `NewCursor_WhenNotSupported`: Test behavior if `IsCursorSupported()` is `false`.

#### 12. [ ] `Night.Mouse.SetCursor()`
*   **Love2D Equivalent:** `love.mouse.setCursor(cursor)` or `love.mouse.setCursor()`
*   **Description:** Sets the current mouse cursor.
*   **C# Signature:** `public static void SetCursor(Night.Mouse.Cursor? cursor)`
*   **Requirements:**
    *   Set the active mouse cursor to the given `Cursor` object.
    *   If `cursor` is `null`, set the system's default arrow cursor.
*   **Acceptance Criteria:**
    *   Calling with a custom `Cursor` changes the mouse appearance.
    *   Calling with a `Cursor` obtained from `GetSystemCursor` changes to that system cursor.
    *   Calling with `null` resets to the default system arrow cursor.
    *   `GetCursor()` reflects the change (returns the custom cursor or `null`).
*   **Test Scenarios/Cases:**
    *   `SetCursor_Custom`: Set a custom cursor and verify (visual or via `GetCursor`).
    *   `SetCursor_System`: Set a system cursor and verify.
    *   `SetCursor_Null`: Set `null` cursor and verify (visual and `GetCursor` returns `null`).
    *   `SetCursor_InvalidCursorObject`: (Optional) Test with a disposed or invalid cursor object.

#### 13. [x] `Night.Mouse.SetGrabbed()`
*   **Love2D Equivalent:** `love.mouse.setGrabbed(grab)`
*   **Description:** Grabs the mouse and confines it to the window.
*   **C# Signature:** `public static void SetGrabbed(bool grabbed)`
*   **Requirements:**
    *   If `grabbed` is `true`, confine the mouse cursor to the window boundaries.
    *   If `grabbed` is `false`, release the mouse cursor.
*   **Acceptance Criteria:**
    *   When `true`, mouse cannot leave the window.
    *   When `false`, mouse can move freely.
    *   `IsGrabbed()` reflects the current state.
*   **Test Scenarios/Cases:**
    *   `SetGrabbed_Enable`: Enable grab, verify `IsGrabbed` is true, and (manually) test confinement.
    *   `SetGrabbed_Disable`: Disable grab, verify `IsGrabbed` is false, and (manually) test freedom.

#### 14. [ ] `Night.Mouse.SetPosition()`
*   **Love2D Equivalent:** `love.mouse.setPosition(x, y)`
*   **Description:** Sets the current position of the mouse.
*   **C# Signature:** `public static void SetPosition(float x, float y)`
*   **Requirements:**
    *   Move the mouse cursor to the specified `x` and `y` coordinates within the window.
    *   Coordinates are relative to the window's client area.
*   **Acceptance Criteria:**
    *   `GetPosition()` returns the new coordinates after calling this method.
    *   The visible mouse cursor moves to the specified position.
    *   Behavior if coordinates are outside window bounds should match SDL (e.g., clamped).
*   **Test Scenarios/Cases:**
    *   `SetPosition_InsideWindow`: Set position within bounds, verify with `GetPosition`.
    *   `SetPosition_OutsideWindow`: Set position outside bounds, verify behavior with `GetPosition`.
    *   `SetPosition_VerifyVisual`: (Manual) Visually confirm cursor movement.

#### 15. [x] `Night.Mouse.SetRelativeMode()`
*   **Love2D Equivalent:** `love.mouse.setRelativeMode(enable)`
*   **Description:** Sets whether relative mode is enabled for the mouse. In relative mode, the cursor is hidden, and mouse motion events report relative changes (dx, dy) rather than absolute positions. Useful for FPS controls.
*   **C# Signature:** `public static void SetRelativeMode(bool enable)`
*   **Requirements:**
    *   If `enable` is `true`, enable relative mouse mode. Cursor typically becomes hidden, and `MouseMoved` events provide delta movements.
    *   If `enable` is `false`, disable relative mouse mode. Cursor typically becomes visible, and `MouseMoved` events provide absolute positions.
*   **Acceptance Criteria:**
    *   `GetRelativeMode()` reflects the current state.
    *   When `true`, cursor is hidden (or behavior defined by SDL).
    *   When `true`, `MouseMoved` event arguments `dx`, `dy` report relative motion.
    *   When `false`, cursor visibility is restored (if previously hidden by relative mode).
*   **Test Scenarios/Cases:**
    *   `SetRelativeMode_Enable`: Enable, verify `GetRelativeMode` is true, check cursor visibility, check `MouseMoved` event args.
    *   `SetRelativeMode_Disable`: Disable, verify `GetRelativeMode` is false, check cursor visibility, check `MouseMoved` event args.

#### 16. [x] `Night.Mouse.SetVisible()`
*   **Love2D Equivalent:** `love.mouse.setVisible(visible)`
*   **Description:** Sets the current visibility of the cursor.
*   **C# Signature:** `public static void SetVisible(bool visible)`
*   **Requirements:**
    *   If `visible` is `true`, show the mouse cursor.
    *   If `visible` is `false`, hide the mouse cursor.
*   **Acceptance Criteria:**
    *   `IsVisible()` reflects the current state.
    *   The mouse cursor's visibility changes accordingly.
*   **Test Scenarios/Cases:**
    *   `SetVisible_False`: Hide cursor, verify `IsVisible` is false, (manual) check visual.
    *   `SetVisible_True`: Show cursor, verify `IsVisible` is true, (manual) check visual.
    *   `SetVisible_InteractionWithRelativeMode`: Test visibility changes when relative mode is active/inactive.

#### 17. [ ] `Night.Mouse.SetX()`
*   **Love2D Equivalent:** `love.mouse.setX(x)`
*   **Description:** Sets the current X position of the mouse, keeping Y the same.
*   **C# Signature:** `public static void SetX(float x)`
*   **Requirements:**
    *   Set the mouse cursor's x-position to `x`, while maintaining its current y-position.
*   **Acceptance Criteria:**
    *   `GetPosition().X` (or `GetX()`) returns the new `x` value.
    *   `GetPosition().Y` (or `GetY()`) remains unchanged.
    *   Visible cursor moves accordingly.
*   **Test Scenarios/Cases:**
    *   `SetX_VerifyXAndY`: Set X, then use `GetPosition` to verify new X and old Y.
    *   `SetX_VisualConfirmation`: (Manual) Visually confirm cursor movement.

#### 18. [ ] `Night.Mouse.SetY()`
*   **Love2D Equivalent:** `love.mouse.setY(y)`
*   **Description:** Sets the current Y position of the mouse, keeping X the same.
*   **C# Signature:** `public static void SetY(float y)`
*   **Requirements:**
    *   Set the mouse cursor's y-position to `y`, while maintaining its current x-position.
*   **Acceptance Criteria:**
    *   `GetPosition().Y` (or `GetY()`) returns the new `y` value.
    *   `GetPosition().X` (or `GetX()`) remains unchanged.
    *   Visible cursor moves accordingly.
*   **Test Scenarios/Cases:**
    *   `SetY_VerifyXAndY`: Set Y, then use `GetPosition` to verify new Y and old X.
    *   `SetY_VisualConfirmation`: (Manual) Visually confirm cursor movement.

---

### Callbacks (Events)

These events would likely be part of the `Night.IGame` interface or a global event subscription system within `Night.Framework`.

#### 1. [ ] `Night.Framework.MouseMoved` (Event)
*   **Love2D Equivalent:** `love.mousemoved(x, y, dx, dy, istouch)`
*   **Description:** Called when the mouse is moved.
*   **C# Delegate/Event Signature Idea (in `IGame` or similar):**
    ```csharp
    // In IGame interface:
    // void MouseMoved(float x, float y, float dx, float dy, bool isTouch);

    // Or as a static event in Night.Framework or Night.Mouse:
    // public static event Action<float, float, float, float, bool> MouseMoved;
    ```
    (Note: `isTouch` indicates if the event is from a touch input emulating a mouse. SDL provides this.)
*   **Requirements:**
    *   The event should be triggered whenever the mouse cursor moves.
    *   `x`, `y`: Absolute current position of the mouse.
    *   `dx`, `dy`: Change in position since the last frame/event. In relative mode, `x` and `y` might be deltas too, or `dx, dy` are the primary values. Clarify SDL behavior for relative mode.
    *   `isTouch`: Boolean indicating if the event originated from a touch device.
*   **Acceptance Criteria:**
    *   Event fires correctly upon mouse movement.
    *   Parameters `x, y, dx, dy, isTouch` provide accurate information.
    *   Behavior in normal mode vs. relative mode is correct for the parameters.
*   **Test Scenarios/Cases:**
    *   `MouseMoved_FiresOnMove`: Verify event fires when mouse is moved.
    *   `MouseMoved_CorrectArguments_AbsoluteMode`: Check `x, y, dx, dy` values in absolute mode.
    *   `MouseMoved_CorrectArguments_RelativeMode`: Check `x, y, dx, dy` values in relative mode (dx, dy should be key).
    *   `MouseMoved_IsTouchParameter`: Test with simulated touch input if possible.

#### 2. [ ] `Night.Framework.MouseWheelMoved` (Event)
*   **Love2D Equivalent:** `love.wheelmoved(x, y)` (Note: Love2D's x,y are dx, dy for wheel)
*   **Description:** Called when the mouse wheel is scrolled.
*   **C# Delegate/Event Signature Idea (in `IGame` or similar):**
    ```csharp
    // In IGame interface:
    // void MouseWheelMoved(float dx, float dy); // SDL provides float values for precise scrolling

    // Or as a static event:
    // public static event Action<float, float> MouseWheelMoved;
    ```
*   **Requirements:**
    *   The event should be triggered when the mouse wheel is scrolled.
    *   `dx`: Amount scrolled horizontally (positive for right, negative for left).
    *   `dy`: Amount scrolled vertically (positive for away from user/up, negative for towards user/down).
*   **Acceptance Criteria:**
    *   Event fires correctly upon mouse wheel movement.
    *   Parameters `dx, dy` provide accurate scroll direction and magnitude.
*   **Test Scenarios/Cases:**
    *   `MouseWheelMoved_FiresOnScroll`: Verify event fires.
    *   `MouseWheelMoved_VerticalScroll_Up`: Check `dy` is positive.
    *   `MouseWheelMoved_VerticalScroll_Down`: Check `dy` is negative.
    *   `MouseWheelMoved_HorizontalScroll_Left`: Check `dx` is negative (if mouse supports it).
    *   `MouseWheelMoved_HorizontalScroll_Right`: Check `dx` is positive (if mouse supports it).