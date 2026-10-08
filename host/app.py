"""The pyxel window: keyboard, gamepad, and pointer input, audio, recording, and replay playback."""

import atexit
from collections.abc import Callable
from dataclasses import dataclass
from pathlib import Path

import pyxel

from ..core import Game
from ..pointer import Pointer, PointerKind, PointerPhase
from ..replay import Recording, digits_for
from ..store import Store
from ..target import Target
from ..target import current as current_target
from . import gamepad, keys, platform, ui
from .audio import AudioManager
from .keys import read_buttons
from .renderer import Renderer
from .storage import open_store

current: "App | None" = None  # The running App, for browser tests that read the game through Pyodide.


@dataclass
class AppConfig:
    title: str
    width: int  # The game's own size. A target may set another (see target.py).
    height: int
    keys: dict[int, tuple[int, ...]]  # Engine button -> pyxel keys and gamepad buttons.
    replays: Path  # Where the last live run is saved.
    fps: int = 60
    label_xy: tuple[int, int] = (4, 4)  # Where the REPLAY label is drawn.
    integer_scale: bool = True  # Scale the screen by whole numbers only, so pixels stay square and even.
    mouse: bool = False  # Show the system mouse cursor over the game, when the target takes the mouse.
    sticks: tuple[tuple[int, int], ...] = ((1, 0),)  # (pad, bit shift): left sticks read as d-pad buttons.
    pad_mappings: Path | None = gamepad.DB  # A controller mapping file for SDL (see host/gamepad.py), or None.
    vendor: str | None = None  # With a vendor, `App.store` keeps the game's files in the user's data folder.
    display_scale: int | None = None  # The window's scale at the start (pyxel.init). None lets pyxel choose.


def screen_size(game: Game, target: Target) -> tuple[int, int]:
    """The size the game wants (`Game.screen_size`), else the target's."""
    return game.screen_size or (target.width, target.height)


def split_keys(table: dict[int, tuple[int, ...]]) -> tuple[dict, dict]:
    """A key table split in two: the keyboard keys, and the pad buttons (`keys.is_pad`)."""
    keyboard = {b: tuple(k for k in ks if not keys.is_pad(k)) for b, ks in table.items()}
    pad = {b: tuple(k for k in ks if keys.is_pad(k)) for b, ks in table.items()}
    return keyboard, pad


def device_of(last: str, key_code: int, pad_code: int, pointers: tuple[Pointer, ...]) -> str:
    """The device in use after this frame: "pointer" on a new contact, else "pad" when a pad button or a stick
    is held, else "keyboard" when a key is held, else the last one. For display only: rules never read it."""
    if any(p.phase == PointerPhase.PRESSED for p in pointers):
        return "pointer"
    if pad_code:
        return "pad"
    if key_code:
        return "keyboard"
    return last


def drain_writes(game: Game, store: Store | None, live: bool):
    """Save the files the game asked for this frame (`Game.write`), then clear them. A replay saves nothing."""
    if live and store is not None:
        for key, text in game.writes:
            store.write(key, text)
    game.writes.clear()


def choose_target(config: AppConfig, replay: Recording | None = None, target: Target | None = None) -> Target:
    """A replay runs as it was played. Otherwise: `target`, else the build's, else `NIGHTENGINE_TARGET`.
    A target without a size gets the game's."""
    chosen = (replay.target if replay else None) or target or current_target()
    return chosen.sized(config.width, config.height)


class App:
    pending: tuple[str, ...] = ()  # Host events for the next frame (`post`).
    device = "keyboard"  # The device used last: "keyboard", "pad", or "pointer". Display only: rules never read it.
    _split: tuple[dict, dict, dict | None] = ({}, {}, None)  # The keyboard and pad tables, and their source.
    _bound: tuple = (None, None)  # The game's last `Bindings` and the key table made from it.
    store: Store | None = None  # The game's files (`AppConfig.vendor`). None: nothing is saved.

    def __init__(
        self,
        config: AppConfig,
        make_game: Callable[..., Game],
        renderer: Renderer,
        audio: AudioManager,
        seed: int | None = None,
        replay: Recording | None = None,
        target: Target | None = None,
    ):
        self.config, self.renderer, self.audio = config, renderer, audio
        self.target = target = choose_target(config, replay, target)
        gamepad.use_mappings(config.pad_mappings)  # Before pyxel.init: SDL reads the hint when it starts.
        if config.vendor:
            self.store = open_store(config.vendor, config.title)
        pyxel.init(
            target.width,
            target.height,
            title=config.title,
            fps=config.fps,
            quit_key=pyxel.KEY_NONE,
            display_scale=config.display_scale,
        )
        mouse = target.takes(PointerKind.MOUSE)
        platform.init(target.width, target.height, mouse)
        # In a browser the page sizes the canvas to the game's aspect, and the pointer bridge maps the whole canvas.
        # The game must fill it, or the picture and the touch coordinates disagree.
        pyxel.integer_scale(config.integer_scale and not platform.available())
        # A page has its own cursor, and fingers need none.
        pyxel.mouse(config.mouse and mouse and not platform.available())
        renderer.setup()
        audio.setup()
        self.replay = replay
        if replay:
            seed = replay.seed
        elif seed is None:
            seed = pyxel.rndi(0, 2**31 - 1)
        self.game = make_game(seed, target)
        self.recording = None if replay else Recording(seed, width=digits_for(self.game.input_mask), target=target)
        if not replay:
            self.pending = (*self.boot(), *self.pending)  # Frame 0 gets them. A replay has them in its recording.
        self.fit()
        global current
        current = self
        atexit.register(self.save)  # pyxel.run ends the process, so this also saves when the window closes.
        pyxel.run(self.update, self.draw)

    def boot(self) -> list[str]:
        """Host events for frame 0 of a live run, such as the saved settings. A game's App subclass overrides it.
        It runs once, after `make_game`. A replay never calls it: the recording holds what it returned."""
        return []

    def post(self, event: str):
        """Send a host event to the game on the next frame. The recording keeps it."""
        self.pending = (*self.pending, event)

    def key_table(self) -> dict[int, tuple[int, ...]]:
        """The key table in use: engine button -> pyxel keys and pad buttons. It comes from `game.bindings` when the
        game sets them (built again only when they are a new object), else from `config.keys`."""
        bindings = self.game.bindings
        if bindings is None:
            return self.config.keys
        if self._bound[0] is not bindings:
            self._bound = (bindings, keys.table(bindings))
        return self._bound[1]

    def key_tables(self) -> tuple[dict, dict]:
        """The key table in use, split into keyboard keys and pad buttons. Built again only when the table changes."""
        table = self.key_table()
        if self._split[2] is not table:
            self._split = (*split_keys(table), table)
        return self._split[0], self._split[1]

    def read_input(self) -> tuple[int, tuple[Pointer, ...], tuple[str, ...]]:
        """This frame's live input: the buttons, the pointers, and the host events. It also sets `device`, and
        adds "press key:<NAME>" or "press pad:<NAME>" while the game listens (`Game.listen`)."""
        keyboard, pad = self.key_tables()
        key_code = read_buttons(keyboard)
        pad_code = read_buttons(pad) | gamepad.read_sticks(self.config.sticks)
        code = key_code | pad_code | platform.buttons()
        pointers = platform.sample()
        events, self.pending = list(self.pending), ()
        if self.game.listen:
            pressed = keys.first_pressed()
            if pressed:
                events.append("press " + pressed)
        self.device = device_of(self.device, key_code, pad_code, pointers)
        return code, pointers, tuple(events)

    def fit(self):
        """Resize the screen when the game asks for another size."""
        size = screen_size(self.game, self.target)
        if size != (platform.screen.width, platform.screen.height):
            platform.resize(*size)

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
            frame = self.game.frame
            if frame >= len(self.replay.frames):
                return
            code, pointers, events = self.replay.frames[frame], self.replay.at(frame), self.replay.events_at(frame)
        else:
            code, pointers, events = self.read_input()
            self.recording.add(code, pointers, events)
        cues = self.game.step(code, pointers, events)
        drain_writes(self.game, self.store, live=not self.replay)
        self.fit()
        self.audio.update(self.game.music, cues)
        if self.game.quit_requested:
            self.quit()

    def draw(self):
        self.renderer.draw(self.game)
        if self.replay:
            over = self.game.frame >= len(self.replay.frames)
            ui.text(*self.config.label_xy, "REPLAY OVER - Q TO QUIT" if over else "REPLAY")
