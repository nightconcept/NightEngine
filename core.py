"""The base of every game: scene stack, input tracker, screen effects, seeded RNG, and sound cues.

No pyxel here: the renderer reads a Game, and tests drive one headless. A game subclasses `Game`, sets the
class attributes below, adds its own state, and starts its first scene. `step` runs one frame.
"""

from random import Random

from .fx import ScreenFx
from .inputs import ALL, REPEAT_DELAY, REPEAT_RATE, Button, Input, InputTracker, mask_of
from .pointer import Pointer
from .scene import Scene, SceneStack
from .target import Target


class Game:
    input_mask = ALL  # The buttons this game reads. The rest are dropped.
    buttons: tuple[Button, ...] = ()  # The game's own buttons (inputs.declare). They join `input_mask`.
    repeat_buttons: tuple[int, ...] = ()  # Buttons that auto-repeat in `Input.repeat` (menu cursors).
    repeat_delay = REPEAT_DELAY
    repeat_rate = REPEAT_RATE

    def __init__(self, seed: int = 0, target: Target | None = None):
        self.seed = seed
        self.target = target or Target()  # Where the game runs (see target.py). Rules may read it.
        self.rng = Random(seed)  # The only source of randomness in game rules.
        self.frame = 0
        self.input_mask = type(self).input_mask | mask_of(self.buttons)
        self.tracker = InputTracker(self.input_mask, self.repeat_buttons, self.repeat_delay, self.repeat_rate)
        self.scenes = SceneStack()
        self.fx = ScreenFx()
        self.cues: list[str] = []
        self.quit_requested = False  # A scene sets it (a Quit menu item). The host saves the run and closes.
        self.listen = False  # While True, the host reports the next key or pad button pressed as a "press" event.
        self.writes: list[tuple[str, str]] = []  # Files to save (`write`). The host saves them after the frame.
        self.bindings = None  # A `Bindings` the host builds its key table from, or None for the host's own table.

    @property
    def scene(self) -> Scene:
        return self.scenes.top

    def push(self, scene: Scene):
        self.scenes.push(scene)

    def pop(self, scene: Scene):
        self.scenes.pop(scene)

    @property
    def screen_size(self) -> tuple[int, int] | None:
        """The screen size the game wants now, or None for the target's. A game overrides it for a setting such as
        a wide screen. The host resizes the screen after the frame that changes it, so it stays in the replay."""
        return None

    @property
    def music(self) -> str | None:
        return self.scene.music(self)

    def cue(self, name: str):
        """Ask the audio layer to play a sound effect this frame."""
        self.cues.append(name)

    def write(self, key: str, text: str):
        """Ask the host to save `text` under `key` (a settings or save file) after this frame.
        A replay saves nothing."""
        self.writes.append((key, text))

    def before_scene(self, inp: Input):
        """Runs each frame before the top scene updates. Games override it."""

    def after_scene(self):
        """Runs each frame after the top scene updates. Games override it."""

    def step(self, code: int, pointers: tuple[Pointer, ...] = (), events: tuple[str, ...] = ()) -> list[str]:
        """Advance one frame with the held buttons in `code`, this frame's pointers, and its host events (text the
        recording keeps, see `Input.events`). Returns sound cue names."""
        self.cues = []
        pointers = tuple(p for p in pointers if self.target.accepts(p))
        inp = self.tracker.feed(code, pointers, events)
        self.fx.tick()
        self.before_scene(inp)
        self.scene.update(self, inp)
        self.after_scene()
        self.frame += 1
        return self.cues
