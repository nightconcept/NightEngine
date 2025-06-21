# Roadmap

Most functions list the Love2D equivalent module/function/callback implementation.

## Version 0.1.0

### Project

- [x] `docfx` generation onto GitHub pages
- [x] Testing framework
- [x] Implement tests
- [ ] Logo and icon - nightO)engine
- [x] CI
- [x] Logging system

### Modules

- [ ] `love.filesystem`: Provides an interface to the user's filesystem.
- [ ] `love.graphics`: Drawing of shapes and images, management of screen geometry. Partial completion.
- [x] `love.joystick`: Provides an interface to connected joysticks.
- [ ] `love.keyboard`: Provides an interface to the user's keyboard.
- [ ] `love.mouse`: Provides an interface to the user's mouse.
- [x] `love.system`: Provides access to information about the user's system.
- [x] `love.timer`: Provides high-resolution timing functionality.
- [ ] `love.window`: Provides an interface for the program's window.

### Callbacks - General

- [x] `love.draw`: Callback function used to draw on the screen every frame.
- [x] `love.load`: This function is called exactly once at the beginning of the game.
- [x] `love.run`: The main callback function, containing the main loop. A sensible default is used when left out.
- [x] `love.update`: Callback function used to update the state of the game every frame.

### Callbacks - Keyboard

- [x] `love.keypressed`: Callback function triggered when a key is pressed.
- [x] `love.keyreleased`: Callback function triggered when a keyboard key is released.

### Callbacks - Mouse

- [x] `love.mousepressed`: Callback function triggered when a mouse button is pressed.
- [x] `love.mousereleased`: Callback function triggered when a mouse button is released.

### Callbacks - General

- [x] `love.errorhandler`: The error handler, used to display error messages.
- [x] `love.joystickpressed`: Callback function triggered when a joystick button is pressed.
- [x] `love.joystickreleased`: Callback function triggered when a joystick button is released.
- [x] `love.gamepadaxis`: Called when a Joystick's virtual gamepad axis is moved.
- [x] `love.gamepadpressed`: Called when a Joystick's virtual gamepad button is pressed.
- [x] `love.gamepadreleased`: Called when a Joystick's virtual gamepad button is released.
- [x] `love.joystickadded`: Called when a Joystick is connected.
- [x] `love.joystickaxis`: Called when a joystick axis moves.
- [x] `love.joystickhat`: Called when a joystick hat direction changes.
- [x] `love.joystickremoved`: Called when a Joystick is disconnected.

### General

- [x] Config Files: Game configuration settings.

## Version 0.2.X

### Modules

- [ ] `love.event`: Manages events, like keypresses.
- [ ] `love.image`: Provides an interface to decode encoded image data.
- [ ] `love.graphics`: Drawing of shapes and images, management of screen geometry. Potential completion.

## Version 0.3.X

### Project

- [ ] Aseprite support
- [ ] NuGet Package

### Modules

- [ ] `love.audio`: Provides an audio interface for playback/recording sound.
- [ ] `love.font`: Allows you to work with fonts.
- [ ] `love.sound`: This module is responsible for decoding sound files.

### Callbacks - General

- [ ] `love.quit`: Callback function triggered when the game is closed.

### Callbacks - Window

- [ ] `love.focus`: Callback function triggered when window receives or loses focus.

## Version 0.4.X

### Modules

- [ ] `love.math`: Provides system-independent mathematical functions.
- [ ] Tiled support

### Callbacks - General

- [ ] `love.thread`: Allows you to work with threads.
- [ ] `love.threaderror`: Callback function triggered when a Thread encounters an error.

### Callbacks - Window

- [ ] `love.mousefocus`: Callback function triggered when window receives or loses mouse focus.
- [ ] `love.resize`: Called when the window is resized.
- [ ] `love.visible`: Callback function triggered when window is shown or hidden.

### Callbacks - Keyboard

- [ ] `love.textinput`: Called when text has been entered by the user.

## Version 0.5.X

### Functions

- [ ] `love.getVersion`: Gets the current running version of LÖVE.

### Third-party modules

- [ ] `utf8`: Provides basic support for manipulating UTF-8 strings.

### Callbacks - Mouse

- [ ] `love.mousemoved`: Callback function triggered when the mouse is moved.

## Version 0.6.X

### Modules

- [ ] `love.video`: This module is responsible for decoding and streaming video files.

### Functions

- [ ] `love.isVersionCompatible`: Gets whether the given version is compatible with the current running version of LÖVE.

### Callbacks - General

- [ ] `love.lowmemory`: Callback function triggered when the system is running out of memory on mobile devices.

### Callbacks - Window

- [ ] `love.directorydropped`: Callback function triggered when a directory is dragged and dropped onto the window.
- [ ] `love.filedropped`: Callback function triggered when a file is dragged and dropped onto the window.

### Callbacks - Keyboard

- [ ] `love.textedited`: Called when the candidate text for an IME has changed.

### Callbacks - Mouse

- [ ] `love.wheelmoved`: Callback function triggered when the mouse wheel is moved.

## Version 0.7.X

### Modules

- [ ] `love.data`: Provides functionality for creating and transforming data.

### Functions

- [ ] `love.hasDeprecationOutput`: Gets whether LÖVE displays warnings when using deprecated functionality.
- [ ] `love.setDeprecationOutput`: Sets whether LÖVE displays warnings when using deprecated functionality.

## Version Horizon (Future)

Mostly related to mobile and touchscreen.

### Modules

- [ ] `love.touch`: Provides an interface to touch-screen presses.

### Callbacks - Window

- [ ] `love.displayrotated`: Called when the device display orientation changed.

### Callbacks - Touch

- [ ] `love.touchmoved`: Callback function triggered when a touch press moves inside the touch screen.
- [ ] `love.touchpressed`: Callback function triggered when the touch screen is touched.
- [ ] `love.touchreleased`: Callback function triggered when the touch screen stops being touched.

## Version Horizon (Far Future)

Networking implementation including rollback.
