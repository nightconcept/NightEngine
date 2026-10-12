"""A named registry for behaviours: moves, guns, commands, brains. Register with a decorator, look up by name.

A game makes one per kind, `MOVES = Registry("move")`, and exposes `move = MOVES.register`.
Then `@move("sine")` on a handler adds it, and `MOVES["sine"]` finds it.
"""

from collections.abc import Callable, Iterable, Iterator
from typing import Generic, TypeVar

T = TypeVar("T")


class Registry(Generic[T]):
    def __init__(self, kind: str):
        self.kind = kind
        self._items: dict[str, T] = {}

    def register(self, name: str) -> Callable[[T], T]:
        def add(item: T) -> T:
            self._items[name] = item
            return item

        return add

    def __getitem__(self, name: str) -> T:
        try:
            return self._items[name]
        except KeyError:
            raise KeyError(f"unknown {self.kind} {name!r}; known: {', '.join(self._items) or 'none'}") from None

    def __contains__(self, name: object) -> bool:
        return name in self._items

    def __iter__(self) -> Iterator[str]:
        return iter(self._items)

    def __len__(self) -> int:
        return len(self._items)

    def names(self) -> list[str]:
        return list(self._items)

    def missing(self, names: Iterable[str]) -> list[str]:
        """The names that are not registered, without repeats. Content tests use this on the names a JSON file uses."""
        return sorted({n for n in names if n not in self._items})
