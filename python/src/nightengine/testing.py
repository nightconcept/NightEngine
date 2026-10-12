"""Test helpers that play a headless Game through button codes, like a player would.

A game's tests subclass `Driver`, build their own game in `__init__`, and add helpers.
"""

from collections.abc import Callable

from .core import Game
from .pointer import Pointer, PointerKind, PointerPhase


class Driver:
    def __init__(self, game: Game):
        self.game = game

    def step(self, code: int = 0, frames: int = 1, pointers: tuple[Pointer, ...] = ()):
        for _ in range(frames):
            self.game.step(code, pointers)

    def tap(self, x: int, y: int, pid: int = 0, kind: PointerKind = PointerKind.TOUCH):
        """Touch (or click, with `kind=PointerKind.MOUSE`) one point: a PRESSED frame, then a RELEASED frame."""
        self.step(pointers=(Pointer(pid, x, y, x, y, PointerPhase.PRESSED, kind),))
        self.step(pointers=(Pointer(pid, x, y, x, y, PointerPhase.RELEASED, kind),))

    def press(self, button: int):
        """Hold a button for one frame, then release it, so the game sees one press edge."""
        self.step(button)
        self.step(0)

    def run_until(self, predicate: Callable[[], bool], limit: int = 5000, code: int | Callable[[], int] = 0):
        """Step until `predicate()` is true. `code` may be a function that returns the buttons for each frame."""
        for _ in range(limit):
            if predicate():
                return
            self.step(code() if callable(code) else code)
        raise AssertionError("condition never held")
