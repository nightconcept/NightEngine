"""A pointer and screen check for a device: run it in the browser (`?app=debug` on a `nightengine.web` page)
or on the desktop. It draws each contact with its id and phase, and the safe-area insets."""

import pyxel

from . import platform

W, H = 256, 144


def main():
    pyxel.init(W, H, title="nightengine: pointer diagnostic", fps=30, display_scale=1)
    pyxel.mouse(True)
    platform.init(W, H)
    state = {"pointers": ()}

    def update():
        state["pointers"] = platform.sample()

    def draw():
        pyxel.cls(1)
        s = platform.screen
        pyxel.text(4, 4, f"Platform: {platform.platform()}  Touch: {platform.is_touch_device()}", 7)
        pyxel.text(4, 12, f"Viewport: {s.viewport_width}x{s.viewport_height}  {s.orientation}", 7)
        a = s.safe_area
        pyxel.text(4, 20, f"Logical: {W}x{H}  Safe: {a.top:.0f} {a.right:.0f} {a.bottom:.0f} {a.left:.0f}", 7)
        pyxel.rectb(a.left, a.top, W - a.left - a.right, H - a.top - a.bottom, 5)
        for i, p in enumerate(state["pointers"]):
            color = 8 + i % 8
            pyxel.circb(p.x, p.y, 6, color)
            pyxel.text(p.x + 8, p.y, f"#{p.id} {p.phase}", color)
            pyxel.text(4, 36 + i * 8, f"#{p.id}: {p.x},{p.y} from {p.start_x},{p.start_y} {p.phase}", color)
        if not platform.available():
            pyxel.text(4, H - 10, "Desktop: the mouse is pointer 0", 10)

    pyxel.run(update, draw)


if __name__ == "__main__":
    main()
