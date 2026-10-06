"""Targets: controls, size, the pointers a game sees, the pyproject tables, the build module, and recordings."""

import os
import sys
import tempfile
import unittest
import zipfile
from pathlib import Path
from unittest import mock

from nightengine import KEYBOARD, MOUSE, TOUCH, Game, Pointer, PointerKind, PointerPhase, Recording, Scene, Target
from nightengine import target as targets
from nightengine.pointer import decode, encode
from nightengine.replay import play
from nightengine.tests.test_web import make_game
from nightengine.web.build import build

PRESS = PointerPhase.PRESSED


def contact(kind: PointerKind, pid: int = 0) -> Pointer:
    return Pointer(pid, 5, 5, 5, 5, PRESS, kind)


class Log(Scene):
    def __init__(self):
        self.seen = []

    def update(self, game, inp):
        self.seen.append(tuple(p.kind.value for p in inp.pointers))


def logged(seed: int, target: Target | None = None) -> Game:
    game = Game(seed, target)
    game.push(Log())
    return game


class TargetTest(unittest.TestCase):
    def test_the_default_takes_every_control(self):
        t = Target()
        self.assertEqual((t.name, t.width, t.controls), ("desktop", 0, {KEYBOARD, MOUSE, TOUCH}))
        self.assertTrue(all(t.accepts(contact(k)) for k in PointerKind))
        self.assertFalse(t.touch_only)

    def test_keyboard_and_touch_drop_the_mouse(self):
        t = Target("web", controls={KEYBOARD, TOUCH})
        self.assertEqual([t.accepts(contact(k)) for k in PointerKind], [True, True, False])

    def test_keyboard_only_drops_every_pointer(self):
        t = Target("desktop", controls={KEYBOARD})
        self.assertFalse(any(t.accepts(contact(k)) for k in PointerKind))

    def test_touch_only_takes_the_mouse_as_a_finger(self):
        t = Target("android", controls={TOUCH})
        self.assertTrue(t.touch_only)
        self.assertTrue(all(t.accepts(contact(k)) for k in PointerKind))

    def test_bad_controls_or_half_a_size_are_errors(self):
        with self.assertRaisesRegex(ValueError, "unknown controls"):
            Target(controls={"joystick"})
        with self.assertRaisesRegex(ValueError, "width and height"):
            Target(width=320)

    def test_sized_fills_only_a_missing_size_and_dicts_round_trip(self):
        self.assertEqual(Target("web").sized(320, 180), Target("web", 320, 180))
        self.assertEqual(Target("web", 640, 360).sized(320, 180).width, 640)
        t = Target("android", 854, 480, {TOUCH})
        self.assertEqual(Target.from_dict(t.to_dict()), t)


class GameTest(unittest.TestCase):
    def test_step_drops_the_pointers_the_target_does_not_take(self):
        game = logged(1, Target("web", controls={KEYBOARD, TOUCH}))
        game.step(0, (contact(PointerKind.MOUSE), contact(PointerKind.TOUCH, 1), contact(PointerKind.PEN, 2)))
        self.assertEqual(game.scene.seen, [("touch", "pen")])

    def test_a_game_without_a_target_takes_everything(self):
        game = logged(1)
        game.step(0, (contact(PointerKind.MOUSE),))
        self.assertEqual(game.scene.seen, [("mouse",)])


class RecordingTest(unittest.TestCase):
    def test_kind_is_saved_and_rows_without_one_are_touch(self):
        mouse = contact(PointerKind.MOUSE)
        self.assertEqual(encode(mouse)[-1], "m")
        self.assertEqual(decode(encode(mouse)), mouse)
        self.assertEqual(len(encode(contact(PointerKind.TOUCH))), 6)
        self.assertEqual(decode([0, 5, 5, 5, 5, "P"]).kind, PointerKind.TOUCH)

    def test_the_target_is_saved_and_play_rebuilds_the_game_with_it(self):
        phone = Target("android", 64, 48, {TOUCH})
        rec = Recording(3, target=phone)
        rec.add(0, (contact(PointerKind.MOUSE),))
        loaded = Recording.from_json(rec.to_json())
        self.assertEqual(loaded.target, phone)
        game = play(logged, loaded)
        self.assertEqual((game.target, game.scene.seen), (phone, [("mouse",)]))
        self.assertIsNone(Recording.from_json('{"seed": 1, "inputs": ""}').target)


PYPROJECT_TARGETS = """
[tool.nightengine.targets.desktop]
controls = ["keyboard"]

[tool.nightengine.targets.android]
controls = ["touch"]
width = 427
height = 240
"""


class ConfigTest(unittest.TestCase):
    def setUp(self):
        self.tmp = tempfile.TemporaryDirectory()
        self.root = Path(self.tmp.name) / "game"
        self.root.mkdir()
        make_game(self.root)
        with (self.root / "pyproject.toml").open("a") as f:
            f.write(PYPROJECT_TARGETS)

    def tearDown(self):
        self.tmp.cleanup()

    def test_targets_come_from_pyproject_and_unlisted_names_get_defaults(self):
        self.assertEqual(targets.named(self.root, "desktop"), Target("desktop", controls={KEYBOARD}))
        self.assertEqual(targets.named(self.root, "android"), Target("android", 427, 240, {TOUCH}))
        self.assertEqual(targets.named(self.root, "web"), Target("web"))
        with self.assertRaisesRegex(ValueError, "unknown target"):
            targets.named(self.root, "console")

    def test_a_source_run_reads_the_environment_and_the_nearest_pyproject(self):
        inner = self.root / "toy" / "scenes"
        with mock.patch.dict(os.environ, {"NIGHTENGINE_TARGET": "android"}):
            self.assertEqual(targets.current(inner), Target("android", 427, 240, {TOUCH}))
        with mock.patch.dict(os.environ, {"NIGHTENGINE_TARGET": ""}):
            self.assertEqual(targets.current(inner).name, "desktop")

    def test_a_build_writes_its_target_into_the_app_and_the_page(self):
        out = build(self.root, Path(self.tmp.name) / "dist", target="android")
        with zipfile.ZipFile(out / "game.pyxapp") as app:
            source = app.read("app/nightengine_build.py").decode()
        self.assertIn('data-width="427" data-height="240"', (out / "index.html").read_text())
        (Path(self.tmp.name) / "nightengine_build.py").write_text(source)
        sys.path.insert(0, self.tmp.name)
        try:
            with mock.patch.dict(os.environ, {"NIGHTENGINE_TARGET": "desktop"}):
                self.assertEqual(targets.current(), Target("android", 427, 240, {TOUCH}))  # The build wins.
        finally:
            sys.path.remove(self.tmp.name)
            sys.modules.pop("nightengine_build", None)

    def test_a_web_build_without_a_target_size_uses_the_games(self):
        out = build(self.root, Path(self.tmp.name) / "dist")
        with zipfile.ZipFile(out / "game.pyxapp") as app:
            source = app.read("app/nightengine_build.py").decode()
        self.assertIn("'width': 320", source)
        self.assertIn('data-width="320"', (out / "index.html").read_text())


if __name__ == "__main__":
    unittest.main()
