"""Keyboard and gamepad input: reads pyxel and returns one button code for the frame."""

import pyxel


def read_buttons(keys: dict[int, tuple[int, ...]]) -> int:
    """`keys` maps an engine button to the pyxel keys and gamepad buttons that press it."""
    code = 0
    for button, bindings in keys.items():
        if any(pyxel.btn(k) for k in bindings):
            code |= button
    return code
