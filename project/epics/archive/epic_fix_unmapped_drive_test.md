# Epic: Fix `Filesystem.Write.Error.DirectoryNotFoundUnmappedDrive` Test Failures on macOS/Linux

**User Story:** As a developer, I want the `Filesystem.Write.Error.DirectoryNotFoundUnmappedDrive` test to pass consistently across all supported platforms (Windows, macOS, Linux), ensuring that the `Night.Filesystem.Write` method correctly handles attempts to write to paths on non-existent or unmapped drives.

**Status:** Review

**Date Created:** 2025-06-13
**Last Updated:** 2025-06-13

## Requirements

- The `Night.Filesystem.Write` method must reliably fail when attempting to write to a path representing an unmapped or non-existent drive on macOS and Linux.
- The failure should result in an appropriate exception (e.g., `System.IO.DirectoryNotFoundException` or a custom engine exception if specified by project guidelines).
- The `NightTest.Groups.Filesystem.FilesystemWrite_Error_DirectoryNotFoundUnmappedDriveTest.Run()` test case in `tests/Groups/Filesystem/WriteTests2.cs` must be updated to correctly assert this failure condition on macOS and Linux.
- The fix must not introduce regressions for this test on Windows or affect other filesystem operations.

## Acceptance Criteria

- The `Filesystem.Write.Error.DirectoryNotFoundUnmappedDrive` test passes on macOS.
- The `Filesystem.Write.Error.DirectoryNotFoundUnmappedDrive` test passes on Linux.
- The `Filesystem.Write.Error.DirectoryNotFoundUnmappedDrive` test continues to pass on Windows.
- Code changes adhere to `project/guidelines.md`.

## Tasks

1.  **Analyze Failure:**
    *   [X] Review test logs for macOS and Linux.
    *   [X] Examine `Night.Filesystem.Write` in `src/Night/Filesystem/Filesystem.Write.cs`.
    *   [X] Examine `FilesystemWrite_Error_DirectoryNotFoundUnmappedDriveTest.Run` in `tests/Groups/Filesystem/WriteTests2.cs`.
    *   [X] Understand how "unmapped drive" is simulated and why `FileStream` behavior differs.
2.  **Plan Solution:**
    *   [X] Define strategy for detecting "unmapped drive" paths on macOS/Linux.
    *   [X] Determine appropriate exception to be thrown.
    *   [X] Outline changes to `Filesystem.Write.cs`.
    *   [X] Outline changes to `WriteTests2.cs`.
3.  **Implement Changes:**
    *   [X] Modify `src/Night/Filesystem/Filesystem.Write.cs`.
    *   [X] Modify `tests/Groups/Filesystem/WriteTests2.cs` (Reviewed, no code changes needed).
4.  **Test:**
    *   [ ] (Simulate) Run tests locally on macOS/Linux if possible.
    *   [X] Prepare changes for CI validation.
5.  **Documentation & Review:**
    *   [X] Document changes and decisions in this epic file.
    *   [X] Ensure adherence to `project/guidelines.md` and `project/PRD.md`.
    *   [X] Update status to `Review`.

## Notes & Decisions

- 2025-06-13: Initial analysis suggests `FileStream` on Unix-like systems might not throw `DirectoryNotFoundException` for paths like "Z:/nonexistent". It might interpret "Z:" as a relative directory. The test expects a failure (Assert.False(success)), but the `Write` operation might be succeeding by creating a directory named "Z:" or similar, or failing with an unexpected exception.
- 2025-06-13: Iteration 1 of fix involved checking `Path.IsPathRooted` which was incorrect for "Z:\..." paths on Unix. Iteration 2 simplifies the check to directly identify "X:\" or "X:/" path prefixes on Unix and throw `DirectoryNotFoundException`.

## Dependencies & Blockers

- None identified yet.

## User Approvals

- 2025-06-13: User approved implementation plan.