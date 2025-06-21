# LÖVE 2D Filesystem API Implementation Tasks

## Types

- [ ] `DroppedFile` - Represents a file dropped from the window. (0.10.0)

- [ ] `File` - Represents a file on the filesystem.

- [ ] `FileData` - Data representing the contents of a file.

## Functions

- [x] `love.filesystem.append(filename, data, size)` - Append data to an existing file. (0.9.0)

- [x] `love.filesystem.createDirectory(path)` - Creates a directory. (0.9.0)

- [x] `love.filesystem.getAppdataDirectory()` - Returns the application data directory.

- [In-Progress] `love.filesystem.getDirectoryItems(path)` - Returns all the files and subdirectories in the directory. (0.9.0)

- [ ] `love.filesystem.getIdentity()` - Gets the write directory name for your game. (0.9.0)

- [ ] `love.filesystem.getInfo(filepath, filetype, filter)` - Gets information about the specified file or directory. (11.0)

- [ ] `love.filesystem.getRealDirectory(filepath)` - Gets the absolute path of the directory containing a filepath. (0.9.2)

- [ ] `love.filesystem.getRequirePath()` - Gets the filesystem paths that will be searched when `require` is called. (0.10.0)

- [ ] `love.filesystem.getSaveDirectory()` - Gets the full path to the designated save directory. (0.5.0)

- [ ] `love.filesystem.getSource()` - Returns the full path to the .love file or directory. (0.9.0)

- [ ] `love.filesystem.getSourceBaseDirectory()` - Returns the full path to the directory containing the .love file. (0.9.0)

- [ ] `love.filesystem.getUserDirectory()` - Returns the path of the user's directory.

- [ ] `love.filesystem.getWorkingDirectory()` - Gets the current working directory. (0.5.0)

- [ ] `love.filesystem.init()` - Initializes love.filesystem (internal use).

- [ ] `love.filesystem.isFused()` - Gets whether the game is in fused mode or not. (0.9.0)

- [ ] `love.filesystem.lines(filename, ...)` - Iterate over the lines in a file. (0.5.0)

- [ ] `love.filesystem.load(filepath)` - Loads a Lua file (but does not run it). (0.5.0)

- [ ] `love.filesystem.mount(archive, mountpoint, appendtopath)` - Mounts a zip file or folder in the game's save directory for reading. (0.9.0)

- [ ] `love.filesystem.newFile(filename, mode)` - Creates a new File object.

- [ ] `love.filesystem.newFileData(contents, name, decoder)` - Creates a new FileData object. (0.7.0)

- [ ] `love.filesystem.read(filename, size)` - Read the contents of a file.

- [ ] `love.filesystem.remove(filepath)` - Removes a file (or directory).

- [ ] `love.filesystem.setIdentity(name, appendtouc)` - Sets the write directory for your game.

- [ ] `love.filesystem.setRequirePath(paths)` - Sets the filesystem paths for `require`. (0.10.0)

- [ ] `love.filesystem.setSource(path)` - Sets the source of the game (internal use).

- [ ] `love.filesystem.unmount(archive)` - Unmounts a zip file or folder. (0.9.0)

- [ ] `love.filesystem.write(filename, data, size)` - Write data to a file.

## Enums

- [ ] `FileMode` - The different modes you can open a File in.

- [ ] `FileType` - The type of a file. (11.0)
