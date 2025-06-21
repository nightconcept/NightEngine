# Night / Love2D API

## Configuration

### Types (Configuration)

- AudioConfig
- GameConfig
- ModulesConfig
- WindowConfig

## Filesystem

### Types (Filesystem)

- DroppedFile
- FileSystemInfo
- NightFile

### Enums (Filesystem)

- BufferMode
- ContainerType
- FileMode
- FileType

## Graphics

### Types (Graphics)

- Color
- ImageData
- PointF
- Rectangle
- Sprite

### Enums (Graphics)

- DrawMode

## Joysticks

### Types (Joysticks)

- DeviceInfo
- GamepadMappingResult
- Joystick
- VibrationStrength

### Functions (Joysticks)

- GetJoystickByInstanceId() - love.joysticks.getJoystickByInstanceId
  - GetJoystickByInstanceId(uint instanceId)
- GetJoystickCount() - love.joysticks.getJoystickCount
- GetJoysticks() - love.joysticks.getJoysticks

### Enums (Joysticks)

- GamepadAxis
- GamepadButton
- JoystickHat
- JoystickInputType

## Keyboard

### Functions (Keyboard)

- IsDown() - love.keyboard.isDown
  - IsDown(KeyCode key)

### Enums (Keyboard)

- KeyCode
- KeySymbol

## Log

### Enums (Log)

- LogLevel

## Mouse

### Functions (Mouse)

- GetPosition() - love.mouse.getPosition
- IsDown() - love.mouse.isDown
  - IsDown(MouseButton button)
- SetGrabbed() - love.mouse.setGrabbed
  - SetGrabbed(bool grabbed)
- SetRelativeMode() - love.mouse.setRelativeMode
  - SetRelativeMode(bool enabled)
- SetVisible() - love.mouse.setVisible
  - SetVisible(bool visible)

### Enums (Mouse)

- MouseButton

## System

### Functions (System)

- GetClipboardText() - love.system.getClipboardText
- GetOS() - love.system.getOS
- GetPowerInfo() - love.system.getPowerInfo
- GetProcessorCount() - love.system.getProcessorCount
- OpenURL() - love.system.openURL
  - OpenURL(string url)
- SetClipboardText() - love.system.setClipboardText
  - SetClipboardText(string text)

### Enums (System)

- PowerState

## Timer

### Functions (Timer)

- GetAverageDelta() - love.timer.getAverageDelta
- GetDelta() - love.timer.getDelta
- GetFPS() - love.timer.getFPS
- GetTime() - love.timer.getTime
- Sleep() - love.timer.sleep
  - Sleep(double seconds)
- Step() - love.timer.step

## Window

### Types (Window)

- WindowMode

### Enums (Window)

- FullscreenType
