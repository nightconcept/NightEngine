import random
import unittest

from nightengine import ALL, DOWN, LEFT, MENU, RIGHT, UP, A, B, C, Input, InputTracker


class ReferenceTracker:
    """The original menu tracker: the timing the engine must reproduce."""

    def __init__(self):
        self.previous = 0
        self.held_for = {b: 0 for b in (UP, DOWN, LEFT, RIGHT)}

    def feed(self, code):
        code &= UP | DOWN | LEFT | RIGHT | A | B
        pressed = code & ~self.previous
        repeat = pressed
        for button in self.held_for:
            if code & button:
                self.held_for[button] += 1
                t = self.held_for[button] - 14
                if t >= 0 and t % 5 == 0:
                    repeat |= button
            else:
                self.held_for[button] = 0
        self.previous = code
        return Input(code, pressed, repeat)


class InputTest(unittest.TestCase):
    def test_button_bits(self):
        self.assertEqual([UP, DOWN, LEFT, RIGHT, A, B, C, MENU], [1, 2, 4, 8, 16, 32, 64, 128])
        self.assertEqual(ALL, 255)

    def test_press_edges(self):
        t = InputTracker()
        first, held = t.feed(A), [t.feed(A) for _ in range(5)]
        self.assertTrue(first.hit(A))
        self.assertFalse(any(i.hit(A) for i in held))
        self.assertTrue(all(i.down(A) for i in held))

    def test_mask_drops_buttons(self):
        t = InputTracker(mask=UP | A)
        inp = t.feed(UP | B | MENU)
        self.assertEqual((inp.held, inp.pressed), (UP, UP))

    def test_repeat_timing(self):
        t = InputTracker(repeat=(DOWN,))
        ticks = [i for i in range(60) if t.feed(DOWN).nav(DOWN)]
        self.assertEqual(ticks, [0, 13, 18, 23, 28, 33, 38, 43, 48, 53, 58])

    def test_repeat_equals_reference_timing(self):
        rng = random.Random(3)
        mine = InputTracker(mask=UP | DOWN | LEFT | RIGHT | A | B, repeat=(UP, DOWN, LEFT, RIGHT))
        ref = ReferenceTracker()
        code = 0
        for _ in range(3000):
            if rng.random() < 0.05:
                code = rng.choice([0, UP, DOWN, LEFT | A, RIGHT, UP | RIGHT, A, B | DOWN, MENU, C])
            self.assertEqual(mine.feed(code), ref.feed(code))

    def test_no_repeat_buttons_means_repeat_is_press(self):
        t = InputTracker()
        inputs = [t.feed(UP) for _ in range(40)]
        self.assertEqual([i.repeat for i in inputs], [UP] + [0] * 39)

    def test_release_resets_the_repeat_counter(self):
        t = InputTracker(repeat=(UP,))
        for _ in range(10):
            t.feed(UP)
        t.feed(0)
        ticks = [i for i in range(20) if t.feed(UP).nav(UP)]
        self.assertEqual(ticks, [0, 13, 18])

    def test_axis_is_a_tuple_and_opposites_cancel(self):
        self.assertEqual(Input(held=LEFT | UP).axis(), (-1, -1))
        self.assertEqual(Input(held=LEFT | RIGHT | DOWN).axis(), (0, 1))
        self.assertEqual(Input().axis(), (0, 0))

    def test_direction_prefers_the_newest_press(self):
        self.assertIsNone(Input().direction())
        self.assertEqual(Input(held=UP | LEFT, pressed=LEFT).direction(), (-1, 0))
        self.assertEqual(Input(held=UP | LEFT).direction(), (0, -1))


if __name__ == "__main__":
    unittest.main()
