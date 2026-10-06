"""The platform the game runs on: pointer input (touch or mouse), screen and safe-area info, and browser storage.

`App` calls `init` after `pyxel.init` and `sample` once per frame, and passes the pointers to `Game.step`.
In a browser built by `nightengine.web`, the page installs `window.nightBridge` (web/static/pointer.mjs), and
touch, pen, and mouse all arrive through it. On the desktop, the mouse is read from pyxel as pointer id 0.
Coordinates are whole logical pixels.
"""

import json
from dataclasses import dataclass, field

import pyxel

from ..inputs import B
from ..pointer import Pointer, PointerKind, PointerPhase


@dataclass(frozen=True)
class SafeArea:
    """Insets in logical pixels that notches and system bars cover. Keep text and touch targets out of them."""

    top: float = 0
    right: float = 0
    bottom: float = 0
    left: float = 0


@dataclass
class Screen:
    width: int = 0  # Logical size, as passed to pyxel.init.
    height: int = 0
    viewport_width: int = 0  # Browser viewport in CSS pixels. 0 on the desktop.
    viewport_height: int = 0
    orientation: str = "LANDSCAPE"  # LANDSCAPE | PORTRAIT
    safe_area: SafeArea = field(default_factory=SafeArea)


class MouseTracker:
    """Turns pyxel's mouse into pointer id 0: PRESSED, then MOVED or HELD while the left button is down,
    then RELEASED."""

    def __init__(self, width: int, height: int):
        self.width, self.height = width, height
        self.down = False
        self.start = self.last = (0, 0)

    def position(self) -> tuple[int, int]:
        x = max(0, min(self.width - 1, int(pyxel.mouse_x)))
        y = max(0, min(self.height - 1, int(pyxel.mouse_y)))
        return x, y

    def sample(self) -> tuple[Pointer, ...]:
        held = pyxel.btn(pyxel.MOUSE_BUTTON_LEFT)
        if not self.down and pyxel.btnp(pyxel.MOUSE_BUTTON_LEFT):
            self.down = True
            self.start = self.last = self.position()
            return (Pointer(0, *self.start, *self.start, PointerPhase.PRESSED, PointerKind.MOUSE),)
        if not self.down:
            return ()
        pos = self.position()
        if not held:
            self.down = False
            return (Pointer(0, *pos, *self.start, PointerPhase.RELEASED, PointerKind.MOUSE),)
        phase = PointerPhase.HELD if pos == self.last else PointerPhase.MOVED
        self.last = pos
        return (Pointer(0, *pos, *self.start, phase, PointerKind.MOUSE),)


screen = Screen()
_bridge = None
_mouse: MouseTracker | None = None
_info = {"touch": False, "platform": "desktop"}


def init(width: int, height: int, mouse: bool = True):
    """Call after pyxel.init. Finds the browser bridge if the page installed one. `mouse=False` leaves the desktop
    mouse unread, for a target without mouse controls."""
    global _bridge, _mouse, _info
    if width <= 0 or height <= 0:
        raise ValueError("Logical screen dimensions must be positive")
    screen.width, screen.height = width, height
    _info = {"touch": False, "platform": "desktop"}
    _mouse = MouseTracker(width, height) if mouse else None
    try:
        from js import window
    except ImportError:
        _bridge = None
    else:
        _bridge = getattr(window, "nightBridge", None)
    _refresh_info()


def available() -> bool:
    """True in a browser page with the nightengine bridge."""
    return _bridge is not None


def sample() -> tuple[Pointer, ...]:
    """This frame's contacts. Call once per frame; every later read of the frame uses the returned tuple."""
    if available():
        rows = json.loads(_bridge.sample())
        _refresh_info()
        return tuple(_pointer(r) for r in rows)
    return _mouse.sample() if _mouse else ()


def _pointer(r: dict) -> Pointer:
    """A bridge row as a Pointer. A row without a kind is touch."""
    xy = (int(r["x"]), int(r["y"]), int(r["start_x"]), int(r["start_y"]))
    return Pointer(r["id"], *xy, PointerPhase(r["phase"]), PointerKind(r.get("kind", "touch")))


def buttons() -> int:
    """Buttons the page presses for the game: a system Back button (an Android app) is B for one frame."""
    return B if available() and _bridge.takeBack() else 0


def quit_page() -> bool:
    """Ask a native shell to close the app. False in a browser tab or on the desktop."""
    return bool(_bridge.quit()) if available() else False


def _refresh_info():
    global _info
    if not available():
        return
    _info = json.loads(_bridge.info())
    screen.viewport_width = _info["viewport_width"]
    screen.viewport_height = _info["viewport_height"]
    screen.orientation = _info["orientation"]
    screen.safe_area = SafeArea(**_info["safe_area"])


def is_touch_device() -> bool:
    return _info["touch"]


def platform() -> str:
    """`android` or `desktop`. Other systems report `desktop` until they have a tested target."""
    return _info["platform"]


def save(key: str, data: str) -> bool:
    """Keep a string in browser storage. False on the desktop or when the browser refuses."""
    return bool(_bridge.save(key, data)) if available() else False


def load(key: str) -> str | None:
    """A string from browser storage, or None. Raises OSError if the browser blocks storage."""
    if not available():
        return None
    try:
        result = _bridge.load(key)
    except Exception as error:
        raise OSError("Browser storage is unavailable. Allow site storage to read saved data.") from error
    return str(result) if result is not None else None
