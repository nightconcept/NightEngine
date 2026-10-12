"""Pointers: touch, mouse, and pen contacts as game input. Pure records, so they can be recorded and replayed.

The host samples pointers once per frame (`host/platform.py`) and passes them to `Game.step` with the buttons.
Rules read them from `Input.pointers`. Coordinates are whole logical pixels, so a replay sees the same values.

A contact goes PRESSED, then HELD or MOVED each frame, then RELEASED (a normal end) or CANCELLED (the system took
it away). Each phase lasts one frame except HELD. The mouse is pointer id 0.
Each contact has a kind (touch, pen, or mouse), so a game can tell a finger from a click (see target.py).
"""

from collections.abc import Iterable
from dataclasses import dataclass
from enum import StrEnum


class PointerPhase(StrEnum):
    PRESSED = "PRESSED"
    HELD = "HELD"
    MOVED = "MOVED"
    RELEASED = "RELEASED"
    CANCELLED = "CANCELLED"


ENDED = (PointerPhase.RELEASED, PointerPhase.CANCELLED)
CODES = {phase: phase.value[0] for phase in PointerPhase}  # One letter per phase in recordings.
PHASES = {code: phase for phase, code in CODES.items()}


class PointerKind(StrEnum):
    TOUCH = "touch"
    PEN = "pen"
    MOUSE = "mouse"


KIND_CODES = {kind: kind.value[0] for kind in PointerKind}  # One letter per kind in recordings.
KINDS = {code: kind for kind, code in KIND_CODES.items()}


@dataclass(frozen=True)
class Pointer:
    id: int
    x: int
    y: int
    start_x: int
    start_y: int
    phase: PointerPhase
    kind: PointerKind = PointerKind.TOUCH

    @property
    def active(self) -> bool:
        """True while the contact is down (not on its RELEASED or CANCELLED frame)."""
        return self.phase not in ENDED


def primary(pointers: Iterable[Pointer]) -> Pointer | None:
    """The oldest active contact, or None."""
    return next((p for p in pointers if p.active), None)


def pressed(pointers: Iterable[Pointer]) -> Pointer | None:
    """A contact that started this frame, or None."""
    return next((p for p in pointers if p.phase == PointerPhase.PRESSED), None)


def released(pointers: Iterable[Pointer]) -> Pointer | None:
    """A contact that ended normally this frame, or None. A cancelled contact is never a tap."""
    return next((p for p in pointers if p.phase == PointerPhase.RELEASED), None)


def in_rect(pointers: Iterable[Pointer], x: float, y: float, w: float, h: float) -> bool:
    """True if an active contact is inside the rectangle."""
    return any(p.active and x <= p.x < x + w and y <= p.y < y + h for p in pointers)


def encode(p: Pointer) -> list:
    """A compact row for recordings: [id, x, y, start_x, start_y, phase letter], then a kind letter if not touch."""
    row = [p.id, p.x, p.y, p.start_x, p.start_y, CODES[p.phase]]
    return row if p.kind == PointerKind.TOUCH else [*row, KIND_CODES[p.kind]]


def decode(row: list) -> Pointer:
    """A row from `encode`. Rows without a kind (older files too) are touch."""
    pid, x, y, sx, sy, code, *kind = row
    return Pointer(pid, x, y, sx, sy, PHASES[code], KINDS[kind[0]] if kind else PointerKind.TOUCH)
