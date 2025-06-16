# Epic: Implement System.OpenURL API

**User Story:** As a developer, I want to be able to open a URL using the system's default browser or file explorer so that I can direct users to web pages or local files.

**Task:** Implement the `system.openURL` API in `src/Night/System/System.cs`.

**Synopsis:**
`success = Night.System.OpenURL(url)`

**Arguments:**
- `string url`: The URL to open. Must be formatted as a proper URL.

**Returns:**
- `boolean success`: Whether the URL was opened successfully.

**Notes:**
- Passing `file://` scheme in Android 7.0 (Nougat) and later always results in failure. Prior to 11.2, this would crash LÖVE instead of returning false. (This note is from LÖVE, ensure it's relevant or adapted for Night).

**Acceptance Criteria:**
- A public static method `OpenURL(string url)` is added to the `Night.System` class in `src/Night/System/System.cs`.
- The method takes a string `url` as input.
- The method returns `true` if the URL was opened successfully, `false` otherwise.
- The method uses `SDL.OpenURL()` internally.
- The method includes XML documentation matching the LÖVE API specification.
- The implementation adheres to `project/guidelines.md` and `project/PRD.md`.

**Status:** Review
**Assigned:** Roo
**Log:**
- 2025-06-16: Task created.
- 2025-06-16: Status updated to In-Progress. Reviewed project/PRD.md and project/guidelines.md.
- 2025-06-16: Implemented `Night.System.OpenURL(string url)` in `src/Night/System/System.cs`. The method uses `SDL.OpenURL(url)` which directly returns a boolean indicating success. XML documentation added.
- 2025-06-16: Status updated to Review.