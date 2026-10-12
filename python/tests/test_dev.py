"""Developer controls drive the host's normal simulation and recording path."""

import tempfile
import unittest
from pathlib import Path
from unittest import mock

import pyxel

from nightengine.host import platform
from nightengine.host.dev import Controls
from nightengine.trace import digest
from tests.test_platform import app_for


class ControlsTest(unittest.TestCase):
    def setUp(self):
        self.app = app_for()
        self.app._dev = Controls(self.app, inspect=lambda game: {"seed": game.seed}, snapshot=lambda game: [game.frame])
        self.dev = self.app.dev
        self.patches = (
            mock.patch.object(platform, "sample", return_value=()),
            mock.patch("nightengine.host.app.read_buttons", return_value=0),
            mock.patch.multiple(pyxel, create=True, btnp=lambda b: False, btn=lambda b: False, btnv=lambda a: 0),
        )
        for patch in self.patches:
            patch.start()
            self.addCleanup(patch.stop)

    def test_pause_step_and_resume(self):
        self.assertEqual(self.dev.pause(), 0)
        self.app.update()
        self.assertEqual(self.app.game.frame, 0)
        self.assertEqual(self.dev.step(2), 2)
        for _ in range(3):
            self.app.update()
        self.assertEqual(self.app.game.frame, 2)
        self.assertTrue(self.dev.state()["paused"])
        self.dev.resume()
        self.app.update()
        self.assertEqual(self.app.game.frame, 3)

    def test_held_input_is_recorded_for_exactly_requested_frames(self):
        self.dev.hold(16, 2)
        for _ in range(3):
            self.app.update()
        self.assertEqual(self.app.recording.frames, [16, 16, 0])
        self.assertEqual(self.app.game.tracker.previous, 0)

    def test_state_includes_game_inspection_and_snapshot_hash(self):
        self.assertEqual(
            self.dev.state(), {"frame": 0, "scene": "Log", "paused": False, "seed": 1, "hash": digest([0])}
        )
        self.assertEqual(self.dev.hash(), digest([0]))

    def test_invalid_controls_are_rejected(self):
        for frames in (0, -1, 1.5, True):
            with self.assertRaises(ValueError):
                self.dev.step(frames)
            with self.assertRaises(ValueError):
                self.dev.hold(0, frames)
        for code in (-1, self.app.game.input_mask + 1, 1.5):
            with self.assertRaises(ValueError):
                self.dev.hold(code)

    def test_screenshot_and_recording_save(self):
        with tempfile.TemporaryDirectory() as tmp:
            path = Path(tmp) / "frame.png"
            self.app.draw = mock.Mock()
            with mock.patch.object(pyxel, "screen", create=True) as screen:
                self.assertEqual(self.dev.shot(path), str(path))
                screen.save.assert_called_once_with(str(path), 1)
            self.app.draw.assert_called_once()
            self.app.update()
            saved = Path(tmp) / "recording.json"
            self.assertEqual(self.dev.save(saved), str(saved))
            self.assertTrue(saved.is_file())

    def test_replays_reject_input_injection(self):
        self.app.replay = self.app.recording
        with self.assertRaises(ValueError):
            self.dev.hold(0)


class PacedControlsTest(unittest.TestCase):
    def setUp(self):
        ControlsTest.setUp(self)
        from nightengine.pacing import InputLatch, Pacer

        self.app.vsync = True
        self.app.pacer, self.app.latch = Pacer(), InputLatch()
        self.app.renderer = mock.Mock(alpha=1.0, smooth=False)

    def test_holds_count_simulation_steps_in_a_multistep_display_frame(self):
        self.app.clock = mock.Mock(side_effect=[0.0, 1 / 30])
        self.dev.hold(16, 2)
        self.app.update()
        self.app.update()
        self.assertEqual(self.app.recording.frames, [16, 16, 0])

    def test_pause_and_manual_step_reset_display_catchup(self):
        self.app.clock = mock.Mock(side_effect=[0.0, 0.1])
        self.app.update()
        self.dev.pause()
        self.app.update()
        self.dev.step(2)
        for _ in range(3):
            self.app.update()
        self.assertEqual(self.app.game.frame, 3)
        self.dev.resume()
        self.app.update()
        self.assertEqual(self.app.game.frame, 4)
        self.assertEqual(self.app.clock.call_count, 2)

    def test_paused_screenshot_draws_current_state_without_a_paced_step(self):
        self.dev.pause()
        self.app.stepped = False
        with tempfile.TemporaryDirectory() as tmp, mock.patch.object(pyxel, "screen", create=True):
            self.dev.shot(Path(tmp) / "paused.png")
        self.app.renderer.draw.assert_called_once_with(self.app.game)
