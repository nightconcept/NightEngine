"""Keyboard and gamepad input: reads pyxel and returns one button code for the frame."""

from collections.abc import Iterable

import pyxel

from ..inputs import Button


def read_buttons(keys: dict[int, tuple[int, ...]]) -> int:
    """`keys` maps an engine button to the pyxel keys and gamepad buttons that press it."""
    code = 0
    for button, bindings in keys.items():
        if any(pyxel.btn(k) for k in bindings):
            code |= button
    return code


def bindings(buttons: Iterable[Button]) -> dict[int, tuple[int, ...]]:
    """The keyboard table for a game's own buttons (`Game.buttons`). Join it to the rest with `gamepad.merge`."""
    return {b.bit: tuple(getattr(pyxel, f"KEY_{k}") for k in b.keys) for b in buttons if b.keys}
