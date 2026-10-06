"""The base of every game: scene stack, input tracker, screen effects, seeded RNG, and sound cues.

No pyxel here: the renderer reads a Game, and tests drive one headless. A game subclasses `Game`, sets the
class attributes below, adds its own state, and starts its first scene. `step` runs one frame.
"""

from random import Random

from .fx import ScreenFx
from .inputs import ALL, REPEAT_DELAY, REPEAT_RATE, Input, InputTracker
from .pointer import Pointer
from .scene import Scene, SceneStack


class Game:
    input_mask = ALL  # The buttons this game reads. The rest are dropped.
    repeat_buttons: tuple[int, ...] = ()  # Buttons that auto-repeat in `Input.repeat` (menu cursors).
    repeat_delay = REPEAT_DELAY
    repeat_rate = REPEAT_RATE

    def __init__(self, seed: int = 0):
        self.seed = seed
        self.rng = Random(seed)  # The only source of randomness in game rules.
        self.frame = 0
        self.tracker = InputTracker(self.input_mask, self.repeat_buttons, self.repeat_delay, self.repeat_rate)
        self.scenes = SceneStack()
        self.fx = ScreenFx()
        self.cues: list[str] = []

    @property
    def scene(self) -> Scene:
        return self.scenes.top

    def push(self, scene: Scene):
        self.scenes.push(scene)

    def pop(self, scene: Scene):
        self.scenes.pop(scene)

    @property
    def music(self) -> str | None:
        return self.scene.music(self)

    def cue(self, name: str):
        """Ask the audio layer to play a sound effect this frame."""
        self.cues.append(name)

    def before_scene(self, inp: Input):
        """Runs each frame before the top scene updates. Games override it."""

    def after_scene(self):
        """Runs each frame after the top scene updates. Games override it."""

    def step(self, code: int, pointers: tuple[Pointer, ...] = ()) -> list[str]:
        """Advance one frame with the held buttons in `code` and this frame's pointers. Returns sound cue names."""
        self.cues = []
        inp = self.tracker.feed(code, pointers)
        self.fx.tick()
        self.before_scene(inp)
        self.scene.update(self, inp)
        self.after_scene()
        self.frame += 1
        return self.cues
