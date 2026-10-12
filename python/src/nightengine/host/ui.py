"""Drawing helpers shared by every game: text, big text, sprites at three anchors, silhouettes, and fades.

Call `setup` once. Games add their own widgets (panels, windows, gauges) in their own ui module.
"""

import pyxel

from .assets import Region

__all__ = [
    "DEFAULT",
    "LINE_H",
    "big_text",
    "blt",
    "blt_center",
    "blt_feet",
    "center",
    "fade",
    "setup",
    "solid",
    "text",
    "width",
    "wrap",
]

LINE_H = 12
DEFAULT = -1  # Pass as `shadow` to use the game's default shadow colour. None means no shadow.

_font: pyxel.Font | None = None
_color = 0
_shadow = 0
_scratch = (2, 200)


def setup(font_path, color: int, shadow: int, scratch: tuple[int, int] = (2, 200)):
    """Load the font. `color` and `shadow` are the default text colours. `scratch` is the (bank, v) of a free
    14-pixel row that `big_text` draws into."""
    global _font, _color, _shadow, _scratch
    _font = pyxel.Font(str(font_path))
    _color, _shadow, _scratch = color, shadow, scratch


def text(x: float, y: float, s: str, col: int | None = None, shadow: int | None = DEFAULT):
    col = _color if col is None else col
    shadow = _shadow if shadow == DEFAULT else shadow
    if shadow is not None:
        pyxel.text(x + 1, y + 1, s, shadow, _font)
    pyxel.text(x, y, s, col, _font)


def width(s: str) -> int:
    return _font.text_width(s)


def center(y: float, s: str, col: int | None = None, shadow: int | None = DEFAULT, cx: float | None = None):
    """Draw text centred on `cx`, or on the screen."""
    cx = pyxel.width / 2 if cx is None else cx
    text(cx - width(s) / 2, y, s, col, shadow)


def wrap(s: str, max_w: int) -> list[str]:
    """Split text into lines no wider than `max_w`. A newline starts a new paragraph."""
    lines = []
    for para in s.split("\n"):
        line = ""
        for word in para.split(" "):
            trial = f"{line} {word}" if line else word
            if line and width(trial) > max_w:
                lines.append(line)
                line = word
            else:
                line = trial
        lines.append(line)
    return lines


def big_text(s: str, cx: float, y: float, scale: int, col: int, shadow: int):
    """Draw text into a scratch strip of an image bank, then blit it scaled up, centred on cx."""
    bank, v = _scratch
    img = pyxel.images[bank]
    w = width(s) + 2
    img.rect(0, v, 256, 14, 0)
    img.text(0, v + 1, s, col, _font)
    for ox, c in ((1, shadow), (0, None)):
        if c is not None:
            pyxel.pal(col, c)
        top = y - 7 + 7 * scale  # A scaled blt grows around the region's centre.
        pyxel.blt(cx - w / 2 + ox * scale, top + ox * scale, bank, 0, v, w, 14, 0, scale=scale)
        pyxel.pal()


def blt(region: Region, x: float, y: float, flip: bool = False, **kw):
    """Draw a sprite with its top-left at (x, y)."""
    w = -region.w if flip else region.w
    pyxel.blt(x, y, region.bank, region.u, region.v, w, region.h, 0, **kw)


def blt_center(region: Region, x: float, y: float, flip: bool = False, **kw):
    """Draw a sprite centred on (x, y)."""
    blt(region, x - region.w // 2, y - region.h // 2, flip, **kw)


def blt_feet(region: Region, x: float, y: float, flip: bool = False, **kw):
    """Draw a sprite centred on x with its bottom row on y."""
    blt(region, round(x - region.w / 2), round(y - region.h), flip, **kw)


def solid(region: Region, x: float, y: float, col: int, flip: bool = False, alpha: float = 1.0, anchor: str = "center"):
    """Draw a sprite as a one-colour silhouette (hit flashes, shadows). `anchor` is "center" or "feet"."""
    draw = {"center": blt_center, "feet": blt_feet}[anchor]
    for c in range(1, 33):
        pyxel.pal(c, col)
    if alpha < 1:
        pyxel.dither(alpha)
    draw(region, x, y, flip)
    if alpha < 1:
        pyxel.dither(1)
    pyxel.pal()


def fade(alpha: float, col: int = 0):
    """Cover the screen with `col`, `alpha` from 0 (nothing) to 1 (solid)."""
    if alpha <= 0:
        return
    pyxel.dither(min(1.0, alpha))
    pyxel.rect(0, 0, pyxel.width, pyxel.height, col)
    pyxel.dither(1)
