"""A tiny pixel canvas for generating art without pyxel. host/assets.py copies canvases into image banks."""

from .palette import INK, KEY


def noise(x: int, y: int, seed: int = 0) -> float:
    """Deterministic hash noise in [0, 1)."""
    n = (x * 374761393 + y * 668265263 + seed * 2147483647) & 0xFFFFFFFF
    n = ((n ^ (n >> 13)) * 1274126177) & 0xFFFFFFFF
    return ((n ^ (n >> 16)) & 0xFFFF) / 65536


class Canvas:
    def __init__(self, w: int, h: int, fill: int = KEY):
        self.w, self.h = w, h
        self.px = [[fill] * w for _ in range(h)]

    def pset(self, x: int, y: int, c: int):
        if 0 <= x < self.w and 0 <= y < self.h:
            self.px[y][x] = c

    def pget(self, x: int, y: int) -> int:
        return self.px[y][x] if 0 <= x < self.w and 0 <= y < self.h else KEY

    def rect(self, x: int, y: int, w: int, h: int, c: int):
        for j in range(y, y + h):
            for i in range(x, x + w):
                self.pset(i, j, c)

    def line(self, x0: int, y0: int, x1: int, y1: int, c: int):
        dx, dy = abs(x1 - x0), -abs(y1 - y0)
        sx, sy = (1 if x0 < x1 else -1), (1 if y0 < y1 else -1)
        err = dx + dy
        while True:
            self.pset(x0, y0, c)
            if (x0, y0) == (x1, y1):
                return
            e2 = 2 * err
            if e2 >= dy:
                err += dy
                x0 += sx
            if e2 <= dx:
                err += dx
                y0 += sy

    def ellipse(self, cx: float, cy: float, rx: float, ry: float, c: int):
        for y in range(int(cy - ry) - 1, int(cy + ry) + 2):
            for x in range(int(cx - rx) - 1, int(cx + rx) + 2):
                if ((x + 0.5 - cx) / rx) ** 2 + ((y + 0.5 - cy) / ry) ** 2 <= 1:
                    self.pset(x, y, c)

    def circ(self, cx: float, cy: float, r: float, c: int):
        self.ellipse(cx, cy, r, r, c)

    def stamp(self, rows: list[str], legend: dict[str, int], x: int = 0, y: int = 0, flip: bool = False):
        """Draw text-art rows. '.' and characters missing from the legend are transparent."""
        for j, row in enumerate(rows):
            for i, ch in enumerate(row[::-1] if flip else row):
                c = legend.get(ch, KEY)
                if c != KEY:
                    self.pset(x + i, y + j, c)

    def outline(self, color: int = INK):
        """Add a 1px outline around every opaque pixel."""
        src = [row[:] for row in self.px]
        for y in range(self.h):
            for x in range(self.w):
                if src[y][x] != KEY:
                    continue
                for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                    nx, ny = x + dx, y + dy
                    if 0 <= nx < self.w and 0 <= ny < self.h and src[ny][nx] not in (KEY, color):
                        self.px[y][x] = color
                        break

    def recolor(self, mapping: dict[int, int]) -> "Canvas":
        out = Canvas(self.w, self.h)
        out.px = [[mapping.get(c, c) for c in row] for row in self.px]
        return out

    def flipped(self) -> "Canvas":
        out = Canvas(self.w, self.h)
        out.px = [row[::-1] for row in self.px]
        return out

    def vflipped(self) -> "Canvas":
        out = Canvas(self.w, self.h)
        out.px = [row[:] for row in self.px[::-1]]
        return out


def mirror(half: list[str]) -> list[str]:
    """Rows drawn as the left half plus the centre column, mirrored into symmetric full rows."""
    return [row + row[-2::-1] for row in half]
