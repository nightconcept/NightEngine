"""Copies generated art into pyxel image banks and records where each piece landed.

A game keeps a dictionary of named canvases. `pack` puts them on a shelf and returns a Region for each name.
"""

from dataclasses import dataclass

import pyxel

from ..canvas import Canvas


@dataclass(frozen=True)
class Region:
    bank: int
    u: int
    v: int
    w: int
    h: int


def copy(canvas: Canvas, bank: int, u: int, v: int) -> Region:
    img = pyxel.images[bank]
    for y, row in enumerate(canvas.px):
        for x, c in enumerate(row):
            img.pset(u + x, v + y, c)
    return Region(bank, u, v, canvas.w, canvas.h)


class Shelf:
    """Packs images left to right in rows."""

    def __init__(self, bank: int, v: int = 0, width: int = 256):
        self.bank, self.u, self.v, self.row_h, self.width = bank, 0, v, 0, width

    def add(self, canvas: Canvas) -> Region:
        if self.u + canvas.w > self.width:
            self.u, self.v, self.row_h = 0, self.v + self.row_h, 0
        region = copy(canvas, self.bank, self.u, self.v)
        self.u += canvas.w
        self.row_h = max(self.row_h, canvas.h)
        return region


def pack(named: dict[str, Canvas], shelf: Shelf) -> dict[str, Region]:
    """Add each canvas to the shelf, in order. Returns the region of each name."""
    return {name: shelf.add(canvas) for name, canvas in named.items()}


def pack_variants(named: dict[str, list[Canvas]], shelf: Shelf) -> dict[str, list[Region]]:
    """Like `pack`, for names that have several canvases (tile variants)."""
    return {name: [shelf.add(c) for c in canvases] for name, canvases in named.items()}
