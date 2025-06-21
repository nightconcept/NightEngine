# Epic: Implement Keyboard Module (Night.Keyboard)

**User Story:** As a game developer, I want a comprehensive keyboard input interface (`Night.Keyboard`), so I can effectively manage key states (pressed/released), scancodes, key symbols, text input, and keyboard properties (key repeat, screen keyboard support) within my game, similar to the capabilities offered by Love2D's `love.keyboard` module.

**Overall Requirements:**

*   Implement or complete the `Night.Keyboard` static class to provide an interface to the user's keyboard.
*   All public APIs should reside within the `Night.Keyboard` class or utilize existing related types within the `Night` namespace (e.g., `Night.KeyCode`, `Night.KeySymbol`).
*   The implementation should primarily use SDL3 functions via `SDL3-CS` bindings.
*   All functions and types must be documented with XML comments explaining their purpose, parameters, and return values, adhering to [`project/guidelines.md`](project/guidelines.md:1).
*   The module code will primarily reside in [`src/Night/Keyboard/Keyboard.cs`](src/Night/Keyboard/Keyboard.cs:1).
*   Existing enums `Night.KeyCode` (from [`src/Night/Keyboard/KeyCode.cs`](src/Night/Keyboard/KeyCode.cs:1)) and `Night.KeySymbol` (from [`src/Night/Keyboard/KeySymbol.cs`](src/Night/Keyboard/KeySymbol.cs:1)) will be used for scancodes and key symbols respectively.
*   Keyboard-related events (key pressed, key released, text input) should be integrated into the `Night.IGame` interface or a similar event handling mechanism (e.g., `IGame.KeyPressed` is already present; `IGame.KeyReleased` and `IGame.TextInput` will be added).
*   New callback methods added to `Night.IGame` must also have corresponding `virtual` empty implementations in the `Night.Game` base class ([`src/Night/Game.cs`](src/Night/Game.cs:1)) to allow developers to only override the callbacks they need.

**Overall Acceptance Criteria:**

*   The `Night.Keyboard` static class is available and provides all specified functionalities.
*   Developers can reliably check key states (by symbol and scancode), convert between keys and scancodes, manage text input state, and query keyboard properties.
*   The API is intuitive and follows C# best practices while mirroring Love2D's `love.keyboard` module structure where appropriate.
*   Automated tests for each function and callback exist within the `NightTest` framework (likely in `tests/Groups/Keyboard/`), verifying correct behavior under various conditions.
*   The module integrates seamlessly with the existing `Night.Framework`.

**Status:** To Do
**Assigned Agent:** AI Dev Agent
**Date Started:** 2025-06-16
**Date Completed:** TBD

**Implementation Notes & Log:**

*   2025-06-16: Task received. Epic drafted for `Night.Keyboard` module.
    *   `Night.Keyboard.IsDown(KeyCode key)` (physical scancode check) is already implemented in [`src/Night/Keyboard/Keyboard.cs`](src/Night/Keyboard/Keyboard.cs:45). This will be documented as `Night.Keyboard.IsScancodeDown()`.
    *   `Night.KeyCode` and `Night.KeySymbol` enums are defined in [`src/Night/Keyboard/KeyCode.cs`](src/Night/Keyboard/KeyCode.cs:1) and [`src/Night/Keyboard/KeySymbol.cs`](src/Night/Keyboard/KeySymbol.cs:1) respectively.
    *   `IGame.KeyPressed` event is implemented as per [`project/PRD.md`](project/PRD.md:35).

**Dependencies:**

*   Standard C# libraries.
*   `SDL3-CS` bindings for SDL3.
*   Existing `Night.Framework` project structure and conventions, including `Night.KeyCode` and `Night.KeySymbol`.
*   SDL3 native libraries (for keyboard input handling).

**Questions for User:**

*   None at this time.

---

## Detailed Module Breakdown

### Existing Types (Enums)

The `Night.Keyboard` module will utilize the following existing enums:

*   **`Night.KeyCode`**: Defined in [`src/Night/Keyboard/KeyCode.cs`](src/Night/Keyboard/KeyCode.cs:1). Represents physical key locations on the keyboard, equivalent to SDL Scancodes (`SDL.Scancode`) and Love2D's `Scancode` type.
*   **`Night.KeySymbol`**: Defined in [`src/Night/Keyboard/KeySymbol.cs`](src/Night/Keyboard/KeySymbol.cs:1). Represents the logical key meaning, equivalent to SDL Keycodes (`SDL.Keycode`) and Love2D's `KeyConstant` type.

---

### Functions

#### 1. [ ] `Night.Keyboard.GetKeyFromScancode()`
*   **Love2D Equivalent:** `love.keyboard.getKeyFromScancode(scancode)`
*   **Description:** Gets the key symbol corresponding to the given hardware scancode under the current keyboard layout.
*   **C# Signature:** `public static Night.KeySymbol GetKeyFromScancode(Night.KeyCode scancode)`
*   **Requirements:**
    *   Translate an `Night.KeyCode` (physical scancode) to an `Night.KeySymbol` (logical key).
    *   Utilize `SDL.GetKeyFromScancode()` and map the result.
*   **Acceptance Criteria:**
    *   Method returns the correct `Night.KeySymbol` for a given `Night.KeyCode`.
    *   Returns `KeySymbol.Unknown` if the scancode is invalid or cannot be mapped.
*   **Test Scenarios/Cases:**
    *   `GetKeyFromScancode_Valid`: Test with common scancodes (e.g., `KeyCode.A`) and verify correct `KeySymbol` (e.g., `KeySymbol.A`).
    *   `GetKeyFromScancode_Unknown`: Test with an invalid or unmapped scancode.
    *   `GetKeyFromScancode_LayoutChanges`: (Advanced) If possible, test if results change with keyboard layout (though this is hard to automate).

#### 2. [ ] `Night.Keyboard.GetScancodeFromKey()`
*   **Love2D Equivalent:** `love.keyboard.getScancodeFromKey(key)`
*   **Description:** Gets the hardware scancode corresponding to the given key symbol on the current keyboard layout.
*   **C# Signature:** `public static Night.KeyCode GetScancodeFromKey(Night.KeySymbol key)`
*   **Requirements:**
    *   Translate an `Night.KeySymbol` (logical key) to an `Night.KeyCode` (physical scancode).
    *   Utilize `SDL.GetScancodeFromKey()` and map the result.
*   **Acceptance Criteria:**
    *   Method returns the correct `Night.KeyCode` for a given `Night.KeySymbol`.
    *   Returns `KeyCode.Unknown` if the key symbol is invalid or has no corresponding scancode on the current layout.
*   **Test Scenarios/Cases:**
    *   `GetScancodeFromKey_Valid`: Test with common key symbols (e.g., `KeySymbol.A`) and verify correct `KeyCode` (e.g., `KeyCode.A`).
    *   `GetScancodeFromKey_Unknown`: Test with an invalid or unmapped key symbol.

#### 3. [ ] `Night.Keyboard.HasKeyRepeat()`
*   **Love2D Equivalent:** `love.keyboard.hasKeyRepeat()`
*   **Description:** Gets whether key repeat is enabled for `Night.Framework.KeyPressed` events.
*   **C# Signature:** `public static bool HasKeyRepeat()`
*   **Requirements:**
    *   Return the internal state flag that determines if `IGame.KeyPressed` events are dispatched for repeated key presses.
    *   This state is controlled by `Night.Keyboard.SetKeyRepeat()`.
*   **Acceptance Criteria:**
    *   Method returns `false` by default (or a sensible default defined by the framework).
    *   Method returns `true` after `SetKeyRepeat(true)` is called.
    *   Method returns `false` after `SetKeyRepeat(false)` is called.
*   **Test Scenarios/Cases:**
    *   `HasKeyRepeat_DefaultState`: Verify initial state.
    *   `HasKeyRepeat_AfterSetTrue`: Verify returns `true` after enabling.
    *   `HasKeyRepeat_AfterSetFalse`: Verify returns `false` after disabling.

#### 4. [ ] `Night.Keyboard.HasScreenKeyboard()`
*   **Love2D Equivalent:** `love.keyboard.hasScreenKeyboard()`
*   **Description:** Gets whether screen keyboard is supported by the system.
*   **C# Signature:** `public static bool HasScreenKeyboard()`
*   **Requirements:**
    *   Call `SDL.HasScreenKeyboardSupport()`.
*   **Acceptance Criteria:**
    *   Method returns `true` if SDL reports screen keyboard support, `false` otherwise.
*   **Test Scenarios/Cases:**
    *   `HasScreenKeyboard_ReturnsBool`: Verify the method returns a boolean value. (Actual value depends on test environment).

#### 5. [ ] `Night.Keyboard.HasTextInput()`
*   **Love2D Equivalent:** `love.keyboard.hasTextInput()`
*   **Description:** Gets whether text input events (`Night.Framework.TextInput`) are currently enabled.
*   **C# Signature:** `public static bool HasTextInput()`
*   **Requirements:**
    *   Return `true` if `SDL.TextInputActive()` is true, `false` otherwise.
*   **Acceptance Criteria:**
    *   Method accurately reflects the state of SDL text input.
    *   Returns `true` after `SetTextInput(true)` is successfully called.
    *   Returns `false` after `SetTextInput(false)` is called or by default.
*   **Test Scenarios/Cases:**
    *   `HasTextInput_DefaultIsFalse`: Verify returns `false` initially.
    *   `HasTextInput_AfterSetTrue`: Verify returns `true` after enabling.
    *   `HasTextInput_AfterSetFalse`: Verify returns `false` after disabling.

#### 6. [ ] `Night.Keyboard.IsDown()` (by KeySymbol)
*   **Love2D Equivalent:** `love.keyboard.isDown(key)` where `key` is a `KeyConstant`.
*   **Description:** Checks whether a certain logical key (represented by `KeySymbol`) is currently pressed.
*   **C# Signature:** `public static bool IsDown(Night.KeySymbol key)`
*   **Requirements:**
    *   Convert the `Night.KeySymbol` to its corresponding `Night.KeyCode` (scancode) using `GetScancodeFromKey()`.
    *   Check the state of this `Night.KeyCode` using the same mechanism as `IsScancodeDown()`.
    *   Handle cases where a `KeySymbol` might map to multiple scancodes or no scancode (though `GetScancodeFromKey` should return one primary one or `Unknown`).
*   **Acceptance Criteria:**
    *   Method returns `true` if the logical key corresponding to the `KeySymbol` is pressed.
    *   Method returns `false` if the key is not pressed or cannot be mapped.
*   **Test Scenarios/Cases:**
    *   `IsDown_Symbol_NotPressed`: Verify returns `false` for a key symbol when not pressed.
    *   `IsDown_Symbol_Pressed`: Press a key, verify `IsDown(correspondingKeySymbol)` returns `true`.
    *   `IsDown_Symbol_Unmapped`: Test with a `KeySymbol` that might not have a direct scancode on some layouts.

#### 7. [x] `Night.Keyboard.IsScancodeDown()`
*   **Love2D Equivalent:** `love.keyboard.isScancodeDown(scancode)`
*   **Description:** Checks whether the specified physical key (represented by `KeyCode`/`Scancode`) is pressed.
*   **C# Signature:** `public static bool IsScancodeDown(Night.KeyCode scancode)`
*   **Implementation Note:** This functionality is already implemented as `public static bool IsDown(Night.KeyCode key)` in [`src/Night/Keyboard/Keyboard.cs`](src/Night/Keyboard/Keyboard.cs:45). This epic entry serves to align naming and track it. Consider renaming the existing method or adding `IsScancodeDown` as an alias or the primary name.
*   **Requirements:**
    *   Return `true` if the specified `Night.KeyCode` is currently held down, `false` otherwise.
    *   Use `SDL.GetKeyboardState()` and check the state for the given scancode.
*   **Acceptance Criteria:**
    *   Method returns `true` only when the specified physical key is pressed.
    *   Method returns `false` when the key is not pressed.
*   **Test Scenarios/Cases:**
    *   `IsScancodeDown_NotPressed`: Verify returns `false` for a scancode when not pressed.
    *   `IsScancodeDown_Pressed`: Press a key, verify `IsScancodeDown(correspondingKeyCode)` returns `true`.
    *   `IsScancodeDown_InvalidScancode`: Test with an out-of-bounds or `KeyCode.Unknown`.

#### 8. [ ] `Night.Keyboard.SetKeyRepeat()`
*   **Love2D Equivalent:** `love.keyboard.setKeyRepeat(enable)`
*   **Description:** Enables or disables key repeat for `Night.Framework.KeyPressed` events. When enabled, `KeyPressed` will fire multiple times if a key is held down.
*   **C# Signature:** `public static void SetKeyRepeat(bool enable)`
*   **Requirements:**
    *   Set an internal framework flag that controls whether repeated SDL key down events trigger repeated `IGame.KeyPressed` callbacks.
    *   This does not directly call an SDL function to enable/disable OS-level key repeat, but rather controls Night's event dispatching behavior for repeats.
*   **Acceptance Criteria:**
    *   `HasKeyRepeat()` reflects the state set by this method.
    *   If enabled, holding a key results in multiple `IGame.KeyPressed` events with `isRepeat = true` (after the initial `isRepeat = false`).
    *   If disabled, holding a key results in only one `IGame.KeyPressed` event (`isRepeat = false`).
*   **Test Scenarios/Cases:**
    *   `SetKeyRepeat_EnableAndVerifyEvent`: Enable, hold key, verify multiple `KeyPressed` events.
    *   `SetKeyRepeat_DisableAndVerifyEvent`: Disable, hold key, verify single `KeyPressed` event.
    *   `SetKeyRepeat_ToggleState`: Verify `HasKeyRepeat()` updates correctly.

#### 9. [ ] `Night.Keyboard.SetTextInput()`
*   **Love2D Equivalent:** `love.keyboard.setTextInput(enable)` (also `love.keyboard.setTextInput(enable, x, y, w, h)` for screen keyboard rect)
*   **Description:** Enables or disables text input events (`Night.Framework.TextInput`).
*   **C# Signature:** `public static void SetTextInput(bool enable)`
*   **Requirements:**
    *   If `enable` is `true`, call `SDL.StartTextInput()`.
    *   If `enable` is `false`, call `SDL.StopTextInput()`.
    *   (Future consideration: overload with rectangle for `SDL.SetTextInputRect()`).
*   **Acceptance Criteria:**
    *   `HasTextInput()` reflects the state after calling this method.
    *   When enabled, `Night.Framework.TextInput` events are generated from user typing.
    *   When disabled, `Night.Framework.TextInput` events are not generated.
*   **Test Scenarios/Cases:**
    *   `SetTextInput_EnableAndVerifyEvent`: Enable, type text, verify `TextInput` events.
    *   `SetTextInput_DisableAndVerifyNoEvent`: Disable, type text, verify no `TextInput` events.
    *   `SetTextInput_ToggleState`: Verify `HasTextInput()` updates correctly.

---

### Callbacks (Events)

These events would be part of the `Night.IGame` interface or a global event subscription system within `Night.Framework`.
Corresponding `virtual` empty methods should be added to `Night.Game` for any new `IGame` callbacks.

#### 1. [x] `Night.Framework.KeyPressed` (Event)
*   **Love2D Equivalent:** `love.keypressed(key, scancode, isrepeat)`
*   **Description:** Called when a key is pressed.
*   **C# Delegate/Event Signature (in `IGame`):** `void KeyPressed(Night.KeySymbol key, Night.KeyCode scancode, bool isRepeat);`
*   **Implementation Note:** This event is already implemented as per [`project/PRD.md`](project/PRD.md:35).
*   **Requirements:**
    *   Triggered when a key is pressed down.
    *   `key`: The `Night.KeySymbol` (logical key) that was pressed.
    *   `scancode`: The `Night.KeyCode` (physical key) that was pressed.
    *   `isRepeat`: `true` if this is a key repeat event (key was already held down), `false` for the initial press. Behavior controlled by `SetKeyRepeat()`.
*   **Acceptance Criteria:**
    *   Event fires correctly upon key press.
    *   Parameters `key`, `scancode`, `isRepeat` provide accurate information.
    *   Repeat behavior respects `SetKeyRepeat()` setting.
*   **Test Scenarios/Cases:**
    *   `KeyPressed_FiresOnPress`: Verify event fires.
    *   `KeyPressed_CorrectArguments`: Check `key`, `scancode`, `isRepeat` values.
    *   `KeyPressed_RepeatBehavior`: Test with `SetKeyRepeat(true)` and `SetKeyRepeat(false)`.

#### 2. [ ] `Night.Framework.KeyReleased` (Event)
*   **Love2D Equivalent:** `love.keyreleased(key, scancode)`
*   **Description:** Called when a key is released.
*   **C# Delegate/Event Signature Idea (in `IGame`):** `void KeyReleased(Night.KeySymbol key, Night.KeyCode scancode);`
*   **Requirements:**
    *   Triggered when a key is released.
    *   `key`: The `Night.KeySymbol` (logical key) that was released.
    *   `scancode`: The `Night.KeyCode` (physical key) that was released.
*   **Acceptance Criteria:**
    *   Event fires correctly upon key release.
    *   Parameters `key`, `scancode` provide accurate information.
*   **Test Scenarios/Cases:**
    *   `KeyReleased_FiresOnRelease`: Verify event fires.
    *   `KeyReleased_CorrectArguments`: Check `key`, `scancode` values.
    *   `KeyReleased_AfterHeldKey`: Press, hold, then release a key and verify event.

#### 3. [ ] `Night.Framework.TextInput` (Event)
*   **Love2D Equivalent:** `love.textinput(text)`
*   **Description:** Called when text has been input by the user.
*   **C# Delegate/Event Signature Idea (in `IGame`):** `void TextInput(string text);`
*   **Requirements:**
    *   Triggered when `SDL.StartTextInput()` is active and the user inputs text.
    *   `text`: The UTF-8 string of text that was input.
*   **Acceptance Criteria:**
    *   Event fires correctly when text input is enabled and user types.
    *   Parameter `text` provides the correct input string.
    *   Does not fire if text input is disabled via `SetTextInput(false)`.
*   **Test Scenarios/Cases:**
    *   `TextInput_FiresOnInput`: Enable text input, type, verify event and text.
    *   `TextInput_UnicodeCharacters`: Test with various Unicode characters.
    *   `TextInput_Disabled`: Disable text input, type, verify no event.