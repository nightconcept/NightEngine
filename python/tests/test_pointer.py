"""Pointer records, their helpers, and how they reach a scene through Game.step."""

import unittest

from nightengine import Game, Pointer, PointerPhase, Scene
from nightengine import pointer as ptr
from nightengine.testing import Driver

P = PointerPhase


def at(pid, x, y, phase, sx=None, sy=None):
    return Pointer(pid, x, y, x if sx is None else sx, y if sy is None else sy, phase)


class HelperTest(unittest.TestCase):
    def test_active_excludes_ended_contacts(self):
        self.assertTrue(at(1, 0, 0, P.HELD).active)
        self.assertFalse(at(1, 0, 0, P.RELEASED).active)
        self.assertFalse(at(1, 0, 0, P.CANCELLED).active)

    def test_primary_pressed_released(self):
        ps = (at(1, 5, 5, P.RELEASED), at(2, 6, 6, P.HELD), at(3, 7, 7, P.PRESSED))
        self.assertEqual(ptr.primary(ps).id, 2)
        self.assertEqual(ptr.pressed(ps).id, 3)
        self.assertEqual(ptr.released(ps).id, 1)
        self.assertIsNone(ptr.released((at(1, 0, 0, P.CANCELLED),)))  # A cancel is never a tap.
        self.assertIsNone(ptr.primary(()))

    def test_in_rect_counts_only_active_contacts(self):
        self.assertTrue(ptr.in_rect((at(1, 10, 10, P.HELD),), 10, 10, 1, 1))
        self.assertFalse(ptr.in_rect((at(1, 9, 10, P.HELD),), 10, 10, 1, 1))
        self.assertFalse(ptr.in_rect((at(1, 10, 10, P.RELEASED),), 0, 0, 50, 50))

    def test_encode_decode_round_trip(self):
        for phase in PointerPhase:
            p = at(4, 12, 34, phase, 1, 2)
            self.assertEqual(ptr.decode(ptr.encode(p)), p)
        self.assertEqual(ptr.encode(at(4, 12, 34, P.MOVED, 1, 2)), [4, 12, 34, 1, 2, "M"])


class Recorder(Scene):
    def __init__(self):
        self.seen = []

    def update(self, game, inp):
        self.seen.append(inp.pointers)


class StepTest(unittest.TestCase):
    def test_game_step_delivers_pointers_to_the_scene(self):
        game, scene = Game(), Recorder()
        game.push(scene)
        game.step(0)
        game.step(0, (at(0, 3, 4, P.PRESSED),))
        self.assertEqual(scene.seen[0], ())
        self.assertEqual(scene.seen[1][0].x, 3)

    def test_driver_tap_is_a_press_then_a_release(self):
        game, scene = Game(), Recorder()
        game.push(scene)
        Driver(game).tap(20, 30)
        self.assertEqual([s[0].phase for s in scene.seen], [P.PRESSED, P.RELEASED])
        self.assertEqual((scene.seen[1][0].x, scene.seen[1][0].y), (20, 30))


if __name__ == "__main__":
    unittest.main()
