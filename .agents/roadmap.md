# Roadmap

Functions reference their Love2D module/function/callback equivalents.

## Version 0.1.0

### Project
- [x] `docfx` generation onto GitHub pages
- [x] Testing framework
- [x] Implement tests
- [ ] Logo and icon
- [x] CI
- [x] Logging system

### Modules
- [ ] `love.filesystem` — user filesystem interface
- [ ] `love.graphics` — shapes, images, screen geometry (partial)
- [x] `love.joystick` — joystick interface
- [ ] `love.keyboard` — keyboard interface
- [ ] `love.mouse` — mouse interface
- [x] `love.system` — system info
- [x] `love.timer` — high-resolution timing
- [ ] `love.window` — window management

### Callbacks — General
- [x] `love.draw`
- [x] `love.load`
- [x] `love.run`
- [x] `love.update`
- [x] `love.errorhandler`

### Callbacks — Keyboard
- [x] `love.keypressed`
- [x] `love.keyreleased`

### Callbacks — Mouse
- [x] `love.mousepressed`
- [x] `love.mousereleased`

### Callbacks — Joystick
- [x] `love.joystickpressed`
- [x] `love.joystickreleased`
- [x] `love.gamepadaxis`
- [x] `love.gamepadpressed`
- [x] `love.gamepadreleased`
- [x] `love.joystickadded`
- [x] `love.joystickaxis`
- [x] `love.joystickhat`
- [x] `love.joystickremoved`

### General
- [x] Config files

## Version 0.2.X

### Modules
- [ ] `love.event` — event queue management
- [ ] `love.image` — encoded image data decoding
- [ ] `love.graphics` — full completion

## Version 0.3.X

### Project
- [ ] Aseprite support
- [ ] NuGet package

### Modules
- [ ] `love.audio` — audio playback/recording
- [ ] `love.font` — font support
- [ ] `love.sound` — sound file decoding

### Callbacks
- [ ] `love.quit`
- [ ] `love.focus`

## Version 0.4.X

### Modules
- [ ] `love.math` — system-independent math functions
- [ ] Tiled support

### Callbacks
- [ ] `love.thread` / `love.threaderror`
- [ ] `love.mousefocus`, `love.resize`, `love.visible`
- [ ] `love.textinput`

## Version 0.5.X

- [ ] `love.getVersion`
- [ ] `utf8` module
- [ ] `love.mousemoved`

## Version 0.6.X

- [ ] `love.video`
- [ ] `love.isVersionCompatible`
- [ ] `love.lowmemory`
- [ ] `love.directorydropped`, `love.filedropped`
- [ ] `love.textedited`
- [ ] `love.wheelmoved`

## Version 0.7.X

- [ ] `love.data`
- [ ] `love.hasDeprecationOutput` / `love.setDeprecationOutput`

## Version Horizon — Future

- [ ] `love.touch` and touch callbacks
- [ ] `love.displayrotated`

## Version Horizon — Far Future

Networking with rollback.
