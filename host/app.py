"""The pyxel window: keyboard, gamepad, and pointer input, audio, recording, and replay playback."""

import atexit
from collections.abc import Callable
from dataclasses import dataclass
from pathlib import Path

import pyxel

from ..core import Game
from ..replay import Recording, digits_for
from . import platform, ui
from .audio import AudioManager
from .keys import read_buttons
from .renderer import Renderer

current: "App | None" = None  # The running App, for browser tests that read the game through Pyodide.


@dataclass
class AppConfig:
    title: str
    width: int
    height: int
    keys: dict[int, tuple[int, ...]]  # Engine button -> pyxel keys and gamepad buttons.
    replays: Path  # Where the last live run is saved.
    fps: int = 60
    label_xy: tuple[int, int] = (4, 4)  # Where the REPLAY label is drawn.
    integer_scale: bool = True  # Scale the screen by whole numbers only, so pixels stay square and even.
    mouse: bool = False  # Show the system mouse cursor over the game.


class App:
    def __init__(
        self,
        config: AppConfig,
        make_game: Callable[[int], Game],
        renderer: Renderer,
        audio: AudioManager,
        seed: int | None = None,
        replay: Recording | None = None,
    ):
        self.config, self.renderer, self.audio = config, renderer, audio
        pyxel.init(config.width, config.height, title=config.title, fps=config.fps, quit_key=pyxel.KEY_NONE)
        platform.init(config.width, config.height)
        # In a browser the page sizes the canvas to the game's aspect, and the pointer bridge maps the whole canvas.
        # The game must fill it, or the picture and the touch coordinates disagree.
        pyxel.integer_scale(config.integer_scale and not platform.available())
        pyxel.mouse(config.mouse and not platform.available())  # A page has its own cursor, and fingers need none.
        renderer.setup()
        audio.setup()
        self.replay = replay
        if replay:
            seed = replay.seed
        elif seed is None:
            seed = pyxel.rndi(0, 2**31 - 1)
        self.game = make_game(seed)
        self.recording = None if replay else Recording(seed, width=digits_for(self.game.input_mask))
        global current
        current = self
        atexit.register(self.save)  # pyxel.run ends the process, so this also saves when the window closes.
        pyxel.run(self.update, self.draw)

    def save(self):
        if self.recording and self.recording.frames:
            self.recording.save(self.config.replays / "last.json")

    def quit(self):
        """Save the run and close. In a page, a native shell (the APK) closes the app; a browser tab stays open,
        because pyxel.quit would freeze the page."""
        self.save()
        if platform.available():
            platform.quit_page()
            self.game.quit_requested = False
        else:
            pyxel.quit()

    def update(self):
        if pyxel.btnp(pyxel.KEY_Q) or (pyxel.btn(pyxel.KEY_ALT) and pyxel.btnp(pyxel.KEY_F4)):
            self.quit()
            return
        if self.replay:
            if self.game.frame >= len(self.replay.frames):
                return
            code, pointers = self.replay.frames[self.game.frame], self.replay.at(self.game.frame)
        else:
            code, pointers = read_buttons(self.config.keys) | platform.buttons(), platform.sample()
            self.recording.add(code, pointers)
        cues = self.game.step(code, pointers)
        self.audio.update(self.game.music, cues)
        if self.game.quit_requested:
            self.quit()

    def draw(self):
        self.renderer.draw(self.game)
        if self.replay:
            over = self.game.frame >= len(self.replay.frames)
            ui.text(*self.config.label_xy, "REPLAY OVER - Q TO QUIT" if over else "REPLAY")
