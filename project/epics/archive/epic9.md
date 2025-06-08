# Epic 9: Simplify macOS Platform Message

**User Story:** When starting the night engine, on MacOS, I want the platform message to be more simple including the MacOS version and just the darwin version, not all this extra stuff.

**Current Message Example:**
`Platform: Darwin 24.4.0 Darwin Kernel Version 24.4.0: Fri Apr 11 18:32:43 PDT 2025; root:xnu-11417.101.15~117/RELEASE_ARM64_T8103 (Arm64)`

**Desired Message Format Example:**
`Platform: macOS <version> (Darwin <kernel_version>)`

**Status:** Review

**Tasks:**

- [x] **1. Review Project Documentation:**
  - [x] Read `project/PRD.md`
  - [x] Read `project/guidelines.md`
- [x] **2. Plan Implementation:**
  - [x] Define Problem
  - [x] Outline Solution
  - [x] List Implementation Steps
  - [x] Identify Risks/Challenges
- [x] **3. Locate Code:**
  - [x] Searched for the existing platform message generation logic. Found in `src/Night.Engine/FrameworkLoop.cs`.
- [x] **4. Implement Changes:**
  - [x] Modified C# code in `src/Night.Engine/FrameworkLoop.cs` to detect macOS.
  - [x] Added logic to retrieve macOS version using `sw_vers -productVersion`.
  - [x] Added logic to retrieve Darwin kernel version using `uname -r`.
  - [x] Formatted the new platform string: `$"Platform: macOS {macOSVersion} (Darwin {darwinVersion})"`
  - [x] Implemented error handling for version retrieval, falling back to `RuntimeInformation.OSDescription`.
- [ ] **5. Test (Manual):**
  - [ ] Build and run `Night.SampleGame` on macOS.
  - [ ] Verify the console output shows the simplified platform string: `Platform: macOS <version> (Darwin <kernel_version>)`.
  - [ ] Test error handling if possible (e.g., by temporarily making `sw_vers` inaccessible if feasible in a test environment, or by simulating an error in code to ensure fallback works).
- [x] **6. Update Story File:**
  - [x] Logged all significant actions, decisions, and outputs.
  - [x] Updated task statuses.
- [x] **7. Handoff for Review:**
  - [x] Set status to `Review`.
  - [x] Provided modified code and instructions for verification.

**Notes:**
Plan presented to user on 2025-05-28.
The plan involves:

- Problem: macOS platform string is too verbose.
- Solution: Retrieve macOS version (`sw_vers -productVersion`) and Darwin kernel version (`uname -r` or parse `SDL.GetPlatform()`) and format a simpler string.
- Steps: Locate code, detect macOS, retrieve versions, format string, update logic.
- Risks: Command availability, SDL string format changes, error handling.

Code changes implemented in `src/Night.Engine/FrameworkLoop.cs` to use `sw_vers` and `uname` on macOS for a simplified platform string. Fallback to `RuntimeInformation.OSDescription` is in place.
