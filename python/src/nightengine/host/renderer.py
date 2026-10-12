"""Draws a Game: the visible scenes bottom-up, each through the draw function registered for its type.

renderer = Renderer(DRAW, PALETTE, bake, FONT, (WHITE, INK), shake=shake_xy)

@renderer.on(Title)
def draw_title(scene, game, t): ...
"""

from collections.abc import Callable
from typing import Any

import pyxel

from ..core import Game
from ..scene import Scene
from . import ui

DrawFn = Callable[[Any, Game, int], None]


def shake_xy(game: Game, t: int) -> tuple[int, int]:
    """Camera offset while `fx.shake` is on: x always, and y only for a strong shake."""
    shake = game.fx.shake
    return (t % 4 - 2) if shake else 0, (t % 3 - 1) if shake > 10 else 0


def shake_x(game: Game, t: int) -> tuple[int, int]:
    """Camera offset while `fx.shake` is on: sideways only."""
    return (t % 4 - 2) if game.fx.shake else 0, 0


def no_shake(game: Game, t: int) -> tuple[int, int]:
    """For a game that shakes inside its own draw functions."""
    return 0, 0


class Renderer:
    """`draw` is the table of draw functions. It is kept, not copied, so a game can add to it after this call.
    `bake` fills the image banks. `ui_colors` is (text colour, shadow colour). `scratch` is for `ui.big_text`.
    `shake` returns the camera offset. `fade=True` draws `game.fx.fade` over everything."""

    def __init__(
        self,
        draw: dict[type, DrawFn] | None,
        palette: tuple[int, ...],
        bake: Callable[[], None],
        font,
        ui_colors: tuple[int, int],
        shake: Callable[[Game, int], tuple[int, int]] = no_shake,
        fade: bool = False,
        scratch: tuple[int, int] = (2, 200),
    ):
        self.table: dict[type, DrawFn] = {} if draw is None else draw
        self.palette, self.bake, self.font, self.ui_colors = palette, bake, font, ui_colors
        self.shake, self.fade, self.scratch = shake, fade, scratch

    def on(self, scene_cls: type[Scene]) -> Callable[[DrawFn], DrawFn]:
        """Register a draw function for a scene type."""

        def register(fn: DrawFn) -> DrawFn:
            self.table[scene_cls] = fn
            return fn

        return register

    def setup(self):
        pyxel.colors[:] = list(self.palette)
        self.bake()
        ui.setup(self.font, *self.ui_colors, scratch=self.scratch)

    def draw(self, game: Game):
        t = game.frame
        dx, dy = self.shake(game, t)
        pyxel.camera(dx, dy)
        for scene in game.scenes.visible():
            self.draw_scene(scene, game, t)
        pyxel.camera()
        if self.fade:
            ui.fade(game.fx.fade)

    def draw_scene(self, scene: Scene, game: Game, t: int):
        """Draw one scene. A scene whose type has no entry uses the entry of its nearest base class."""
        for cls in type(scene).__mro__:
            if cls in self.table:
                return self.table[cls](scene, game, t)
        raise KeyError(f"no draw function for scene {type(scene).__name__}; known: {[c.__name__ for c in self.table]}")
