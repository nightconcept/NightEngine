# Epic: Framework Environment Variable Refactor

**User Story:** As a developer, I want to simplify test environment detection in the Night Framework by relying solely on the `SDL_VIDEODRIVER=dummy` environment variable, removing the `isTestingEnvironment` and `isCI` checks.

**Assigned Task:**

- Remove `isTestingEnvironment` and `isCI` variables and their initialization from `src/Night/Framework.cs`.
- Remove the `IsTestingEnvironment()` and `IsCIEnvironment()` methods from `src/Night/Framework.cs`.
- Adjust any logic that uses these variables/methods to rely on the `SDL_VIDEODRIVER` check for headless/testing mode.

**Status:** Review

**Log:**

- 2025-06-09: Task initiated.
- 2025-06-09: Removed `isTestingEnvironment`, `isCI` variables and `IsTestingEnvironment()`, `IsCIEnvironment()` methods from `src/Night/Framework.cs`. Updated logic to rely on `SDL_VIDEODRIVER` environment variable.
