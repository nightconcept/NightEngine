import os
import tempfile
import unittest
from pathlib import Path
from unittest import mock

import pyxel

from nightengine import DOWN, LEFT, MENU, RIGHT, UP, A, B, Button, C, declare
from nightengine.host import gamepad, keys

DASH, GUARD = declare(Button("DASH", keys=("SHIFT",), pad=("RIGHTSHOULDER",)), Button("GUARD", keys=("V",)))


class MappingsTest(unittest.TestCase):
    def test_the_bundled_database_has_mappings_for_every_desktop(self):
        text = gamepad.DB.read_text()
        for system in ("Windows", "Mac OS X", "Linux"):
            self.assertIn(f"platform:{system},", text)
        rows = [r for r in text.splitlines() if r and not r.startswith("#")]
        self.assertGreater(len(rows), 1000)
        self.assertTrue(all(r.count(",") >= 3 for r in rows))
        self.assertTrue((gamepad.DB.parent / "LICENSE").is_file())

    def test_use_mappings_sets_the_sdl_hint(self):
        with mock.patch.dict(os.environ, {}, clear=True):
            self.assertTrue(gamepad.use_mappings())
            self.assertEqual(os.environ[gamepad.HINT], str(gamepad.DB.resolve()))

    def test_a_players_own_mapping_file_wins(self):
        with mock.patch.dict(os.environ, {gamepad.HINT: "/mine.txt"}, clear=True):
            gamepad.use_mappings()
            self.assertEqual(os.environ[gamepad.HINT], "/mine.txt")

    def test_missing_file_or_none_leaves_sdl_alone(self):
        with mock.patch.dict(os.environ, {}, clear=True):
            self.assertFalse(gamepad.use_mappings(None))
            with tempfile.TemporaryDirectory() as tmp:
                self.assertFalse(gamepad.use_mappings(Path(tmp) / "nope.txt"))
            self.assertNotIn(gamepad.HINT, os.environ)


class LayoutTest(unittest.TestCase):
    def test_xbox_layout(self):
        keys = gamepad.xbox(1)
        self.assertEqual(keys[A], (pyxel.GAMEPAD1_BUTTON_A,))
        self.assertEqual(keys[B], (pyxel.GAMEPAD1_BUTTON_B,))
        self.assertEqual(keys[C], (pyxel.GAMEPAD1_BUTTON_X,))
        self.assertEqual(keys[MENU], (pyxel.GAMEPAD1_BUTTON_START,))
        self.assertEqual(keys[UP], (pyxel.GAMEPAD1_BUTTON_DPAD_UP,))

    def test_second_pad_and_shift(self):
        keys = gamepad.xbox(2, shift=8)
        self.assertEqual(keys[A << 8], (pyxel.GAMEPAD2_BUTTON_A,))
        self.assertNotIn(A, keys)

    def test_layout_overrides_and_merge(self):
        keys = gamepad.xbox(1, {B: ("B", "Y"), MENU: ()})
        self.assertEqual(keys[B], (pyxel.GAMEPAD1_BUTTON_B, pyxel.GAMEPAD1_BUTTON_Y))
        self.assertNotIn(MENU, keys)
        merged = gamepad.merge({A: (1,)}, {A: (2, 1)}, {B: (3,)})
        self.assertEqual(merged, {A: (1, 2), B: (3,)})


class GameButtonsTest(unittest.TestCase):
    def test_keyboard_table(self):
        self.assertEqual(keys.bindings((DASH, GUARD)), {DASH.bit: (pyxel.KEY_SHIFT,), GUARD.bit: (pyxel.KEY_V,)})

    def test_pad_table(self):
        table = gamepad.xbox(1, buttons=(DASH, GUARD))
        self.assertEqual(table[DASH.bit], (pyxel.GAMEPAD1_BUTTON_RIGHTSHOULDER,))
        self.assertNotIn(GUARD.bit, table)
        self.assertEqual(table[A], (pyxel.GAMEPAD1_BUTTON_A,))
        self.assertEqual(gamepad.xbox(2, buttons=(DASH,))[DASH.bit], (pyxel.GAMEPAD2_BUTTON_RIGHTSHOULDER,))


class StickTest(unittest.TestCase):
    def axes(self, x, y):
        values = {pyxel.GAMEPAD1_AXIS_LEFTX: x, pyxel.GAMEPAD1_AXIS_LEFTY: y}
        return mock.patch.object(pyxel, "btnv", side_effect=lambda a: values.get(a, 0))

    def test_stick_pushes_are_directions(self):
        with self.axes(-30000, 0):
            self.assertEqual(gamepad.stick(1), LEFT)
        with self.axes(30000, 30000):
            self.assertEqual(gamepad.stick(1), RIGHT | DOWN)
        with self.axes(0, -32768):
            self.assertEqual(gamepad.stick(1, shift=8), UP << 8)

    def test_small_pushes_are_ignored(self):
        with self.axes(9000, -9000):
            self.assertEqual(gamepad.stick(1), 0)

    def test_read_sticks(self):
        with self.axes(30000, 0):
            self.assertEqual(gamepad.read_sticks(((1, 0), (1, 8))), RIGHT | RIGHT << 8)


if __name__ == "__main__":
    unittest.main()
