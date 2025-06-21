# Epic 13: Filesystem Module Testing

**User Story:** As a developer, I want comprehensive tests for the `Night.Filesystem` module to ensure its reliability and correctness, adhering to the project's testing guidelines.

**Status:** In-Progress

**Assigned To:** Roo

**Start Date:** 2025-06-06

**Tasks:**

- [x] **Task 1: Review Project Documentation**
  - [x] Review `project/PRD.md`
  - [x] Review `project/guidelines.md`
  - [x] Review `project/testing-guidelines.md`
- [x] **Task 2: Analyze Existing Filesystem Tests**
  - [x] Review `tests/Groups/Filesystem/FilesystemGroup.cs`
  - [x] Review `tests/Groups/Filesystem/LinesTests.cs`
- [x] **Task 3: Plan Filesystem Tests**
  - [x] Identify all public methods and properties in `src/Night/Filesystem/` classes.
  - [x] Define test cases for each method/property, covering:
    - Normal operation (happy path)
    - Edge cases
    - Error conditions (e.g., invalid input, file not found, permissions)
  - [x] Determine which tests can be automated and which might require manual verification (though aiming for full automation if possible for filesystem operations).
  - [x] Present implementation plan to the user.
- [x] **Task 4: Implement Filesystem Tests**
  - [x] Create new test case files under `tests/Groups/Filesystem/` as needed (`GetInfoTests.cs`, `ReadWriteTests.cs`, `DirectoryTests.cs`, `NightFileTests.cs`).
  - [x] Implement test case classes inheriting from `NightTest.Core.GameTestCase`.
  - [x] Implement `Load()`, `Update()`, and `Draw()` (if necessary) methods for each test case.
  - [x] Ensure proper setup and cleanup of test resources (e.g., temporary files/directories).
  - [x] Add new test methods to `FilesystemGroup.cs`.
- [ ] **Task 5: Run and Verify Tests**
  - [ ] Execute all new and existing filesystem tests.
  - [ ] Debug and fix any failing tests.
- [ ] **Task 6: Documentation and Final Review**
  - [ ] Ensure all test cases have clear `Name` and `Description` properties.
  - [ ] Ensure `Details` property provides meaningful information on test outcomes.
  - [ ] Update this epic file with progress and any notes.
  - [ ] Request user review.

**Notes:**

- Focus on testing the functionality of `Filesystem.cs` and `NightFile.cs`.
- The enums (`BufferMode.cs`, `FileMode.cs`, `FileType.cs`) and `FileSystemInfo.cs` (primarily a data class) might not require extensive behavioral tests themselves but will be tested implicitly through `Filesystem.cs` methods.
- **Blocker:** All newly created test files (`GetInfoTests.cs`, `ReadWriteTests.cs`, `DirectoryTests.cs`, `NightFileTests.cs`) and the updated `FilesystemGroup.cs` are experiencing compilation errors related to fundamental System types (e.g., `System.IO`, `System.String`, `System.Collections.Generic` not found) and missing attributes. This indicates a project-level configuration issue with the `tests` project that needs to be resolved before tests can be compiled and run.

**Dependencies:**

- Access to `project/guidelines.md`.

**User Approval for Dependencies:**
*(No new external dependencies anticipated for this task)*
