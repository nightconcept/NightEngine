# Night / Love2D API

## CLI

- ApplySettings() - love.cli.applySettings

## ConfigurationManager

- LoadConfig() - love.configurationmanager.loadConfig
  - LoadConfig(string? gameDirectory)

## Error

- SetHandler() - love.error.setHandler
  - SetHandler(ErrorHandlerDelegate handler)

## FileData

- GetBytes() - love.filedata.getBytes
- GetExtension() - love.filedata.getExtension
- GetFilenameHint() - love.filedata.getFilenameHint
- GetSize() - love.filedata.getSize
- GetString() - love.filedata.getString

## Filesystem

- Append() - love.filesystem.append
  - Append(string filepath, byte[] data, long? size)
  - Append(string filepath, string data, long? size)
- CreateDirectory() - love.filesystem.createDirectory
  - CreateDirectory(string path)
- GetAppdataDirectory() - love.filesystem.getAppdataDirectory
- GetDirectoryItems() - love.filesystem.getDirectoryItems
  - GetDirectoryItems(string path)
- GetIdentity() - love.filesystem.getIdentity
- GetInfo() - love.filesystem.getInfo
  - GetInfo(string path, FileSystemInfo info)
  - GetInfo(string path, FileType filterType, FileSystemInfo info)
  - GetInfo(string path, FileType? filterType)
- GetSaveDirectory() - love.filesystem.getSaveDirectory
- GetSource() - love.filesystem.getSource
- GetSourceBaseDirectory() - love.filesystem.getSourceBaseDirectory
- GetUserDirectory() - love.filesystem.getUserDirectory
- GetWorkingDirectory() - love.filesystem.getWorkingDirectory
- IsFused() - love.filesystem.isFused
- Lines() - love.filesystem.lines
  - Lines(string filePath)
- NewFile() - love.filesystem.newFile
  - NewFile(string filename)
  - NewFile(string filename, FileMode mode)
- NewFileData() - love.filesystem.newFileData
  - NewFileData(byte[] data, string name)
  - NewFileData(string content, string name)
- Read() - love.filesystem.read
  - Read(ContainerType container, string name, long? sizeToRead)
  - Read(string name, long? sizeToRead)
- ReadBytes() - love.filesystem.readBytes
  - ReadBytes(string path)
- ReadText() - love.filesystem.readText
  - ReadText(string path)
- Remove() - love.filesystem.remove
  - Remove(string filepath)
- SetIdentity() - love.filesystem.setIdentity
  - SetIdentity(string? identityName)
- Write() - love.filesystem.write
  - Write(string name, byte[] data, long? size)
  - Write(string name, string data, long? size)

## Framework

- GetVersion() - love.getVersion
- Run() - love.run
  - Run(IGame game, CLI? cliArgs)

## Game

- Draw() - love.game.draw
- FileDropped() - love.game.fileDropped
  - FileDropped(DroppedFile file)
- GamepadAxis() - love.game.gamepadAxis
  - GamepadAxis(Joystick joystick, GamepadAxis axis, float value)
- GamepadPressed() - love.game.gamepadPressed
  - GamepadPressed(Joystick joystick, GamepadButton button)
- GamepadReleased() - love.game.gamepadReleased
  - GamepadReleased(Joystick joystick, GamepadButton button)
- JoystickAdded() - love.game.joystickAdded
  - JoystickAdded(Joystick joystick)
- JoystickAxis() - love.game.joystickAxis
  - JoystickAxis(Joystick joystick, int axis, float value)
- JoystickHat() - love.game.joystickHat
  - JoystickHat(Joystick joystick, int hat, JoystickHat direction)
- JoystickPressed() - love.game.joystickPressed
  - JoystickPressed(Joystick joystick, int button)
- JoystickReleased() - love.game.joystickReleased
  - JoystickReleased(Joystick joystick, int button)
- JoystickRemoved() - love.game.joystickRemoved
  - JoystickRemoved(Joystick joystick)
- KeyPressed() - love.game.keyPressed
  - KeyPressed(KeySymbol key, KeyCode scancode, bool isRepeat)
- KeyReleased() - love.game.keyReleased
  - KeyReleased(KeySymbol key, KeyCode scancode)
- Load() - love.game.load
- MousePressed() - love.game.mousePressed
  - MousePressed(int x, int y, MouseButton button, bool istouch, int presses)
- MouseReleased() - love.game.mouseReleased
  - MouseReleased(int x, int y, MouseButton button, bool istouch, int presses)
- Quit() - love.game.quit
- Run() - love.game.run
- Update() - love.game.update
  - Update(double deltaTime)

## Graphics

- Circle() - love.graphics.circle
  - Circle(DrawMode mode, float x, float y, float radius, int segments)
- Clear() - love.graphics.clear
  - Clear(Color color)
- Draw() - love.graphics.draw
  - Draw(Sprite sprite, float x, float y, float rotation, float scaleX, float scaleY, float offsetX, float offsetY)
- GetBackgroundColor() - love.graphics.getBackgroundColor
- Line() - love.graphics.line
  - Line(PointF[] points)
  - Line(float x1, float y1, float x2, float y2)
- NewImage() - love.graphics.newImage
  - NewImage(string filePath)
- Polygon() - love.graphics.polygon
  - Polygon(DrawMode mode, PointF[] vertices)
- Present() - love.graphics.present
- Rectangle() - love.graphics.rectangle
  - Rectangle(DrawMode mode, float x, float y, float width, float height)
- SetColor() - love.graphics.setColor
  - SetColor(Color color)
  - SetColor(byte r, byte g, byte b, byte a)

## IGame

- Draw() - love.igame.draw
- FileDropped() - love.igame.fileDropped
  - FileDropped(DroppedFile file)
- GamepadAxis() - love.igame.gamepadAxis
  - GamepadAxis(Joystick joystick, GamepadAxis axis, float value)
- GamepadPressed() - love.igame.gamepadPressed
  - GamepadPressed(Joystick joystick, GamepadButton button)
- GamepadReleased() - love.igame.gamepadReleased
  - GamepadReleased(Joystick joystick, GamepadButton button)
- JoystickAdded() - love.igame.joystickAdded
  - JoystickAdded(Joystick joystick)
- JoystickAxis() - love.igame.joystickAxis
  - JoystickAxis(Joystick joystick, int axis, float value)
- JoystickHat() - love.igame.joystickHat
  - JoystickHat(Joystick joystick, int hat, JoystickHat direction)
- JoystickPressed() - love.igame.joystickPressed
  - JoystickPressed(Joystick joystick, int button)
- JoystickReleased() - love.igame.joystickReleased
  - JoystickReleased(Joystick joystick, int button)
- JoystickRemoved() - love.igame.joystickRemoved
  - JoystickRemoved(Joystick joystick)
- KeyPressed() - love.igame.keyPressed
  - KeyPressed(KeySymbol key, KeyCode scancode, bool isRepeat)
- KeyReleased() - love.igame.keyReleased
  - KeyReleased(KeySymbol key, KeyCode scancode)
- Load() - love.igame.load
- MousePressed() - love.igame.mousePressed
  - MousePressed(int x, int y, MouseButton button, bool istouch, int presses)
- MouseReleased() - love.igame.mouseReleased
  - MouseReleased(int x, int y, MouseButton button, bool istouch, int presses)
- Quit() - love.igame.quit
- Run() - love.igame.run
- Update() - love.igame.update
  - Update(double deltaTime)

## ILogSink

- Write() - love.ilogsink.write
  - Write(LogEntry entry)

## ILogger

- Debug() - love.ilogger.debug
  - Debug(string message)
- Error() - love.ilogger.error
  - Error(string message, Exception? exception)
- Fatal() - love.ilogger.fatal
  - Fatal(string message, Exception? exception)
- Info() - love.ilogger.info
  - Info(string message)
- IsEnabled() - love.ilogger.isEnabled
  - IsEnabled(LogLevel level)
- Log() - love.ilogger.log
  - Log(LogLevel level, string message, Exception? exception)
- Trace() - love.ilogger.trace
  - Trace(string message)
- Warn() - love.ilogger.warn
  - Warn(string message)

## Joystick

- Dispose() - love.joystick.dispose
- GetAxes() - love.joystick.getAxes
- GetAxis() - love.joystick.getAxis
  - GetAxis(int axisIndex)
- GetAxisCount() - love.joystick.getAxisCount
- GetButtonCount() - love.joystick.getButtonCount
- GetDeviceInfo() - love.joystick.getDeviceInfo
- GetGamepadAxis() - love.joystick.getGamepadAxis
  - GetGamepadAxis(GamepadAxis axis)
- GetGamepadMapping() - love.joystick.getGamepadMapping
  - GetGamepadMapping(GamepadAxis axis)
  - GetGamepadMapping(GamepadButton button)
- GetGamepadMappingString() - love.joystick.getGamepadMappingString
- GetGuid() - love.joystick.getGuid
- GetHat() - love.joystick.getHat
  - GetHat(int hatIndex)
- GetHatCount() - love.joystick.getHatCount
- GetId() - love.joystick.getId
- GetName() - love.joystick.getName
- GetVibration() - love.joystick.getVibration
- IsConnected() - love.joystick.isConnected
- IsDown() - love.joystick.isDown
  - IsDown(int buttonIndex)
- IsGamepad() - love.joystick.isGamepad
- IsGamepadDown() - love.joystick.isGamepadDown
  - IsGamepadDown(GamepadButton button)
- IsVibrationSupported() - love.joystick.isVibrationSupported
- SetVibration() - love.joystick.setVibration
  - SetVibration(float left, float right, float durationSeconds)

## Joysticks

- GetJoystickByInstanceId() - love.joystick.getJoystickByInstanceId
  - GetJoystickByInstanceId(uint instanceId)
- GetJoystickCount() - love.joystick.getJoystickCount
- GetJoysticks() - love.joystick.getJoysticks

## Keyboard

- IsDown() - love.keyboard.isDown
  - IsDown(KeyCode key)

## LogManager

- AddSink() - love.logmanager.addSink
  - AddSink(ILogSink sink)
- ClearSinks() - love.logmanager.clearSinks
- ConfigureFileSink() - love.logmanager.configureFileSink
  - ConfigureFileSink(string filePath)
  - ConfigureFileSink(string filePath, LogLevel minLevelForFile)
- DisableFileSink() - love.logmanager.disableFileSink
- EnableSystemConsoleSink() - love.logmanager.enableSystemConsoleSink
  - EnableSystemConsoleSink(bool enable)
- GetLogger() - love.logmanager.getLogger
  - GetLogger(string categoryName)
- IsSystemConsoleSinkEnabled() - love.logmanager.isSystemConsoleSinkEnabled
- RemoveSink() - love.logmanager.removeSink
  - RemoveSink(ILogSink sink)

## MemorySink

- GetEntries() - love.memorysink.getEntries
- Write() - love.memorysink.write
  - Write(LogEntry entry)

## Mouse

- GetPosition() - love.mouse.getPosition
- IsDown() - love.mouse.isDown
  - IsDown(MouseButton button)
- SetGrabbed() - love.mouse.setGrabbed
  - SetGrabbed(bool grabbed)
- SetRelativeMode() - love.mouse.setRelativeMode
  - SetRelativeMode(bool enabled)
- SetVisible() - love.mouse.setVisible
  - SetVisible(bool visible)

## NightFile

- Close() - love.nightfile.close
- Dispose() - love.nightfile.dispose
- Open() - love.nightfile.open
  - Open(Night.FileMode mode)
  - Open(string modeString)
- Read() - love.nightfile.read
- ReadBytes() - love.nightfile.readBytes
  - ReadBytes()
  - ReadBytes(long bytesToRead)

## NightSDL

- GetError() - love.nightsdl.getError
- GetVersion() - love.nightsdl.getVersion

## System

- GetClipboardText() - love.system.getClipboardText
- GetOS() - love.system.getOS
- GetPowerInfo() - love.system.getPowerInfo
- GetProcessorCount() - love.system.getProcessorCount
- OpenURL() - love.system.openURL
  - OpenURL(string url)
- SetClipboardText() - love.system.setClipboardText
  - SetClipboardText(string text)

## SystemConsoleSink

- Write() - love.systemconsolesink.write
  - Write(LogEntry entry)

## Timer

- GetAverageDelta() - love.timer.getAverageDelta
- GetDelta() - love.timer.getDelta
- GetFPS() - love.timer.getFPS
- GetTime() - love.timer.getTime
- Sleep() - love.timer.sleep
  - Sleep(double seconds)
- Step() - love.timer.step

## VersionInfo

- GetVersion() - love.getVersion

## Window

- Close() - love.window.close
- FromPixels() - love.window.fromPixels
  - FromPixels(float value)
- GetDPIScale() - love.window.getDPIScale
- GetDesktopDimensions() - love.window.getDesktopDimensions
  - GetDesktopDimensions(int displayIndex)
- GetDisplayCount() - love.window.getDisplayCount
- GetFullscreen() - love.window.getFullscreen
- GetFullscreenModes() - love.window.getFullscreenModes
  - GetFullscreenModes(int displayIndex)
- GetIcon() - love.window.getIcon
- GetMode() - love.window.getMode
- IsOpen() - love.window.isOpen
- SetFullscreen() - love.window.setFullscreen
  - SetFullscreen(bool fullscreen, FullscreenType fsType)
- SetIcon() - love.window.setIcon
  - SetIcon(string imagePath)
- SetMode() - love.window.setMode
  - SetMode(int width, int height, SDL.WindowFlags flags)
- SetTitle() - love.window.setTitle
  - SetTitle(string title)
- ToPixels() - love.window.toPixels
  - ToPixels(float value)
