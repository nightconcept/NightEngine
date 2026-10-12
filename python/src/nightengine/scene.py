"""The scene protocol and the scene stack. Only the top scene updates; the renderer draws `visible()` bottom-up."""

from collections.abc import Iterator
from typing import TYPE_CHECKING, TypeVar

from .inputs import Input

if TYPE_CHECKING:
    from .core import Game

S = TypeVar("S", bound="Scene")


class Scene:
    #: True when the scenes below should still be drawn (overlays, transitions).
    overlay = False

    def update(self, game: "Game", inp: Input):
        pass

    def music(self, game: "Game") -> str | None:
        return None


class SceneStack:
    """A list of scenes, bottom first. `replace` changes it in place, so references to it stay valid."""

    def __init__(self, *scenes: Scene):
        self._items: list[Scene] = list(scenes)

    @property
    def top(self) -> Scene:
        return self._items[-1]

    def push(self, scene: Scene):
        self._items.append(scene)

    def pop(self, scene: Scene):
        """Remove `scene` from anywhere in the stack, if it is there."""
        if scene in self._items:
            self._items.remove(scene)

    def replace(self, *scenes: Scene):
        self._items[:] = scenes

    def find(self, cls: type[S]) -> S | None:
        """The topmost scene that is an instance of `cls`."""
        return next((s for s in reversed(self._items) if isinstance(s, cls)), None)

    def visible(self) -> list[Scene]:
        """From the topmost opaque scene up through any overlays: what the renderer draws."""
        start = len(self._items) - 1
        while start > 0 and self._items[start].overlay:
            start -= 1
        return self._items[max(start, 0) :]

    def index(self, scene: Scene) -> int:
        return self._items.index(scene)

    def __iter__(self) -> Iterator[Scene]:
        return iter(self._items)

    def __len__(self) -> int:
        return len(self._items)

    def __getitem__(self, index):
        return self._items[index]

    def __contains__(self, scene: object) -> bool:
        return scene in self._items

    def __repr__(self) -> str:
        return f"SceneStack({', '.join(type(s).__name__ for s in self._items)})"
