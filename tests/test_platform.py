"""The host platform layer: the browser bridge, the desktop mouse, screen info, storage, and how App records
and replays pointers. Every pyxel and browser call is stubbed; no window opens."""

import json
import sys
import unittest
from types import SimpleNamespace
from unittest import mock

import pyxel

from nightengine import Game, Pointer, PointerPhase, Recording, Scene
from nightengine.host import platform
from nightengine.host.app import App, AppConfig

P = PointerPhase


class Bridge:
    """A stand-in for window.nightBridge (web/static/pointer.mjs)."""

    def __init__(self, rows=()):
        self.rows, self.calls, self.stored = list(rows), 0, {}

    def sample(self):
        self.calls += 1
        return json.dumps(self.rows)

    def info(self):
        return json.dumps(
            dict(
                touch=True,
                platform="android",
                viewport_width=800,
                viewport_height=360,
                orientation="LANDSCAPE",
                safe_area=dict(top=1, right=2, bottom=3, left=4),
            )
        )

    def save(self, key, data):
        self.stored[key] = data
        return True

    def load(self, key):
        return self.stored.get(key)

    backs = 0
    closed = False

    def takeBack(self):  # noqa: N802 (the JS name)
        if self.backs:
            self.backs -= 1
            return True
        return False

    def quit(self):
        self.closed = True
        return True


def in_browser(bridge):
    return mock.patch.dict(sys.modules, {"js": SimpleNamespace(window=SimpleNamespace(nightBridge=bridge))})


class BrowserTest(unittest.TestCase):
    def tearDown(self):
        platform.init(64, 64)

    def test_bridge_pointers_become_int_records_and_info_updates(self):
        row = dict(id=3, x=20.7, y=30.2, start_x=10.9, start_y=15.0, phase="MOVED")
        bridge = Bridge([row])
        with in_browser(bridge):
            platform.init(256, 144)
        self.assertTrue(platform.available())
        self.assertEqual(bridge.calls, 0)  # init never consumes input.
        (p,) = platform.sample()
        self.assertEqual((p.id, p.x, p.y, p.start_x, p.start_y, p.phase), (3, 20, 30, 10, 15, P.MOVED))
        self.assertEqual(platform.platform(), "android")
        self.assertTrue(platform.is_touch_device())
        self.assertEqual(platform.screen.safe_area.left, 4)
        self.assertEqual(platform.screen.viewport_width, 800)

    def test_back_is_b_for_one_frame_and_quit_asks_the_shell(self):
        from nightengine import B

        bridge = Bridge()
        bridge.backs = 1
        with in_browser(bridge):
            platform.init(64, 64)
            self.assertEqual([platform.buttons(), platform.buttons()], [B, 0])
            self.assertTrue(platform.quit_page())
        self.assertTrue(bridge.closed)

    def test_storage_round_trip_and_denied_read(self):
        with in_browser(Bridge()):
            platform.init(64, 64)
        self.assertTrue(platform.save("replay", "{}"))
        self.assertEqual(platform.load("replay"), "{}")
        self.assertIsNone(platform.load("missing"))

        def denied(key):
            raise RuntimeError("blocked")

        with in_browser(SimpleNamespace(load=denied, info=Bridge().info)):
            platform.init(64, 64)
        with self.assertRaisesRegex(OSError, "Allow site storage"):
            platform.load("replay")


class DesktopTest(unittest.TestCase):
    def mouse(self, x, y, down, pressed=False):
        return mock.patch.multiple(
            pyxel,
            create=True,
            mouse_x=x,
            mouse_y=y,
            MOUSE_BUTTON_LEFT=0,
            btn=lambda b: down,
            btnp=lambda b: pressed,
        )

    def test_desktop_has_no_page_buttons_or_shell(self):
        platform.init(64, 64)
        self.assertEqual((platform.buttons(), platform.quit_page()), (0, False))

    def test_desktop_fallback_has_no_bridge_or_storage(self):
        platform.init(64, 64)
        self.assertFalse(platform.available())
        self.assertFalse(platform.save("k", "v"))
        self.assertIsNone(platform.load("k"))
        self.assertEqual(platform.platform(), "desktop")
        with self.assertRaises(ValueError):
            platform.init(0, 10)

    def test_mouse_is_pointer_zero_through_a_whole_click(self):
        platform.init(64, 64)
        phases = []
        for x, y, down, pressed in ((5, 5, False, False), (5, 5, True, True), (5, 5, True, False),
                                    (9, 6, True, False), (99, -3, False, False), (9, 6, False, False)):  # fmt: skip
            with self.mouse(x, y, down, pressed):
                phases.append([(p.id, p.x, p.y, p.start_x, p.phase) for p in platform.sample()])
        self.assertEqual(
            phases,
            [[], [(0, 5, 5, 5, P.PRESSED)], [(0, 5, 5, 5, P.HELD)], [(0, 9, 6, 5, P.MOVED)],
             [(0, 63, 0, 5, P.RELEASED)], []],
        )  # fmt: skip


class Log(Scene):
    def __init__(self):
        self.seen = []

    def update(self, game, inp):
        self.seen.append(tuple((p.x, p.phase) for p in inp.pointers))


def app_for(replay=None):
    """An App with its fields set by hand: __init__ would open a window."""
    app = App.__new__(App)
    app.config = AppConfig("T", 64, 64, {}, replays="r")
    app.audio = SimpleNamespace(update=lambda track, cues: None)
    app.game = Game(1)
    app.log = Log()
    app.game.push(app.log)
    app.replay = replay
    app.recording = None if replay else Recording(1)
    return app


class AppTest(unittest.TestCase):
    def test_live_run_records_pointers_and_replay_feeds_them_back(self):
        script = [(), ((7, P.PRESSED),), ((7, P.RELEASED),), ()]
        frames = [tuple(Pointer(0, x, 3, x, 3, ph) for x, ph in f) for f in script]
        app = app_for()
        with (
            mock.patch.object(platform, "sample", side_effect=frames),
            mock.patch("nightengine.host.app.read_buttons", return_value=0),
            mock.patch.multiple(pyxel, create=True, btnp=lambda b: False, btn=lambda b: False),
        ):
            for _ in frames:
                app.update()
        self.assertEqual(sorted(app.recording.pointers), [1, 2])
        self.assertEqual(app.log.seen, script)
        replayed = app_for(Recording.from_json(app.recording.to_json()))
        with mock.patch.multiple(pyxel, create=True, btnp=lambda b: False, btn=lambda b: False):
            for _ in range(len(frames) + 2):  # Past the end: the replay stops.
                replayed.update()
        self.assertEqual(replayed.log.seen, script)

    def test_a_quit_request_saves_and_quits(self):
        app = app_for()
        app.log.update = lambda game, inp: setattr(game, "quit_requested", True)
        with (
            mock.patch.object(platform, "sample", return_value=()),
            mock.patch("nightengine.host.app.read_buttons", return_value=0),
            mock.patch.multiple(pyxel, create=True, btnp=lambda b: False, btn=lambda b: False, quit=mock.DEFAULT),
            mock.patch.object(App, "save") as save,
        ):
            app.update()
            pyxel.quit.assert_called_once()
        save.assert_called_once()

    def test_a_quit_in_a_page_asks_the_shell_and_keeps_running(self):
        app = app_for()
        app.log.update = lambda game, inp: setattr(game, "quit_requested", True)
        with (
            mock.patch.object(platform, "available", return_value=True),
            mock.patch.object(platform, "quit_page") as quit_page,
            mock.patch.object(platform, "buttons", return_value=0),
            mock.patch.object(platform, "sample", return_value=()),
            mock.patch("nightengine.host.app.read_buttons", return_value=0),
            mock.patch.multiple(pyxel, create=True, btnp=lambda b: False, btn=lambda b: False, quit=mock.DEFAULT),
            mock.patch.object(App, "save"),
        ):
            app.update()
            pyxel.quit.assert_not_called()
        quit_page.assert_called_once()
        self.assertFalse(app.game.quit_requested)


if __name__ == "__main__":
    unittest.main()
