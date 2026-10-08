"""Keyboard and gamepad input: reads pyxel and returns one button code for the frame."""

from collections.abc import Iterable

import pyxel

from ..bindings import Bindings
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


# The virtual keys stand for both their left and right keys, so a press reports SHIFT, not LSHIFT or RSHIFT.
VIRTUAL = ("SHIFT", "CTRL", "ALT", "GUI")
SIDES = {f"{side}{name}" for name in VIRTUAL for side in "LR"}
SKIP = {"NONE", "UNKNOWN"}
_pad_codes: set[int] | None = None


def key_names() -> list[str]:
    """Every key name pyxel knows (`KEY_<name>`), the virtual keys first, without their left and right keys."""
    names = sorted(n[4:] for n in dir(pyxel) if n.startswith("KEY_"))
    rest = [n for n in names if n not in SIDES and n not in SKIP and n not in VIRTUAL]
    return [*VIRTUAL, *rest]


def pad_names(pad: int = 1) -> list[str]:
    """Every button name of pad 1-4 (`GAMEPADn_BUTTON_<name>`)."""
    prefix = f"GAMEPAD{pad}_BUTTON_"
    return sorted(n[len(prefix) :] for n in dir(pyxel) if n.startswith(prefix))


def first_pressed(pads: Iterable[int] = (1,)) -> str | None:
    """The first key or pad button pressed on this frame, as "key:<NAME>" or "pad:<NAME>" with the pyxel name
    without its prefix (KEY_SPACE is "key:SPACE", GAMEPAD1_BUTTON_X is "pad:X"), or None."""
    for name in key_names():
        if pyxel.btnp(getattr(pyxel, f"KEY_{name}")):
            return f"key:{name}"
    for pad in pads:
        for name in pad_names(pad):
            if pyxel.btnp(getattr(pyxel, f"GAMEPAD{pad}_BUTTON_{name}")):
                return f"pad:{name}"
    return None


def is_pad(code: int) -> bool:
    """True when a pyxel button code is a gamepad button, not a key."""
    global _pad_codes
    if _pad_codes is None:
        _pad_codes = {getattr(pyxel, n) for n in dir(pyxel) if n.startswith("GAMEPAD") and "_BUTTON_" in n}
    return code in _pad_codes


def code(kind: str, name: str, pad: int = 1) -> int:
    """The pyxel constant for a key name ("SPACE") or a pad button name ("X"). An unknown name raises ValueError."""
    attr = f"KEY_{name}" if kind == "key" else f"GAMEPAD{pad}_BUTTON_{name}"
    if not hasattr(pyxel, attr):
        raise ValueError(f"unknown {kind} name {name!r} (pyxel has no {attr})")
    return getattr(pyxel, attr)


def table(bindings: Bindings, pad: int = 1) -> dict[int, tuple[int, ...]]:
    """The host's key table from a game's bindings: every key and pad name becomes its pyxel constant."""
    return {
        bit: tuple(code("key", n) for n in b.keys) + tuple(code("pad", n, pad) for n in b.pad)
        for bit, b in bindings.table.items()
    }
