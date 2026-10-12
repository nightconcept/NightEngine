import random
import unittest

from nightengine.inputs import LEFT, A
from nightengine.pacing import InputLatch, Pacer
from nightengine.pointer import Pointer, PointerPhase

P = PointerPhase


def touch(phase: PointerPhase, x: int = 10, y: int = 20, id: int = 1) -> Pointer:
    return Pointer(id, x, y, 10, 20, phase)


def run(pacer: Pacer, intervals) -> list[tuple[int, float]]:
    """Feed the pacer a fake clock: the first frame at 0, then one frame after each interval (seconds)."""
    now, out = 0.0, [pacer.frame(0.0)]
    for dt in intervals:
        now += dt
        out.append(pacer.frame(now))
    return out


class PacerTest(unittest.TestCase):
    def test_first_frame_runs_one_step(self):
        self.assertEqual(Pacer().frame(12.5), (1, 1.0))

    def test_60_hz_steps_once_each_frame(self):
        out = run(Pacer(), [1 / 60] * 600)
        self.assertTrue(all(o == (1, 1.0) for o in out))

    def test_59_94_hz_locks_and_steps_once_each_frame(self):
        pacer = Pacer()
        out = run(pacer, [1 / 59.94] * 600)
        self.assertTrue(pacer.locked)
        self.assertTrue(all(steps == 1 for steps, _ in out))

    def test_120_hz_locks_to_every_second_frame(self):
        pacer = Pacer()
        out = run(pacer, [1 / 120] * 600)
        self.assertTrue(pacer.locked)
        self.assertAlmostEqual(pacer.refresh_hz, 120, places=3)
        tail = out[-8:]
        steps = [s for s, _ in tail]
        self.assertIn(steps, ([1, 0] * 4, [0, 1] * 4))
        for s, a in tail:
            self.assertEqual(a, 0.5 if s else 1.0)

    def test_144_hz_follows_wall_time_with_one_step_at_most(self):
        pacer = Pacer()
        out = run(pacer, [1 / 144] * 600)
        self.assertFalse(pacer.locked)
        self.assertLessEqual(abs(sum(s for s, _ in out[1:]) - 250), 1)
        self.assertTrue(all(s <= 1 for s, _ in out))
        self.assertTrue(all(0 <= a <= 1 for _, a in out))

    def test_30_hz_runs_two_steps_each_frame(self):
        out = run(Pacer(), [1 / 30] * 300)
        self.assertTrue(all(s == 2 for s, _ in out[1:]))

    def test_a_stall_runs_one_step_and_resets(self):
        pacer = Pacer()
        run(pacer, [1 / 120] * 300)
        self.assertEqual(pacer.frame(1000.0 + 1.0), (1, 1.0))
        self.assertFalse(pacer.locked)
        self.assertIsNone(pacer.refresh_hz)

    def test_variable_refresh_follows_wall_time(self):
        rng = random.Random(4)
        intervals = [rng.uniform(1 / 144, 1 / 48) for _ in range(2000)]
        pacer = Pacer()
        out = run(pacer, intervals)
        self.assertTrue(all(s <= pacer.max_steps for s, _ in out))
        self.assertLessEqual(abs(sum(s for s, _ in out[1:]) - sum(intervals) * 60), 1)

    def test_reset_starts_over(self):
        pacer = Pacer()
        run(pacer, [1 / 120] * 300)
        pacer.reset()
        self.assertFalse(pacer.locked)
        self.assertEqual(pacer.frame(50.0), (1, 1.0))


class InputLatchTest(unittest.TestCase):
    def test_a_tap_in_a_frame_with_no_step_reaches_the_next_take(self):
        latch = InputLatch()
        latch.add(A, (), ())
        latch.add(LEFT, (), ("press KEY_Z",))
        self.assertEqual(latch.take(), (A | LEFT, (), ("press KEY_Z",)))
        self.assertEqual(latch.take(), (0, (), ()))

    def test_events_keep_their_order(self):
        latch = InputLatch()
        latch.add(0, (), ("one",))
        latch.add(0, (), ("two", "three"))
        self.assertEqual(latch.take()[2], ("one", "two", "three"))

    def test_a_pointer_keeps_its_last_sample(self):
        latch = InputLatch()
        latch.add(0, (touch(P.HELD, 10),), ())
        latch.add(0, (touch(P.MOVED, 14),), ())
        self.assertEqual(latch.take()[1], (touch(P.MOVED, 14),))

    def test_a_press_is_kept_at_its_own_position(self):
        latch = InputLatch()
        latch.add(0, (touch(P.PRESSED, 10),), ())
        latch.add(0, (touch(P.MOVED, 14),), ())
        self.assertEqual(latch.take()[1], (touch(P.PRESSED, 10),))

    def test_a_press_and_release_in_one_window_give_both(self):
        latch = InputLatch()
        latch.add(0, (touch(P.PRESSED, 10),), ())
        latch.add(0, (touch(P.HELD, 10),), ())
        latch.add(0, (touch(P.RELEASED, 12),), ())
        self.assertEqual(latch.take()[1], (touch(P.PRESSED, 10), touch(P.RELEASED, 12)))

    def test_pointers_keep_the_order_they_came_in(self):
        latch = InputLatch()
        latch.add(0, (touch(P.HELD, id=0),), ())
        latch.add(0, (touch(P.PRESSED, id=3), touch(P.HELD, id=0)), ())
        self.assertEqual([p.id for p in latch.take()[1]], [0, 3])


if __name__ == "__main__":
    unittest.main()
