import json
import tempfile
import unittest
from pathlib import Path

from nightengine import UP, A, Game, Pointer, PointerPhase, Recording, Scene, Tapper, autopilot, replay


class Counter(Scene):
    def update(self, game, inp):
        game.count = getattr(game, "count", 0) + inp.held + game.rng.randint(0, 9)
        game.count += sum(p.x * 1000 + p.y for p in inp.pointers)


def tap(x, y, phase=PointerPhase.PRESSED):
    return (Pointer(0, x, y, x, y, phase),)


def make_game(seed):
    game = Game(seed)
    game.scenes.replace(Counter())
    return game


class RecordingTest(unittest.TestCase):
    def test_round_trip_and_format(self):
        rec = Recording(42, [0, 1, 16, 255, 128])
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "sub" / "run.json"
            rec.save(path)
            self.assertEqual(json.loads(path.read_text()), {"seed": 42, "inputs": "000110ff80"})  # No pointers key.
            self.assertEqual(Recording.load(path), rec)

    def test_pointers_round_trip_sparsely(self):
        rec = Recording(5)
        rec.add(0)
        rec.add(A, tap(10, 20))
        rec.add(0, tap(10, 20, PointerPhase.RELEASED))
        rec.add(0)
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "run.json"
            rec.save(path)
            data = json.loads(path.read_text())
            self.assertEqual(data["pointers"], {"1": [[0, 10, 20, 10, 20, "P"]], "2": [[0, 10, 20, 10, 20, "R"]]})
            loaded = Recording.load(path)
        self.assertEqual(loaded, rec)
        self.assertEqual(loaded.at(0), ())
        self.assertEqual(loaded.at(2)[0].phase, PointerPhase.RELEASED)

    def test_files_without_pointers_load(self):
        self.assertEqual(Recording.from_json('{"seed": 1, "inputs": "0001"}'), Recording(1, [0, 1]))

    def test_play_replays_pointers(self):
        rec = Recording(3)
        for i in range(6):
            rec.add(0, tap(i, 7) if i % 2 else ())
        live = make_game(3)
        for i, code in enumerate(rec.frames):
            live.step(code, rec.at(i))
        self.assertEqual(replay.play(make_game, rec).count, live.count)

    def test_play_reproduces_the_run(self):
        rec = Recording(9, [UP, 0, A, A | UP, 0])
        a, b = replay.play(make_game, rec), replay.play(make_game, rec)
        self.assertEqual((a.count, a.frame), (b.count, b.frame))
        self.assertEqual(replay.play(make_game, rec, frames=2).frame, 2)


class AutopilotTest(unittest.TestCase):
    def test_tapper_alternates_edges(self):
        t = Tapper()
        self.assertEqual([t.press(A) for _ in range(4)], [A, 0, A, 0])

    def test_run_records_and_stops(self):
        rec, game = autopilot.run(make_game, lambda g: UP, seed=1, limit=50, done=lambda g: g.frame == 7)
        self.assertEqual((game.frame, len(rec.frames), rec.seed), (7, 7, 1))
        rec, game = autopilot.run(make_game, lambda g: 0, seed=1, limit=10)
        self.assertEqual(game.frame, 10)
        self.assertEqual(replay.play(make_game, rec).count, game.count)

    def test_run_records_pointers_from_choose(self):
        choose = lambda g: (0, tap(g.frame, 1)) if g.frame == 3 else A  # noqa: E731
        rec, game = autopilot.run(make_game, choose, seed=2, limit=6)
        self.assertEqual(list(rec.pointers), [3])
        self.assertEqual(replay.play(make_game, rec).count, game.count)


if __name__ == "__main__":
    unittest.main()
