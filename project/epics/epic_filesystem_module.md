# Epic: Implement Filesystem Module (Night.Filesystem)

**User Story:** As a game developer, I want a robust filesystem interface (`Night.Filesystem`), so I can manage game assets and user data (saves, configurations) in a way that is consistent across platforms and familiar to those with Love2D experience, while being adapted for the Night engine's C# environment.

**Overall Requirements:**

*   Implement the `Night.Filesystem` static class to provide an interface to the user's filesystem, mirroring Love2D's `love.filesystem` module, with necessary C# adaptations.
*   **Save Directory:**
    *   All file write operations (e.g., `Write`, `Append`, `CreateDirectory`, `NightFile:write`) MUST occur exclusively within the game's designated save directory.
    *   The save directory path will be structured as:
        *   Windows: `%APPDATA%\Night\[Identity]\`
        *   macOS: `~/Library/Application Support/Night/[Identity]/`
        *   Linux: `$XDG_DATA_HOME/night/[Identity]/` or `~/.local/share/night/[Identity]/`
    *   The game's identity is managed by `Night.Filesystem.SetIdentity()` and `Night.Filesystem.GetIdentity()`. The default identity is "NightDefault".
    *   The save directory should be automatically created if it doesn't exist when first needed (e.g., by `GetSaveDirectory()` or any write operation).
    *   This save directory logic and paths MUST be clearly documented.
*   **Source/Read Path:**
    *   Read operations (e.g., `Read`, `Lines`, `NightFile:read`) will first check the save directory, then the game's source directory.
    *   The "source directory" (since `.love` archives are not used) will typically be the application's base directory (e.g., where the executable resides) or a developer-configured assets root. `GetSource()` and `GetSourceBaseDirectory()` will reflect this.
*   **Path Handling:**
    *   All paths passed to `Night.Filesystem` functions (unless specified otherwise, like `Get*Directory()` calls) are relative to the save directory (for writes) or resolved against save then source (for reads).
*   **No `.love` Archive Specifics:** Functionality explicitly tied to `.love` archives (e.g., `IsFused()`, aspects of `Mount()` related to the archive itself) will be omitted or adapted.
*   All public APIs must reside within the `Night.Filesystem` class or related types within the `Night` namespace (e.g., `Night.File`, `Night.FileData`, `Night.FileMode`).
*   The implementation should primarily use standard .NET `System.IO` functionalities.
*   All functions and types must be documented with XML comments explaining their purpose, parameters, and return values, adhering to [`project/guidelines.md`](project/guidelines.md:1).
*   The module code will primarily reside in files within `src/Night/Filesystem/`.
*   Associated types like `NightFile` (for `File`), `FileData`, and `DroppedFile` will be defined appropriately.

**Overall Acceptance Criteria:**

*   The `Night.Filesystem` static class is available and provides all specified functionalities adapted from Love2D.
*   Developers can reliably read from source/save locations and write to/manage files and directories within the designated save location.
*   Save directory creation and path resolution (save-first, then source for reads) works correctly across supported platforms (Windows, macOS, Linux).
*   The API is intuitive, follows C# best practices, and mirrors Love2D's `love.filesystem` module structure where appropriate.
*   Automated tests for each function and type exist within the `NightTest` framework (likely in `tests/Groups/Filesystem/`), verifying correct behavior, especially around path resolution and save directory constraints.
*   The module integrates seamlessly with the existing `Night.Framework`.
*   Save directory paths and behavior are clearly documented.

**Status:** In-Progress
**Assigned Agent:** AI Dev Agent
**Date Started:** 2025-06-16
**Date Completed:** TBD

**Implementation Notes & Log:**

*   2025-06-16: Task received. Epic drafted for `Night.Filesystem` module.
    *   Reviewed existing files: [`BufferMode.cs`](src/Night/Filesystem/BufferMode.cs:1), [`FileMode.cs`](src/Night/Filesystem/FileMode.cs:1), [`FileSystemInfo.cs`](src/Night/Filesystem/FileSystemInfo.cs:1), [`FileType.cs`](src/Night/Filesystem/FileType.cs:1), [`Filesystem.Read.cs`](src/Night/Filesystem/Filesystem.Read.cs:1), [`Filesystem.Write.cs`](src/Night/Filesystem/Filesystem.Write.cs:1), [`Filesystem.cs`](src/Night/Filesystem/Filesystem.cs:1), [`NightFile.cs`](src/Night/Filesystem/NightFile.cs:1).
    *   Key focus areas: `SetIdentity`/`GetIdentity`, `GetSaveDirectory`, read/write path resolution logic, and ensuring all write operations are sandboxed to the save directory.
    *   Documentation of save paths is a priority.

**Dependencies:**

*   Standard C# libraries (primarily `System.IO`).
*   Existing `Night.Framework` project structure and conventions.
*   `Night.Log` for logging.

**Questions for User:**

*   For `love.filesystem.mount` and `unmount`: Given `.love` archives are ignored, should these functions be adapted for mounting arbitrary zip files or directories as asset sources (e.g., for DLC or modding), or should they be considered lower priority/out of scope for now?
*   For `love.filesystem.load` (which loads but doesn't run Lua files): What is the desired C# equivalent? Should it simply read a file's content as a string, or is there an expectation for loading C# scripts or other structured data (which might be beyond a direct `love.filesystem` port)?
*   For `love.filesystem.getRequirePath` and `getCRequirePath`: These are specific to Lua's `require` system. Should they be omitted, or is there a C# analogue we should consider (e.g., paths for dynamic assembly loading, though this seems outside the scope of `love.filesystem`)?

---

## Detailed Module Breakdown

### Types

#### 1. [x] `Night.DroppedFile`
*   **Love2D Equivalent:** `love.filesystem.DroppedFile` (Added since 0.10.0)
*   **Description:** Represents a file dropped onto the window. (This implies integration with `Night.Window` or `Night.Framework` event system for file drop events).
*   **C# Definition Idea:**
    ```csharp
    namespace Night
    {
        public class DroppedFile // Potentially inherits from NightFile or shares common base
        {
            public string Path { get; } // Absolute path of the dropped file
            // Constructor internal to Night.Framework, populated by file drop event
            internal DroppedFile(string path);

            // May include methods from NightFile if it's to be treated like a readable file directly
            // e.g., Open(), Read(), GetSize(), etc.
            // Or, it might just be a data object and users use Filesystem.NewFile(droppedFile.Path)
        }
    }
    ```
*   **Requirements:**
    *   [x] Define a `DroppedFile` class.
    *   [x] Store the absolute path of the dropped file.
    *   [x] Integrate with a file drop event from the windowing system.
*   **Acceptance Criteria:**
    *   [x] `DroppedFile` objects are correctly created when files are dropped on the game window.
    *   [x] The `Path` property provides the correct absolute path to the dropped file.
*   **Test Scenarios/Cases:**
    *   `DroppedFile_PathCorrectness`: Check `Path` property for various dropped files. (Automated test created)
    *   **Manual Test:** `DroppedFile_EventFires`:
        *   **Setup:** Run the `SampleGame` or a dedicated test application.
        *   **Action:** Drag and drop a file from the host OS onto the game window.
        *   **Expected Result:** The application's `FileDropped` callback should be triggered, and the received `DroppedFile` object should contain the correct absolute path of the dropped file, which should be logged or displayed on screen for verification.

#### 2. [~] `Night.File` (Implemented as `NightFile.cs`)
*   **Love2D Equivalent:** `love.filesystem.File`
*   **Description:** Represents a file on the filesystem, opened for reading or writing.
*   **C# Definition:** [`src/Night/Filesystem/NightFile.cs`](src/Night/Filesystem/NightFile.cs:1)
*   **Review & Enhancement Requirements:**
    *   Ensure `NightFile` instances are created via `Night.Filesystem.NewFile()`.
    *   The `filename` passed to `NightFile` constructor should be the fully resolved path (either in save dir or source dir).
    *   Implement missing methods from `love.filesystem.File`:
        *   `[ ] public (bool Success, string? Error) Flush()` (Currently, flush is only called internally on close)
        *   `[ ] public (BufferMode? Mode, long? Size, string? Error) GetBuffer()`
        *   `[ ] public IEnumerable<string> Lines()` (iterator for lines from current position)
        *   `[ ] public Night.FileMode? GetMode()`
        *   `[ ] public (long? Size, string? Error) GetSize()`
        *   `[ ] public bool IsEOF()`
        *   `[ ] public (long? Position, string? Error) Seek(long offset, SeekOrigin origin = SeekOrigin.Begin)`
        *   `[ ] public (bool Success, string? Error) SetBuffer(BufferMode mode, long size = 0)` (May be complex or simplified for FileStream)
        *   `[ ] public (long? Position, string? Error) Tell()`
        *   `[ ] public (bool Success, string? Error) Write(string data, long? size = null)`
        *   `[ ] public (bool Success, string? Error) Write(byte[] data, long? size = null)`
    *   Existing methods like `Open`, `Read`, `ReadBytes`, `Close` should be verified against Love2D behavior, especially regarding path resolution handled by `Filesystem.NewFile`.
*   **Acceptance Criteria:**
    *   `NightFile` provides all functionalities of `love.filesystem.File` as adapted for C#.
    *   File operations (read, write, seek, etc.) work correctly based on the mode the file was opened in.
    *   Error handling is robust.
*   **Test Scenarios/Cases:** (For new/enhanced methods)
    *   `NightFile_Flush`: Verify data is written after flush.
    *   `NightFile_GetSetBuffer`: Test buffer mode changes if implemented.
    *   `NightFile_LinesIterator`: Verify iteration over file lines.
    *   `NightFile_GetMode`: Check correct mode is returned.
    *   `NightFile_GetSize`: Verify correct file size.
    *   `NightFile_IsEOF`: Test at end of file and before.
    *   `NightFile_SeekAndTell`: Test seeking to various positions and `Tell` reporting correctly.
    *   `NightFile_WriteData`: Test writing strings and bytes.

#### 3. [ ] `Night.FileData`
*   **Love2D Equivalent:** `love.filesystem.FileData`
*   **Description:** Data representing the contents of a file, typically loaded from disk or created from a string/byte array in memory.
*   **C# Definition Idea:**
    ```csharp
    namespace Night
    {
        public class FileData // : IDisposable if it holds unmanaged resources, though likely just byte[]/string
        {
            private readonly byte[] _data;
            private readonly string _filenameHint; // Original filename, for extension etc.

            public FileData(byte[] data, string filenameHint = "data")
            {
                _data = data ?? throw new ArgumentNullException(nameof(data));
                _filenameHint = filenameHint;
            }

            public FileData(string content, string filenameHint = "data.txt")
            {
                _data = System.Text.Encoding.UTF8.GetBytes(content ?? throw new ArgumentNullException(nameof(content)));
                _filenameHint = filenameHint;
            }

            public byte[] GetBytes() => (byte[])_data.Clone(); // Return a copy
            public string GetString() => System.Text.Encoding.UTF8.GetString(_data);
            public long GetSize() => _data.Length;
            public string GetFilenameHint() => _filenameHint;
            // public string GetExtension() => Path.GetExtension(_filenameHint); // Example utility
        }
    }
    ```
*   **Requirements:**
    *   Define a `FileData` class.
    *   Allow creation from byte array or string.
    *   Provide methods to get content as bytes or string, get size.
    *   Store a "filename hint" for context (e.g., for `love.image.newImageData(filedata)`).
*   **Acceptance Criteria:**
    *   `FileData` can be created from raw bytes or string content.
    *   `GetBytes()`, `GetString()`, `GetSize()` return correct information.
*   **Test Scenarios/Cases:**
    *   `FileData_CreateFromBytes`: Verify content and size.
    *   `FileData_CreateFromString`: Verify content and size.
    *   `FileData_FilenameHint`: Check hint is stored and retrievable.

### Enums

#### 1. [x] `Night.BufferMode`
*   **Love2D Equivalent:** `File.setBuffer` modes (none, line, full)
*   **C# Definition:** [`src/Night/Filesystem/BufferMode.cs`](src/Night/Filesystem/BufferMode.cs:1)
*   **Status:** Exists. Matches Love2D.

#### 2. [~] `Night.FileMode`
*   **Love2D Equivalent:** `love.filesystem.FileMode` (r, w, a, c) and `File:open` modes (r, w, a, rb, wb, ab)
*   **C# Definition:** [`src/Night/Filesystem/FileMode.cs`](src/Night/Filesystem/FileMode.cs:1) (Read, Write, Append)
*   **Review & Enhancement Requirements:**
    *   Love2D `FileMode` enum itself has `read`, `write`, `append`, `closed`. Our `Night.FileMode` is for opening.
    *   Love2D `File:open` takes "r", "w", "a". The 'b' (binary) modifier is less relevant in C# stream handling but `NightFile.Open(string modeString)` handles "rb", "wb", "ab".
    *   Consider if a `Closed` state is needed in the enum if `NightFile.GetMode()` is to return it, or if `GetMode()` returns null when closed.
*   **Status:** Exists. Largely sufficient for opening files. `NightFile.Open(string)` handles Love2D-style mode strings.

#### 3. [x] `Night.FileType`
*   **Love2D Equivalent:** `love.filesystem.FileType` (file, directory, symlink, other, unknown)
*   **C# Definition:** [`src/Night/Filesystem/FileType.cs`](src/Night/Filesystem/FileType.cs:1) (File, Directory, Symlink, Other, None)
*   **Status:** Exists. Matches Love2D closely ("None" for "unknown").

---

### Functions (`Night.Filesystem` static class)

#### 1. [x] `Night.Filesystem.Append(string filepath, byte[] data, long? size = null)`
*   **Love2D Equivalent:** `love.filesystem.append(filepath, data, size)`
*   **C# Implementation:** In [`src/Night/Filesystem/Filesystem.cs`](src/Night/Filesystem/Filesystem.cs:196) (byte[] overload)
*   **Enhancement:**
    *   Overload `Append(string filepath, string data, long? size = null)` should exist (it does in Love2D).
    *   Ensure `filepath` is resolved relative to the **save directory**. The directory should be created if it doesn't exist.
*   **Status:** Partially exists. String overload and path resolution needed.

#### 2. [ ] `Night.Filesystem.Append(string filepath, string data, long? size = null)`
*   **Love2D Equivalent:** `love.filesystem.append(filepath, data, size)`
*   **C# Signature Idea:** `public static (bool Success, string? ErrorMessage) Append(string filepath, string data, long? size = null)`
*   **Requirements:**
    *   Append string data (UTF-8 encoded) to a file.
    *   Filepath is relative to the **save directory**.
    *   Create file/directory if it doesn't exist within the save directory.
*   **Acceptance Criteria:**
    *   Data is correctly appended. Path resolved to save directory.
*   **Test Scenarios/Cases:**
    *   `Append_String_NewFile`: Appending to a non-existent file in save dir.
    *   `Append_String_ExistingFile`: Appending to an existing file in save dir.
    *   `Append_String_WithPath`: Appending to a file in a subdirectory of save dir.

#### 3. [ ] `Night.Filesystem.AreSymlinksEnabled()`
*   **Love2D Equivalent:** `love.filesystem.areSymlinksEnabled()`
*   **C# Signature Idea:** `public static bool AreSymlinksEnabled()`
*   **Requirements:**
    *   Return a boolean indicating if symlink following is enabled for filesystem operations.
    *   This would likely be a static flag in `Night.Filesystem`.
*   **Acceptance Criteria:**
    *   Returns the current state of symlink following.
*   **Test Scenarios/Cases:**
    *   `AreSymlinksEnabled_Default`: Check default state.
    *   `AreSymlinksEnabled_AfterSet`: Check after calling `SetSymlinksEnabled`.

#### 4. [x] `Night.Filesystem.CreateDirectory(string path)`
*   **Love2D Equivalent:** `love.filesystem.createDirectory(path)`
*   **C# Implementation:** In [`src/Night/Filesystem/Filesystem.cs`](src/Night/Filesystem/Filesystem.cs:251)
*   **Enhancement:**
    *   Ensure `path` is resolved relative to the **save directory**.
*   **Status:** Exists. Path resolution to save directory needs verification/implementation.

#### 5. [x] `Night.Filesystem.GetAppdataDirectory()`
*   **Love2D Equivalent:** `love.filesystem.getAppdataDirectory()`
*   **C# Implementation:** In [`src/Night/Filesystem/Filesystem.cs`](src/Night/Filesystem/Filesystem.cs:285)
*   **Review:** Current implementation uses `gameIdentity` directly under OS-specific appdata paths (e.g., `%APPDATA%\NightDefault`). This is good. Love2D docs say "could be the same as getUserDirectory". Our implementation is specific to the application.
*   **Status:** Exists. Seems to align with the need for an application-specific writable directory.

#### 6. [ ] `Night.Filesystem.GetCRequirePath()`
*   **Love2D Equivalent:** `love.filesystem.getCRequirePath()`
*   **C# Signature Idea:** `public static string GetCRequirePath()` (or `IEnumerable<string>`)
*   **Requirements:**
    *   Love2D: "Gets the filesystem paths that will be searched for C libraries when require is called."
    *   Night: This is Lua-specific. Consider if there's a C# equivalent (e.g., paths for `AssemblyLoadContext` or native library probing paths) or if it should be omitted.
*   **Acceptance Criteria:** TBD based on decision.
*   **Test Scenarios/Cases:** TBD.
*   **Note:** Likely low priority or out of scope unless a clear C# mapping is defined.

#### 7. [ ] `Night.Filesystem.GetDirectoryItems(string path)`
*   **Love2D Equivalent:** `love.filesystem.getDirectoryItems(path)`
*   **C# Signature Idea:** `public static IEnumerable<string> GetDirectoryItems(string path)`
*   **Requirements:**
    *   Return a list of all files and subdirectories in the given `path`.
    *   `path` is resolved by checking the save directory first, then the source directory.
*   **Acceptance Criteria:**
    *   Correctly lists items from save or source directory based on path resolution.
    *   Returns relative paths from the `path` argument.
*   **Test Scenarios/Cases:**
    *   `GetDirectoryItems_SaveDir`: List items in a save directory path.
    *   `GetDirectoryItems_SourceDir`: List items in a source directory path.
    *   `GetDirectoryItems_Empty`: Test on an empty directory.
    *   `GetDirectoryItems_NotFound`: Test on a non-existent path.

#### 8. [ ] `Night.Filesystem.GetIdentity()`
*   **Love2D Equivalent:** `love.filesystem.getIdentity()`
*   **C# Signature Idea:** `public static string GetIdentity()`
*   **Requirements:**
    *   Return the current game identity string.
    *   Default should be "NightDefault".
*   **Acceptance Criteria:**
    *   Returns the identity set by `SetIdentity()` or the default.
*   **Test Scenarios/Cases:**
    *   `GetIdentity_Default`: Check returns "NightDefault".
    *   `GetIdentity_AfterSet`: Check after calling `SetIdentity`.

#### 9. [x] `Night.Filesystem.GetInfo(string path, FileType? filterType = null)`
*   **Love2D Equivalent:** `love.filesystem.getInfo(path, filtertype)`
*   **C# Implementation:** In [`src/Night/Filesystem/Filesystem.cs`](src/Night/Filesystem/Filesystem.cs:57)
*   **Enhancement:**
    *   `path` needs to be resolved: check save directory first, then source directory.
*   **Status:** Exists. Path resolution needs implementation. Overloads for populating existing `FileSystemInfo` also exist.

#### 10. [ ] `Night.Filesystem.GetRealDirectory(string filepath)`
*   **Love2D Equivalent:** `love.filesystem.getRealDirectory(filepath)`
*   **C# Signature Idea:** `public static string? GetRealDirectory(string filepath)`
*   **Requirements:**
    *   Returns the real, absolute path of the directory containing the given `filepath`.
    *   `filepath` is resolved (save then source). The function then returns the absolute path to the *directory* where this resolved file resides.
*   **Acceptance Criteria:**
    *   Returns correct absolute directory path for files in save or source.
    *   Returns `null` if filepath is not found.
*   **Test Scenarios/Cases:**
    *   `GetRealDirectory_SaveFile`: Test with a file in the save directory.
    *   `GetRealDirectory_SourceFile`: Test with a file in the source directory.

#### 11. [ ] `Night.Filesystem.GetRequirePath()`
*   **Love2D Equivalent:** `love.filesystem.getRequirePath()`
*   **C# Signature Idea:** `public static string GetRequirePath()` (or `IEnumerable<string>`)
*   **Requirements:**
    *   Love2D: "Gets the filesystem paths that will be searched when require is called."
    *   Night: Lua-specific. See `GetCRequirePath`.
*   **Acceptance Criteria:** TBD.
*   **Test Scenarios/Cases:** TBD.
*   **Note:** Likely low priority or out of scope.

#### 12. [ ] `Night.Filesystem.GetSaveDirectory()`
*   **Love2D Equivalent:** `love.filesystem.getSaveDirectory()`
*   **C# Signature Idea:** `public static string GetSaveDirectory()`
*   **Requirements:**
    *   Return the full, absolute path to the game's save directory.
    *   Path construction based on OS and `gameIdentity` (e.g., `%APPDATA%\Night\[Identity]\`).
    *   The directory should be created if it doesn't exist.
    *   This function is critical and its behavior (paths, creation) must be documented.
*   **Acceptance Criteria:**
    *   Returns correct, platform-specific absolute path to the save directory.
    *   Directory is created if it doesn't exist.
*   **Test Scenarios/Cases:**
    *   `GetSaveDirectory_Windows`: Verify path on Windows.
    *   `GetSaveDirectory_MacOS`: Verify path on macOS.
    *   `GetSaveDirectory_Linux`: Verify path on Linux.
    *   `GetSaveDirectory_CreatesDir`: Verify directory creation.
    *   `GetSaveDirectory_WithCustomIdentity`: Verify path with non-default identity.

#### 13. [ ] `Night.Filesystem.GetSource()`
*   **Love2D Equivalent:** `love.filesystem.getSource()`
*   **C# Signature Idea:** `public static string GetSource()`
*   **Requirements:**
    *   Love2D: "Returns the full path to the .love file or directory."
    *   Night: Since no `.love` files, this should return the full path to the game's source/assets directory. This could be `AppContext.BaseDirectory` by default, or configurable.
*   **Acceptance Criteria:**
    *   Returns the correct absolute path to the defined source directory.
*   **Test Scenarios/Cases:**
    *   `GetSource_DefaultPath`: Verify default source path.

#### 14. [ ] `Night.Filesystem.GetSourceBaseDirectory()`
*   **Love2D Equivalent:** `love.filesystem.getSourceBaseDirectory()`
*   **C# Signature Idea:** `public static string GetSourceBaseDirectory()`
*   **Requirements:**
    *   Love2D: "Returns the full path to the directory containing the .love file."
    *   Night: Should return the parent directory of what `GetSource()` returns.
*   **Acceptance Criteria:**
    *   Returns the correct absolute path to the parent of the source directory.
*   **Test Scenarios/Cases:**
    *   `GetSourceBaseDirectory_Path`: Verify correct parent path.

#### 15. [ ] `Night.Filesystem.GetUserDirectory()`
*   **Love2D Equivalent:** `love.filesystem.getUserDirectory()`
*   **C# Signature Idea:** `public static string GetUserDirectory()`
*   **Requirements:**
    *   Return the path to the current user's home directory (e.g., `Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)`).
*   **Acceptance Criteria:**
    *   Returns the correct user home directory path.
*   **Test Scenarios/Cases:**
    *   `GetUserDirectory_Path`: Verify path on different OSes.

#### 16. [ ] `Night.Filesystem.GetWorkingDirectory()`
*   **Love2D Equivalent:** `love.filesystem.getWorkingDirectory()`
*   **C# Signature Idea:** `public static string GetWorkingDirectory()`
*   **Requirements:**
    *   Return the current working directory of the application (`Directory.GetCurrentDirectory()`).
*   **Acceptance Criteria:**
    *   Returns the correct CWD.
*   **Test Scenarios/Cases:**
    *   `GetWorkingDirectory_Path`: Verify CWD.

#### 17. [ ] `Night.Filesystem.Init()`
*   **Love2D Equivalent:** `love.filesystem.init()`
*   **Description:** "Initializes love.filesystem, will be called internally, so should not be used explicitly."
*   **Night:** This can be an internal static constructor or an explicit internal `Initialize()` method for `Night.Filesystem` if needed (e.g., to set up default identity, ensure base Night directory exists). Not part of the public API.
*   **Status:** Internal, no plan needed for public API.

#### 18. [ ] `Night.Filesystem.IsFused()`
*   **Love2D Equivalent:** `love.filesystem.isFused()`
*   **C# Signature Idea:** `public static bool IsFused()`
*   **Requirements:**
    *   Love2D: "Gets whether the game is in fused mode or not." (Fused mode means game and engine are one executable, relevant for `.love` files).
    *   Night: Since no `.love` files, this should likely always return `false`, or a value indicating it's not a concept that applies in the same way.
*   **Acceptance Criteria:**
    *   Consistently returns `false` (or appropriate value).
*   **Test Scenarios/Cases:**
    *   `IsFused_ReturnsFalse`: Verify it returns false.

#### 19. [x] `Night.Filesystem.Lines(string filepath)`
*   **Love2D Equivalent:** `love.filesystem.lines(filepath)`
*   **C# Implementation:** In [`src/Night/Filesystem/Filesystem.Read.cs`](src/Night/Filesystem/Filesystem.Read.cs:38)
*   **Enhancement:**
    *   `filepath` needs to be resolved: check save directory first, then source directory. Current implementation uses `File.ReadLines(filePath)` directly.
*   **Status:** Exists. Path resolution needs implementation.

#### 20. [ ] `Night.Filesystem.Load(string filepath)`
*   **Love2D Equivalent:** `love.filesystem.load(filepath)`
*   **C# Signature Idea:** `public static (string? Content, string? ErrorMessage) Load(string filepath)`
*   **Requirements:**
    *   Love2D: "Loads a Lua file (but does not run it)." Returns a function or throws error.
    *   Night: Could return the file content as a string. `filepath` resolved (save then source).
    *   See "Questions for User". For now, assume it reads content as string.
*   **Acceptance Criteria:**
    *   Returns file content as string if successful, or error.
*   **Test Scenarios/Cases:**
    *   `Load_FileContent`: Verify content of a loaded file.
    *   `Load_NotFound`: Test error for non-existent file.

#### 21. [ ] `Night.Filesystem.Mount(string archivePath, string mountPoint, bool appendToPath = false)`
*   **Love2D Equivalent:** `love.filesystem.mount(archive, mountpoint, appendToPath)`
*   **C# Signature Idea:** `public static bool Mount(string archivePath, string mountPoint, bool appendToPath = false)`
*   **Requirements:**
    *   Love2D: "Mounts a zip file or folder in the game's save directory for reading."
    *   Night: See "Questions for User". If implemented, `archivePath` could be an absolute path to a zip/folder. `mountPoint` is a virtual path. Read operations would then check these mounted sources. This is a complex feature.
*   **Acceptance Criteria:** TBD.
*   **Test Scenarios/Cases:** TBD.
*   **Note:** Potentially complex and lower priority.

#### 22. [x] `Night.Filesystem.NewFile(string filename)`
*   **Love2D Equivalent:** `love.filesystem.newFile(filename)`
*   **C# Implementation:** In [`src/Night/Filesystem/Filesystem.cs`](src/Night/Filesystem/Filesystem.cs:336) (returns `NightFile`)
*   **Enhancement:**
    *   This is the crucial point for path resolution for `NightFile` objects.
    *   When `NewFile(filename)` is called, `filename` is relative.
    *   The actual path used to construct `NightFile` needs to be determined here.
    *   If `NightFile.Open()` is subsequently called with Read mode, it should have tried save then source.
    *   If `NightFile.Open()` is called with Write/Append mode, it must be in the save directory.
    *   This implies `NewFile` might not resolve immediately, but `NightFile.Open` does the final resolution based on mode. Or, `NewFile(filename, mode)` resolves upfront.
    *   The current `NewFile(filename, mode)` in `Filesystem.cs` directly passes `filename` to `NightFile` constructor, then calls `file.Open(mode)`. `NightFile.Open` uses `new FileStream(this.filename, ...)`. This means `this.filename` in `NightFile` must be the *final, absolute path*.
    *   **Revised Logic for `Filesystem.NewFile(string relativePath, FileMode mode)`:**
        1.  If mode is Read:
            *   Try `Path.Combine(GetSaveDirectory(), relativePath)`. If exists, use this absolute path.
            *   Else, try `Path.Combine(GetSource(), relativePath)`. If exists, use this absolute path.
            *   Else, error or use the source path for potential creation by `FileStream` if `FileMode.Open` allows (it doesn't, `OpenOrCreate` would). Love2D `File:open("r")` fails if not found.
        2.  If mode is Write or Append:
            *   Use `Path.Combine(GetSaveDirectory(), relativePath)`. Ensure save directory (and subdirs in `relativePath`) are created.
        3.  Construct `NightFile` with this resolved absolute path.
*   **Status:** Exists. Path resolution logic within `NewFile` or `NightFile.Open` needs significant work.

#### 23. [ ] `Night.Filesystem.NewFileData(byte[] data, string name)`
*   **Love2D Equivalent:** `love.filesystem.newFileData(string, name)` or `love.filesystem.newFileData(data, name)`
*   **C# Signature Idea:**
    *   `public static FileData NewFileData(byte[] data, string name)`
    *   `public static FileData NewFileData(string content, string name)`
*   **Requirements:**
    *   Create a `FileData` object from raw bytes or string.
    *   `name` is used as the filename hint for the `FileData`.
*   **Acceptance Criteria:**
    *   Correctly creates `FileData` instances.
*   **Test Scenarios/Cases:**
    *   `NewFileData_FromBytes`: Verify.
    *   `NewFileData_FromString`: Verify.

#### 24. [x] `Night.Filesystem.Read(string filepath, long? sizeToRead = null)`
*   **Love2D Equivalent:** `love.filesystem.read(filepath, size)`
*   **C# Implementation:** In [`src/Night/Filesystem/Filesystem.Read.cs`](src/Night/Filesystem/Filesystem.Read.cs:80) (returns string)
*   **Enhancement:**
    *   `filepath` needs to be resolved: check save directory first, then source directory.
*   **Status:** Exists. Path resolution needs implementation.

#### 25. [x] `Night.Filesystem.Read(ContainerType container, string filepath, long? sizeToRead = null)`
*   **Love2D Equivalent:** `love.filesystem.read(container, filepath, size)`
*   **C# Implementation:** In [`src/Night/Filesystem/Filesystem.Read.cs`](src/Night/Filesystem/Filesystem.Read.cs:112) (returns object)
*   **Enhancement:**
    *   `filepath` needs to be resolved: check save directory first, then source directory.
*   **Status:** Exists. Path resolution needs implementation. [`ContainerType`](src/Night/Filesystem/Filesystem.cs:43) enum also exists.

#### 26. [ ] `Night.Filesystem.Remove(string filepath)`
*   **Love2D Equivalent:** `love.filesystem.remove(filepath)`
*   **C# Signature Idea:** `public static bool Remove(string filepath)`
*   **Requirements:**
    *   Removes a file or an empty directory.
    *   `filepath` is relative to the **save directory**. Operations outside save directory are forbidden.
*   **Acceptance Criteria:**
    *   Successfully removes file/directory from save location.
    *   Fails to remove items outside save directory or non-empty directories.
    *   Returns true on success, false on failure.
*   **Test Scenarios/Cases:**
    *   `Remove_FileInSaveDir`: Test removing a file.
    *   `Remove_EmptyDirInSaveDir`: Test removing an empty directory.
    *   `Remove_NonEmptyDir`: Verify fails.
    *   `Remove_OutsideSaveDir`: Verify fails.
    *   `Remove_NotFound`: Verify behavior for non-existent path.

#### 27. [ ] `Night.Filesystem.SetCRequirePath(string path)`
*   **Love2D Equivalent:** `love.filesystem.setCRequirePath(path)`
*   **C# Signature Idea:** `public static void SetCRequirePath(string path)`
*   **Requirements:** Lua-specific. See `GetCRequirePath`.
*   **Acceptance Criteria:** TBD.
*   **Test Scenarios/Cases:** TBD.
*   **Note:** Likely low priority or out of scope.

#### 28. [ ] `Night.Filesystem.SetIdentity(string identityName)`
*   **Love2D Equivalent:** `love.filesystem.setIdentity(name)`
*   **C# Signature Idea:** `public static void SetIdentity(string identityName)`
*   **Requirements:**
    *   Sets the game's identity, used for the save directory.
    *   `identityName` should be sanitized (e.g., remove invalid path characters).
    *   If `identityName` is null or empty, perhaps revert to default or throw error. Love2D uses it directly.
    *   This will affect subsequent calls to `GetSaveDirectory()` and all write operations.
    *   The `gameIdentity` static field in [`Filesystem.cs`](src/Night/Filesystem/Filesystem.cs:39) should be updated.
*   **Acceptance Criteria:**
    *   `GetIdentity()` returns the new name.
    *   `GetSaveDirectory()` uses the new name.
    *   Save operations use the new directory.
*   **Test Scenarios/Cases:**
    *   `SetIdentity_ValidName`: Set and verify `GetIdentity` and `GetSaveDirectory`.
    *   `SetIdentity_InvalidChars`: Test sanitization or error handling.
    *   `SetIdentity_NullOrEmpty`: Test behavior.

#### 29. [ ] `Night.Filesystem.SetRequirePath(string path)`
*   **Love2D Equivalent:** `love.filesystem.setRequirePath(path)`
*   **C# Signature Idea:** `public static void SetRequirePath(string path)`
*   **Requirements:** Lua-specific. See `GetRequirePath`.
*   **Acceptance Criteria:** TBD.
*   **Test Scenarios/Cases:** TBD.
*   **Note:** Likely low priority or out of scope.

#### 30. [ ] `Night.Filesystem.SetSource(string path)`
*   **Love2D Equivalent:** `love.filesystem.setSource(path)`
*   **Description:** "Sets the source of the game, where the code is present. Used internally."
*   **Night:** If we allow configuring the source/assets directory beyond `AppContext.BaseDirectory`, this would be the function. It should be clearly marked if it's for advanced use or internal setup.
*   **Status:** Internal/Advanced. May not need public exposure initially.

#### 31. [ ] `Night.Filesystem.SetSymlinksEnabled(bool enable)`
*   **Love2D Equivalent:** `love.filesystem.setSymlinksEnabled(enable)`
*   **C# Signature Idea:** `public static void SetSymlinksEnabled(bool enable)`
*   **Requirements:**
    *   Enable or disable symlink following for filesystem operations.
    *   Updates the internal static flag queried by `AreSymlinksEnabled()`.
*   **Acceptance Criteria:**
    *   `AreSymlinksEnabled()` reflects the new state.
    *   Filesystem operations (e.g., `GetInfo`, `Read`) respect this setting when encountering symlinks.
*   **Test Scenarios/Cases:**
    *   `SetSymlinksEnabled_True`: Enable and test operations on a symlink.
    *   `SetSymlinksEnabled_False`: Disable and test operations on a symlink.

#### 32. [ ] `Night.Filesystem.Unmount(string archivePath)`
*   **Love2D Equivalent:** `love.filesystem.unmount(archive)`
*   **C# Signature Idea:** `public static bool Unmount(string archivePath)`
*   **Requirements:** See `Mount()`.
*   **Acceptance Criteria:** TBD.
*   **Test Scenarios/Cases:** TBD.
*   **Note:** Potentially complex and lower priority.

#### 33. [x] `Night.Filesystem.Write(string filepath, string data, long? size = null)`
*   **Love2D Equivalent:** `love.filesystem.write(filepath, data, size)`
*   **C# Implementation:** In [`src/Night/Filesystem/Filesystem.Write.cs`](src/Night/Filesystem/Filesystem.Write.cs:54) (string data overload)
*   **Enhancement:**
    *   `filepath` must be resolved relative to the **save directory**. The directory (and subdirectories in `filepath`) should be created if it doesn't exist within the save directory.
*   **Status:** Exists. Path resolution to save directory and auto-creation of subdirs needs verification/implementation.

#### 34. [x] `Night.Filesystem.Write(string filepath, byte[] data, long? size = null)`
*   **Love2D Equivalent:** `love.filesystem.write(filepath, data, size)` (Love2D uses `Data` object, we use `byte[]`)
*   **C# Implementation:** In [`src/Night/Filesystem/Filesystem.Write.cs`](src/Night/Filesystem/Filesystem.Write.cs:79) (byte[] data overload)
*   **Enhancement:**
    *   `filepath` must be resolved relative to the **save directory**. The directory (and subdirectories in `filepath`) should be created if it doesn't exist within the save directory.
*   **Status:** Exists. Path resolution to save directory and auto-creation of subdirs needs verification/implementation.

---
**Documentation Task:**
*   [ ] Create/Update a markdown document (e.g., `docs/filesystem.md`) detailing:
    *   The save directory mechanism: `SetIdentity`, `GetIdentity`, `GetSaveDirectory`.
    *   Exact save paths for Windows, macOS, Linux.
    *   The "source" directory concept for Night.
    *   Path resolution rules (relative paths, save-first for reads, save-only for writes).
    *   Any significant deviations from Love2D behavior.