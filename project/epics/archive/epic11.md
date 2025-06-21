# Epic 11: Implement Graphics.GetBackgroundColor

**User Story:** As a game developer, I want to be able to retrieve the current background color set in the graphics module, so I can use this information in my game logic or for debugging purposes.

**Requirements:**

* Implement the API `love.graphics.getBackgroundColor()` as `Night.Graphics.GetBackgroundColor()`.
* The method should be located in `src/Night/Graphics/Graphics.State.cs`.
* It should take no arguments.
* It should return four numbers (float or double) representing the Red, Green, Blue, and Alpha components of the background color.
* These component values must be in the range of 0.0 to 1.0.

**Acceptance Criteria:**

* A public method `GetBackgroundColor()` exists in the `Night.Graphics` class.
* Calling `GetBackgroundColor()` returns the R, G, B, A components of the current background color, normalized to 0.0-1.0.
* If the background color was previously set (e.g., by `SetBackgroundColor`), `GetBackgroundColor()` returns those values.
* If `SetBackgroundColor` has not been called, a default background color (e.g., black: 0,0,0,1) is returned.
* The method is documented with XML comments explaining its purpose, arguments (none), and return values.
* Automated tests verify the default background color and the color after `Graphics.Clear()` is called.

**Status:** Review
**Assigned Agent:** AI Dev Agent
**Date Started:** 2025-06-13
**Date Completed:** 2025-06-13

**Implementation Notes & Log:**

* 2025-06-13: Task received. Initial plan formulated.
* 2025-06-13: Created `src/Night/Graphics/Graphics.State.cs` with `_backgroundColor` field and `GetBackgroundColor()` method.
* 2025-06-13: Modified `Graphics.Clear(Color color)` in `src/Night/Graphics/Graphics.cs` to update `_backgroundColor`.
* 2025-06-13: Added `partial` modifier to `Graphics` class in `src/Night/Graphics/Graphics.cs`.
* 2025-06-13: Created `tests/Groups/Graphics/GraphicsBackgroundColorTests.cs` with `GameTestCase` implementations:
  * `GraphicsGetBackgroundColor_DefaultTest`
  * `GraphicsGetBackgroundColor_AfterClearTest`
* 2025-06-13: Updated `tests/Groups/Graphics/GraphicsGroup.cs` to include a new `[Fact]` method for running the new background color tests.

**Dependencies:**

* None explicitly approved yet. Assumes standard C# libraries and existing project structure.

**Questions for User:**

* None at this time.
