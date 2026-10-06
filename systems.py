"""Ordered systems: a big scene's frame as a list of small classes, each with one job.

    class Gravity:
        def update(self, scene, game, inp):
            ...

    scene.systems = Systems(Gravity(), Collide(), Cull())

A system returns True to end the frame early (a room exit, a game over). The order is the order of the frame.
"""

from collections.abc import Iterator
from typing import TYPE_CHECKING, Protocol

if TYPE_CHECKING:
    from .core import Game
    from .inputs import Input
    from .scene import Scene


class System(Protocol):
    def update(self, scene: "Scene", game: "Game", inp: "Input") -> bool | None: ...


class Systems:
    def __init__(self, *systems: System):
        self.systems = list(systems)

    def update(self, scene: "Scene", game: "Game", inp: "Input") -> bool:
        """Run each system in order. Stop and return True when one returns True."""
        for system in self.systems:
            if system.update(scene, game, inp):
                return True
        return False

    def __iter__(self) -> Iterator[System]:
        return iter(self.systems)

    def __len__(self) -> int:
        return len(self.systems)
