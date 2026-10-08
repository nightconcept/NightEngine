import unittest

from nightengine import ALL, A, Button, Driver, Game, Recording, Scene, autopilot, declare

DASH, GUARD = declare(Button("DASH", keys=("SHIFT",), pad=("RIGHTSHOULDER",)), Button("GUARD", keys=("V",)))


class Log(Scene):
    def update(self, game, inp):
        game.log.append((inp.held, inp.pressed))


class TwoExtra(Game):
    buttons = (DASH, GUARD)

    def __init__(self, seed=0, target=None):
        super().__init__(seed, target)
        self.log = []
        self.scenes.replace(Log())


class DeclareTest(unittest.TestCase):
    def test_bits_go_above_the_engine_buttons(self):
        self.assertEqual((DASH.bit, GUARD.bit), (256, 512))
        self.assertEqual((DASH.name, GUARD.pad), ("DASH", ()))

    def test_the_game_reads_its_buttons(self):
        self.assertEqual(TwoExtra().input_mask, ALL | 256 | 512)
        self.assertEqual(Game().input_mask, ALL)


class EdgeTest(unittest.TestCase):
    def test_press_edges_for_extra_buttons(self):
        game = TwoExtra()
        d = Driver(game)
        d.step(DASH.bit | A, 2)
        d.step(DASH.bit | GUARD.bit, 1)
        d.step(0x1000, 1)  # Not declared: dropped.
        self.assertEqual(
            game.log,
            [(DASH.bit | A, DASH.bit | A), (DASH.bit | A, 0), (DASH.bit | GUARD.bit, GUARD.bit), (0, 0)],
        )


class RecordingTest(unittest.TestCase):
    def test_round_trip_keeps_extra_buttons(self):
        codes = [GUARD.bit, DASH.bit | A, 0, GUARD.bit | DASH.bit]
        rec = autopilot.run(TwoExtra, lambda g: codes[g.frame], seed=3, limit=4, done=lambda g: False)[0]
        self.assertEqual(rec.width, 3)
        loaded = Recording.from_json(rec.to_json())
        self.assertEqual(loaded.frames, codes)


if __name__ == "__main__":
    unittest.main()
