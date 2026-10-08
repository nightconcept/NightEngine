"""Abstract buttons. One frame of input is one small int, so runs can be recorded and replayed exactly.

The bits are the same in every game. A game masks out the buttons it does not use with `Game.input_mask`.
A game can declare its own buttons above the 8 engine bits with `declare` and `Game.buttons`.
"""

from collections.abc import Iterable
from dataclasses import dataclass, replace

from .pointer import Pointer

UP, DOWN, LEFT, RIGHT, A, B, C, MENU = 1, 2, 4, 8, 16, 32, 64, 128
ALL = UP | DOWN | LEFT | RIGHT | A | B | C | MENU
REPEAT_DELAY, REPEAT_RATE = 14, 5  # Menu cursor auto-repeat, in frames.

DIRECTIONS = ((UP, (0, -1)), (DOWN, (0, 1)), (LEFT, (-1, 0)), (RIGHT, (1, 0)))
EXTRA = 256  # The first bit a game may declare.


@dataclass(frozen=True)
class Button:
    """A game's own button. `keys` are pyxel key names (`KEY_<name>`, such as "SHIFT"), and `pad` Xbox button names
    (`GAMEPADn_BUTTON_<name>`, such as "RIGHTSHOULDER"). `declare` gives it a bit."""

    name: str
    keys: tuple[str, ...] = ()
    pad: tuple[str, ...] = ()
    bit: int = 0


def declare(*buttons: Button) -> tuple[Button, ...]:
    """Give each button the next bit above the engine buttons, in order: 256, 512, and so on."""
    return tuple(replace(b, bit=EXTRA << i) for i, b in enumerate(buttons))


def mask_of(buttons: Iterable[Button]) -> int:
    code = 0
    for b in buttons:
        code |= b.bit
    return code


@dataclass
class Input:
    """Buttons for one frame: `held` this frame, `pressed` on this frame, `repeat` for menu cursors.
    `pointers` are the touch and mouse contacts this frame (see pointer.py)."""

    held: int = 0
    pressed: int = 0
    repeat: int = 0
    pointers: tuple[Pointer, ...] = ()

    def down(self, button: int) -> bool:
        return bool(self.held & button)

    def hit(self, button: int) -> bool:
        return bool(self.pressed & button)

    def nav(self, button: int) -> bool:
        """True on the press, and again each repeat tick while a repeating button stays held."""
        return bool(self.repeat & button)

    def axis(self) -> tuple[int, int]:
        """The held direction as (-1..1, -1..1). Diagonals are allowed; opposite keys cancel."""
        dx = bool(self.held & RIGHT) - bool(self.held & LEFT)
        dy = bool(self.held & DOWN) - bool(self.held & UP)
        return dx, dy

    def direction(self) -> tuple[int, int] | None:
        """The held direction, preferring the newest press. Diagonals are not allowed (tile movement)."""
        for button, step in DIRECTIONS:
            if self.pressed & button:
                return step
        for button, step in DIRECTIONS:
            if self.held & button:
                return step
        return None


class InputTracker:
    """Turns raw held codes into Input objects with press edges and optional auto-repeat.

    `mask` drops the buttons a game does not use. `repeat` lists the buttons that auto-repeat: after `delay`
    frames held they tick every `rate` frames in `Input.repeat`.
    """

    def __init__(self, mask: int = ALL, repeat: Iterable[int] = (), delay: int = REPEAT_DELAY, rate: int = REPEAT_RATE):
        self.mask = mask
        self.delay, self.rate = delay, rate
        self.previous = 0
        self.held_for = {b: 0 for b in repeat}

    def feed(self, code: int, pointers: tuple[Pointer, ...] = ()) -> Input:
        code &= self.mask
        pressed = code & ~self.previous
        repeat = pressed
        for button in self.held_for:
            if code & button:
                self.held_for[button] += 1
                t = self.held_for[button] - self.delay
                if t >= 0 and t % self.rate == 0:
                    repeat |= button
            else:
                self.held_for[button] = 0
        self.previous = code
        return Input(code, pressed, repeat, tuple(pointers))
