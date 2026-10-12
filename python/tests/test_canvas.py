import unittest

from nightengine import Canvas, mirror, noise
from nightengine.palette import ENDESGA32, INK, KEY, SKINSHADE, WHITE


class CanvasTest(unittest.TestCase):
    def test_pset_clips_and_pget_defaults_to_key(self):
        c = Canvas(3, 2)
        c.pset(-1, 0, WHITE)
        c.pset(3, 0, WHITE)
        c.pset(1, 1, WHITE)
        self.assertEqual(c.pget(1, 1), WHITE)
        self.assertEqual(c.pget(9, 9), KEY)
        self.assertEqual(sum(v == WHITE for row in c.px for v in row), 1)

    def test_flips(self):
        c = Canvas(3, 2)
        c.stamp(["ab.", "..c"], {"a": 1, "b": 2, "c": 3})
        self.assertEqual(c.flipped().px, [[0, 2, 1], [3, 0, 0]])
        self.assertEqual(c.vflipped().px, [[0, 0, 3], [1, 2, 0]])
        self.assertEqual(c.px, [[1, 2, 0], [0, 0, 3]])

    def test_stamp_flip_and_recolor(self):
        c = Canvas(3, 1)
        c.stamp(["ab."], {"a": 1, "b": 2}, flip=True)
        self.assertEqual(c.px, [[0, 2, 1]])
        self.assertEqual(c.recolor({1: 9}).px, [[0, 2, 9]])

    def test_outline_surrounds_opaque_pixels(self):
        c = Canvas(3, 3)
        c.pset(1, 1, WHITE)
        c.outline()
        self.assertEqual(c.pget(0, 1), INK)
        self.assertEqual(c.pget(1, 0), INK)
        self.assertEqual(c.pget(0, 0), KEY)

    def test_line_and_circle_draw_pixels(self):
        c = Canvas(8, 8)
        c.line(0, 0, 7, 7, WHITE)
        c.circ(4, 4, 2, INK)
        self.assertEqual(c.pget(0, 0), WHITE)
        self.assertEqual(c.pget(4, 4), INK)

    def test_mirror(self):
        self.assertEqual(mirror(["ab", "cd"]), ["aba", "cdc"])

    def test_noise_is_deterministic_and_in_range(self):
        self.assertEqual(noise(3, 4, 5), noise(3, 4, 5))
        self.assertTrue(all(0 <= noise(x, 7) < 1 for x in range(50)))

    def test_palette(self):
        self.assertEqual(len(ENDESGA32), 33)  # Transparent black, then the 32 colours.
        self.assertEqual((ENDESGA32[0], KEY, SKINSHADE), (0, 0, 32))
        self.assertEqual(ENDESGA32[WHITE], 0xFFFFFF)


if __name__ == "__main__":
    unittest.main()
