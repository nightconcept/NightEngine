"""Gamepads, Xbox first: one shared button layout, the left stick as a d-pad, and the community mappings.

SDL names every pad's buttons after the Xbox pad, so `XBOX` below is the layout for every controller SDL knows:
Xbox, PlayStation (cross is A), Switch Pro (by position), and the ~2,000 more in `gamepads/gamecontrollerdb.txt`.

    keys = merge(KEYBOARD, xbox(1))                        # One player.
    keys = merge(KEYBOARD, xbox(1), xbox(2, shift=8))      # Two players: pad 2 sets the bits above 8.
"""

import os
from collections.abc import Iterable
from pathlib import Path

import pyxel

from ..inputs import DOWN, LEFT, MENU, RIGHT, UP, A, B, Button, C

DB = Path(__file__).with_name("gamepads") / "gamecontrollerdb.txt"
HINT = "SDL_GAMECONTROLLERCONFIG_FILE"
PADS = 4  # Pyxel reads gamepads 1 to 4.
AXIS_MAX = 32767
DEADZONE = 0.4  # Of a full stick push. Less than this is not a direction.

# Engine button -> Xbox button names (pyxel's GAMEPADn_BUTTON_<name>). A is the bottom face button, B the right,
# X the left, Y the top. Games use A to confirm or act, B to cancel or jump, C for the third action, MENU to pause.
XBOX: dict[int, tuple[str, ...]] = {
    UP: ("DPAD_UP",),
    DOWN: ("DPAD_DOWN",),
    LEFT: ("DPAD_LEFT",),
    RIGHT: ("DPAD_RIGHT",),
    A: ("A",),
    B: ("B",),
    C: ("X",),
    MENU: ("START",),
}


def use_mappings(path: Path | None = DB) -> bool:
    """Make SDL load a controller mapping file. Call before `pyxel.init` (App does). A mapping file the player
    set in the environment wins. Returns True when the hint is set."""
    if path is None or HINT in os.environ:
        return HINT in os.environ
    if not Path(path).is_file():
        return False
    os.environ[HINT] = str(Path(path).resolve())
    return True


def button(pad: int, name: str) -> int:
    """The pyxel constant for one button of pad 1-4, such as button(2, "A")."""
    return getattr(pyxel, f"GAMEPAD{pad}_BUTTON_{name}")


def xbox(
    pad: int = 1,
    layout: dict[int, tuple[str, ...]] | None = None,
    shift: int = 0,
    buttons: Iterable[Button] = (),
) -> dict[int, tuple[int, ...]]:
    """A key table (engine button -> pyxel buttons) for one pad. `layout` adds to or replaces entries of XBOX,
    for example {B: ("B", "Y")}. `shift` moves the engine buttons up, for a second player's bits. `buttons` adds
    the game's own buttons (`Game.buttons`) with their `pad` names."""
    names = {**XBOX, **{b.bit: b.pad for b in buttons}, **(layout or {})}
    return {b << shift: tuple(button(pad, n) for n in ns) for b, ns in names.items() if ns}


def merge(*tables: dict[int, tuple[int, ...]]) -> dict[int, tuple[int, ...]]:
    """Join key tables. A button in several tables takes every binding."""
    out: dict[int, tuple[int, ...]] = {}
    for table in tables:
        for b, keys in table.items():
            out[b] = out.get(b, ()) + tuple(k for k in keys if k not in out.get(b, ()))
    return out


def axis(pad: int, name: str) -> float:
    """A stick axis of pad 1-4 in -1..1 (name: LEFTX, LEFTY, RIGHTX, RIGHTY)."""
    return pyxel.btnv(getattr(pyxel, f"GAMEPAD{pad}_AXIS_{name}")) / AXIS_MAX


def stick(pad: int = 1, shift: int = 0, deadzone: float = DEADZONE) -> int:
    """The left stick of a pad as d-pad buttons. A diagonal push sets two."""
    x, y = axis(pad, "LEFTX"), axis(pad, "LEFTY")
    code = (LEFT if x <= -deadzone else RIGHT if x >= deadzone else 0) | (
        UP if y <= -deadzone else DOWN if y >= deadzone else 0
    )
    return code << shift


def read_sticks(sticks: tuple[tuple[int, int], ...], deadzone: float = DEADZONE) -> int:
    """Every configured stick, as (pad, shift) pairs."""
    code = 0
    for pad, shift in sticks:
        code |= stick(pad, shift, deadzone)
    return code
