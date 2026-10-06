import contextlib
import io
import tempfile
import unittest
from pathlib import Path
from unittest import mock

from nightengine import Game, Recording, Scene
from nightengine.autopilot import Tapper, cli, run


class Idle(Scene):
    pass


def make_game(seed: int, target=None) -> Game:
    game = Game(seed, target)
    game.push(Idle())
    return game


class AutopilotTest(unittest.TestCase):
    def test_tapper_alternates_press_and_release(self):
        t = Tapper()
        self.assertEqual([t.press(16) for _ in range(4)], [16, 0, 16, 0])

    def test_run_records_every_frame_and_stops_when_done(self):
        recording, game = run(make_game, lambda g: g.frame % 3, 5, 100, done=lambda g: g.frame >= 10)
        self.assertEqual(game.frame, 10)
        self.assertEqual(recording.seed, 5)
        self.assertEqual(recording.frames, [f % 3 for f in range(10)])

    def test_cli_passes_game_options_to_play(self):
        calls = []

        def play(seed, battles=1, fast_mode=False):
            calls.append((seed, battles, fast_mode))
            return Recording(seed, [0, 16]), make_game(seed)

        with tempfile.TemporaryDirectory() as tmp:
            out = Path(tmp) / "run.json"
            argv = ["autopilot", "--seed", "3", "--battles", "4", "--fast-mode", "1", "--out", str(out)]
            with mock.patch("sys.argv", argv), contextlib.redirect_stdout(io.StringIO()):
                cli(play, lambda g: "ok", out, "doc", options={"battles": 1, "fast_mode": False})
            self.assertEqual(Recording.load(out).frames, [0, 16])
            argv[6] = "0"
            with mock.patch("sys.argv", argv), contextlib.redirect_stdout(io.StringIO()):
                cli(play, lambda g: "ok", out, "doc", options={"battles": 1, "fast_mode": False})
        self.assertEqual(calls, [(3, 4, True), (3, 4, False)])


if __name__ == "__main__":
    unittest.main()
