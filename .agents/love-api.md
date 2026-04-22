# Love2D API Coverage

Current implementation status vs. the latest tracked Love2D API surface for NightEngine.

Source: `https://raw.githubusercontent.com/love2d-community/love-api/master/love_api.lua`
Upstream version: `11.5`

Summary:
- Tracked: 351
- Implemented: 85
- Excluded: 1
- Remaining: 265
- Coverage: 24.3%

## love

- [x] `love.conf`
- [ ] `love.directorydropped`
- [ ] `love.displayrotated`
- [x] `love.draw`
- [ ] `love.errorhandler`
- [x] `love.filedropped`
- [ ] `love.focus`
- [x] `love.gamepadaxis`
- [x] `love.gamepadpressed`
- [x] `love.gamepadreleased`
- [x] `love.getVersion`
- [ ] `love.hasDeprecationOutput`
- [ ] `love.isVersionCompatible`
- [x] `love.joystickadded`
- [x] `love.joystickaxis`
- [x] `love.joystickhat`
- [x] `love.joystickpressed`
- [x] `love.joystickreleased`
- [x] `love.joystickremoved`
- [x] `love.keypressed`
- [x] `love.keyreleased`
- [x] `love.load`
- [ ] `love.lowmemory`
- [ ] `love.mousefocus`
- [ ] `love.mousemoved`
- [x] `love.mousepressed`
- [x] `love.mousereleased`
- [x] `love.quit`
- [ ] `love.resize`
- [x] `love.run`
- [ ] `love.setDeprecationOutput`
- [ ] `love.textedited`
- [ ] `love.textinput`
- [ ] `love.threaderror`
- [ ] `love.touchmoved`
- [~] `love.touchpressed` - excluded in script
- [ ] `love.touchreleased`
- [x] `love.update`
- [ ] `love.visible`
- [ ] `love.wheelmoved`

## love.audio

- [ ] `love.audio.getActiveEffects`
- [ ] `love.audio.getActiveSourceCount`
- [ ] `love.audio.getDistanceModel`
- [ ] `love.audio.getDopplerScale`
- [ ] `love.audio.getEffect`
- [ ] `love.audio.getMaxSceneEffects`
- [ ] `love.audio.getMaxSourceEffects`
- [ ] `love.audio.getOrientation`
- [ ] `love.audio.getPosition`
- [ ] `love.audio.getRecordingDevices`
- [ ] `love.audio.getVelocity`
- [ ] `love.audio.getVolume`
- [ ] `love.audio.isEffectsSupported`
- [ ] `love.audio.newQueueableSource`
- [ ] `love.audio.newSource`
- [ ] `love.audio.pause`
- [ ] `love.audio.play`
- [ ] `love.audio.setDistanceModel`
- [ ] `love.audio.setDopplerScale`
- [ ] `love.audio.setEffect`
- [ ] `love.audio.setMixWithSystem`
- [ ] `love.audio.setOrientation`
- [ ] `love.audio.setPosition`
- [ ] `love.audio.setVelocity`
- [ ] `love.audio.setVolume`
- [ ] `love.audio.stop`

## love.data

- [ ] `love.data.compress`
- [ ] `love.data.decode`
- [ ] `love.data.decompress`
- [ ] `love.data.encode`
- [ ] `love.data.getPackedSize`
- [ ] `love.data.hash`
- [ ] `love.data.newByteData`
- [ ] `love.data.newDataView`
- [ ] `love.data.pack`
- [ ] `love.data.unpack`

## love.event

- [ ] `love.event.clear`
- [ ] `love.event.poll`
- [ ] `love.event.pump`
- [ ] `love.event.push`
- [ ] `love.event.quit`
- [ ] `love.event.wait`

## love.filesystem

- [x] `love.filesystem.append`
- [ ] `love.filesystem.areSymlinksEnabled`
- [x] `love.filesystem.createDirectory`
- [x] `love.filesystem.getAppdataDirectory`
- [ ] `love.filesystem.getCRequirePath`
- [x] `love.filesystem.getDirectoryItems`
- [x] `love.filesystem.getIdentity`
- [x] `love.filesystem.getInfo`
- [ ] `love.filesystem.getRealDirectory`
- [ ] `love.filesystem.getRequirePath`
- [x] `love.filesystem.getSaveDirectory`
- [x] `love.filesystem.getSource`
- [x] `love.filesystem.getSourceBaseDirectory`
- [x] `love.filesystem.getUserDirectory`
- [x] `love.filesystem.getWorkingDirectory`
- [ ] `love.filesystem.init`
- [x] `love.filesystem.isFused`
- [x] `love.filesystem.lines`
- [ ] `love.filesystem.load`
- [ ] `love.filesystem.mount`
- [x] `love.filesystem.newFile`
- [x] `love.filesystem.newFileData`
- [x] `love.filesystem.read`
- [x] `love.filesystem.remove`
- [ ] `love.filesystem.setCRequirePath`
- [x] `love.filesystem.setIdentity`
- [ ] `love.filesystem.setRequirePath`
- [ ] `love.filesystem.setSource`
- [ ] `love.filesystem.setSymlinksEnabled`
- [ ] `love.filesystem.unmount`
- [x] `love.filesystem.write`

## love.font

- [ ] `love.font.newBMFontRasterizer`
- [ ] `love.font.newGlyphData`
- [ ] `love.font.newImageRasterizer`
- [ ] `love.font.newRasterizer`
- [ ] `love.font.newTrueTypeRasterizer`

## love.graphics

- [ ] `love.graphics.applyTransform`
- [ ] `love.graphics.arc`
- [ ] `love.graphics.captureScreenshot`
- [x] `love.graphics.circle`
- [x] `love.graphics.clear`
- [ ] `love.graphics.discard`
- [x] `love.graphics.draw`
- [ ] `love.graphics.drawInstanced`
- [ ] `love.graphics.drawLayer`
- [ ] `love.graphics.ellipse`
- [ ] `love.graphics.flushBatch`
- [x] `love.graphics.getBackgroundColor`
- [ ] `love.graphics.getBlendMode`
- [ ] `love.graphics.getCanvas`
- [ ] `love.graphics.getCanvasFormats`
- [ ] `love.graphics.getColor`
- [ ] `love.graphics.getColorMask`
- [ ] `love.graphics.getDPIScale`
- [ ] `love.graphics.getDefaultFilter`
- [ ] `love.graphics.getDepthMode`
- [ ] `love.graphics.getDimensions`
- [ ] `love.graphics.getFont`
- [ ] `love.graphics.getFrontFaceWinding`
- [ ] `love.graphics.getHeight`
- [ ] `love.graphics.getImageFormats`
- [ ] `love.graphics.getLineJoin`
- [ ] `love.graphics.getLineStyle`
- [ ] `love.graphics.getLineWidth`
- [ ] `love.graphics.getMeshCullMode`
- [ ] `love.graphics.getPixelDimensions`
- [ ] `love.graphics.getPixelHeight`
- [ ] `love.graphics.getPixelWidth`
- [ ] `love.graphics.getPointSize`
- [ ] `love.graphics.getRendererInfo`
- [ ] `love.graphics.getScissor`
- [ ] `love.graphics.getShader`
- [ ] `love.graphics.getStackDepth`
- [ ] `love.graphics.getStats`
- [ ] `love.graphics.getStencilTest`
- [ ] `love.graphics.getSupported`
- [ ] `love.graphics.getSystemLimits`
- [ ] `love.graphics.getTextureTypes`
- [ ] `love.graphics.getWidth`
- [ ] `love.graphics.intersectScissor`
- [ ] `love.graphics.inverseTransformPoint`
- [ ] `love.graphics.isActive`
- [ ] `love.graphics.isGammaCorrect`
- [ ] `love.graphics.isWireframe`
- [x] `love.graphics.line`
- [ ] `love.graphics.newArrayImage`
- [ ] `love.graphics.newCanvas`
- [ ] `love.graphics.newCubeImage`
- [ ] `love.graphics.newFont`
- [x] `love.graphics.newImage`
- [ ] `love.graphics.newImageFont`
- [ ] `love.graphics.newMesh`
- [ ] `love.graphics.newParticleSystem`
- [ ] `love.graphics.newQuad`
- [ ] `love.graphics.newShader`
- [ ] `love.graphics.newSpriteBatch`
- [ ] `love.graphics.newText`
- [ ] `love.graphics.newVideo`
- [ ] `love.graphics.newVolumeImage`
- [ ] `love.graphics.origin`
- [ ] `love.graphics.points`
- [x] `love.graphics.polygon`
- [ ] `love.graphics.pop`
- [x] `love.graphics.present`
- [ ] `love.graphics.print`
- [ ] `love.graphics.printf`
- [ ] `love.graphics.push`
- [x] `love.graphics.rectangle`
- [ ] `love.graphics.replaceTransform`
- [ ] `love.graphics.reset`
- [ ] `love.graphics.rotate`
- [ ] `love.graphics.scale`
- [ ] `love.graphics.setBackgroundColor`
- [ ] `love.graphics.setBlendMode`
- [ ] `love.graphics.setCanvas`
- [x] `love.graphics.setColor`
- [ ] `love.graphics.setColorMask`
- [ ] `love.graphics.setDefaultFilter`
- [ ] `love.graphics.setDepthMode`
- [ ] `love.graphics.setFont`
- [ ] `love.graphics.setFrontFaceWinding`
- [ ] `love.graphics.setLineJoin`
- [ ] `love.graphics.setLineStyle`
- [ ] `love.graphics.setLineWidth`
- [ ] `love.graphics.setMeshCullMode`
- [ ] `love.graphics.setNewFont`
- [ ] `love.graphics.setPointSize`
- [ ] `love.graphics.setScissor`
- [ ] `love.graphics.setShader`
- [ ] `love.graphics.setStencilTest`
- [ ] `love.graphics.setWireframe`
- [ ] `love.graphics.shear`
- [ ] `love.graphics.stencil`
- [ ] `love.graphics.transformPoint`
- [ ] `love.graphics.translate`
- [ ] `love.graphics.validateShader`

## love.image

- [ ] `love.image.isCompressed`
- [ ] `love.image.newCompressedData`
- [ ] `love.image.newImageData`

## love.joystick

- [ ] `love.joystick.getGamepadMappingString`
- [x] `love.joystick.getJoystickCount`
- [x] `love.joystick.getJoysticks`
- [ ] `love.joystick.loadGamepadMappings`
- [ ] `love.joystick.saveGamepadMappings`
- [ ] `love.joystick.setGamepadMapping`

## love.keyboard

- [ ] `love.keyboard.getKeyFromScancode`
- [ ] `love.keyboard.getScancodeFromKey`
- [ ] `love.keyboard.hasKeyRepeat`
- [ ] `love.keyboard.hasScreenKeyboard`
- [ ] `love.keyboard.hasTextInput`
- [x] `love.keyboard.isDown`
- [ ] `love.keyboard.isScancodeDown`
- [ ] `love.keyboard.setKeyRepeat`
- [ ] `love.keyboard.setTextInput`

## love.math

- [ ] `love.math.colorFromBytes`
- [ ] `love.math.colorToBytes`
- [ ] `love.math.gammaToLinear`
- [ ] `love.math.getRandomSeed`
- [ ] `love.math.getRandomState`
- [ ] `love.math.isConvex`
- [ ] `love.math.linearToGamma`
- [ ] `love.math.newBezierCurve`
- [ ] `love.math.newRandomGenerator`
- [ ] `love.math.newTransform`
- [ ] `love.math.noise`
- [ ] `love.math.random`
- [ ] `love.math.randomNormal`
- [ ] `love.math.setRandomSeed`
- [ ] `love.math.setRandomState`
- [ ] `love.math.triangulate`

## love.mouse

- [ ] `love.mouse.getCursor`
- [x] `love.mouse.getPosition`
- [ ] `love.mouse.getRelativeMode`
- [ ] `love.mouse.getSystemCursor`
- [ ] `love.mouse.getX`
- [ ] `love.mouse.getY`
- [ ] `love.mouse.isCursorSupported`
- [x] `love.mouse.isDown`
- [ ] `love.mouse.isGrabbed`
- [ ] `love.mouse.isVisible`
- [ ] `love.mouse.newCursor`
- [ ] `love.mouse.setCursor`
- [x] `love.mouse.setGrabbed`
- [ ] `love.mouse.setPosition`
- [x] `love.mouse.setRelativeMode`
- [x] `love.mouse.setVisible`
- [ ] `love.mouse.setX`
- [ ] `love.mouse.setY`

## love.physics

- [ ] `love.physics.getDistance`
- [ ] `love.physics.getMeter`
- [ ] `love.physics.newBody`
- [ ] `love.physics.newChainShape`
- [ ] `love.physics.newCircleShape`
- [ ] `love.physics.newDistanceJoint`
- [ ] `love.physics.newEdgeShape`
- [ ] `love.physics.newFixture`
- [ ] `love.physics.newFrictionJoint`
- [ ] `love.physics.newGearJoint`
- [ ] `love.physics.newMotorJoint`
- [ ] `love.physics.newMouseJoint`
- [ ] `love.physics.newPolygonShape`
- [ ] `love.physics.newPrismaticJoint`
- [ ] `love.physics.newPulleyJoint`
- [ ] `love.physics.newRectangleShape`
- [ ] `love.physics.newRevoluteJoint`
- [ ] `love.physics.newRopeJoint`
- [ ] `love.physics.newWeldJoint`
- [ ] `love.physics.newWheelJoint`
- [ ] `love.physics.newWorld`
- [ ] `love.physics.setMeter`

## love.sound

- [ ] `love.sound.newDecoder`
- [ ] `love.sound.newSoundData`

## love.system

- [x] `love.system.getClipboardText`
- [x] `love.system.getOS`
- [x] `love.system.getPowerInfo`
- [x] `love.system.getProcessorCount`
- [ ] `love.system.hasBackgroundMusic`
- [x] `love.system.openURL`
- [x] `love.system.setClipboardText`
- [ ] `love.system.vibrate`

## love.thread

- [ ] `love.thread.getChannel`
- [ ] `love.thread.newChannel`
- [ ] `love.thread.newThread`

## love.timer

- [x] `love.timer.getAverageDelta`
- [x] `love.timer.getDelta`
- [x] `love.timer.getFPS`
- [x] `love.timer.getTime`
- [x] `love.timer.sleep`
- [x] `love.timer.step`

## love.touch

- [ ] `love.touch.getPosition`
- [ ] `love.touch.getPressure`
- [ ] `love.touch.getTouches`

## love.video

- [ ] `love.video.newVideoStream`

## love.window

- [x] `love.window.close`
- [x] `love.window.fromPixels`
- [x] `love.window.getDPIScale`
- [x] `love.window.getDesktopDimensions`
- [x] `love.window.getDisplayCount`
- [ ] `love.window.getDisplayName`
- [ ] `love.window.getDisplayOrientation`
- [x] `love.window.getFullscreen`
- [x] `love.window.getFullscreenModes`
- [x] `love.window.getIcon`
- [x] `love.window.getMode`
- [ ] `love.window.getPosition`
- [ ] `love.window.getSafeArea`
- [ ] `love.window.getTitle`
- [ ] `love.window.getVSync`
- [ ] `love.window.hasFocus`
- [ ] `love.window.hasMouseFocus`
- [ ] `love.window.isDisplaySleepEnabled`
- [ ] `love.window.isMaximized`
- [ ] `love.window.isMinimized`
- [x] `love.window.isOpen`
- [ ] `love.window.isVisible`
- [ ] `love.window.maximize`
- [ ] `love.window.minimize`
- [ ] `love.window.requestAttention`
- [ ] `love.window.restore`
- [ ] `love.window.setDisplaySleepEnabled`
- [x] `love.window.setFullscreen`
- [x] `love.window.setIcon`
- [x] `love.window.setMode`
- [ ] `love.window.setPosition`
- [x] `love.window.setTitle`
- [ ] `love.window.setVSync`
- [ ] `love.window.showMessageBox`
- [x] `love.window.toPixels`
- [ ] `love.window.updateMode`
