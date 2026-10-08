"""Host events: text from outside the buttons that the recording keeps (saved settings, a key pressed to rebind).
Also the device in use, the first key pressed, and the volumes. No window opens and no sound plays."""

import json
import unittest
from types import SimpleNamespace
from unittest import mock

import pyxel

from nightengine import A, Driver, Game, Pointer, PointerPhase, Recording, Scene, Target, replay
from nightengine.host import keys, platform
from nightengine.host.app import App, AppConfig, device_of, split_keys
from nightengine.host.audio import AudioManager


class Log(Scene):
    def __init__(self):
        self.events = []

    def update(self, game, inp):
        self.events.append(inp.events)
        game.total = getattr(game, "total", 0) + sum(len(e) for e in inp.events) + game.rng.randint(0, 9)


def make_game(seed, target=None):
    game = Game(seed, target)
    game.scenes.replace(Log())
    return game


class RecordingEventsTest(unittest.TestCase):
    def test_events_round_trip_sparsely_and_play_feeds_them(self):
        rec = Recording(5)
        rec.add(0, (), ("profile {}",))
        rec.add(A)
        rec.add(0, (), ("press key:Z", "press pad:X"))
        data = json.loads(rec.to_json())
        self.assertEqual(data["events"], {"0": ["profile {}"], "2": ["press key:Z", "press pad:X"]})
        loaded = Recording.from_json(rec.to_json())
        self.assertEqual(loaded.events_at(2), ("press key:Z", "press pad:X"))
        self.assertEqual(loaded.events_at(1), ())
        game = replay.play(make_game, loaded)
        self.assertEqual(game.scene.events, [("profile {}",), (), ("press key:Z", "press pad:X")])
        self.assertEqual(game.total, replay.play(make_game, rec).total)

    def test_a_file_without_events_loads_and_writes_none(self):
        rec = Recording.from_json('{"seed": 1, "inputs": "0010"}')
        self.assertEqual((rec.frames, rec.events), ([0, 16], {}))
        self.assertNotIn("events", json.loads(rec.to_json()))


class DriverEventTest(unittest.TestCase):
    def test_event_reaches_input_events_for_one_frame(self):
        d = Driver(make_game(1))
        d.event("press key:SPACE")
        d.step(0, 2, events=("once",))
        self.assertEqual(d.game.scene.events, [("press key:SPACE",), ("once",), ()])

    def test_listen_starts_false(self):
        self.assertFalse(Game().listen)


class FirstPressedTest(unittest.TestCase):
    def pressed(self, *codes):
        down = set(codes)
        return mock.patch.object(pyxel, "btnp", side_effect=lambda k, *a, **kw: k in down)

    def test_names_drop_the_prefix(self):
        with self.pressed(pyxel.KEY_SPACE):
            self.assertEqual(keys.first_pressed(), "key:SPACE")
        with self.pressed(pyxel.GAMEPAD1_BUTTON_X):
            self.assertEqual(keys.first_pressed(), "pad:X")
        with self.pressed(pyxel.GAMEPAD2_BUTTON_X):
            self.assertIsNone(keys.first_pressed())
            self.assertEqual(keys.first_pressed(pads=(2,)), "pad:X")
        with self.pressed():
            self.assertIsNone(keys.first_pressed())

    def test_the_virtual_key_wins_over_its_sides(self):
        with self.pressed(pyxel.KEY_LSHIFT, pyxel.KEY_SHIFT):
            self.assertEqual(keys.first_pressed(), "key:SHIFT")
        self.assertNotIn("LSHIFT", keys.key_names())
        self.assertEqual(keys.key_names()[:4], ["SHIFT", "CTRL", "ALT", "GUI"])

    def test_is_pad(self):
        self.assertTrue(keys.is_pad(pyxel.GAMEPAD1_BUTTON_A))
        self.assertTrue(keys.is_pad(pyxel.GAMEPAD4_BUTTON_START))
        self.assertFalse(keys.is_pad(pyxel.KEY_A))


class DeviceTest(unittest.TestCase):
    def test_device_of(self):
        press = (Pointer(0, 1, 1, 1, 1, PointerPhase.PRESSED),)
        held = (Pointer(0, 1, 1, 1, 1, PointerPhase.HELD),)
        self.assertEqual(device_of("keyboard", 0, 0, ()), "keyboard")
        self.assertEqual(device_of("pad", 0, 0, held), "pad")
        self.assertEqual(device_of("keyboard", 0, A, ()), "pad")
        self.assertEqual(device_of("pad", A, 0, ()), "keyboard")
        self.assertEqual(device_of("pad", A, 0, press), "pointer")

    def test_split_keys(self):
        board, pad = split_keys({A: (pyxel.KEY_Z, pyxel.GAMEPAD1_BUTTON_A)})
        self.assertEqual((board, pad), ({A: (pyxel.KEY_Z,)}, {A: (pyxel.GAMEPAD1_BUTTON_A,)}))


def app_for(replay=None, keys_table=None):
    """An App with its fields set by hand: __init__ would open a window."""
    app = App.__new__(App)
    app.config = AppConfig("T", 64, 64, keys_table or {A: (pyxel.KEY_Z, pyxel.GAMEPAD1_BUTTON_A)}, replays="r")
    app.target = Target("desktop", 64, 64)
    platform.init(64, 64)
    app.audio = SimpleNamespace(update=lambda track, cues: None)
    app.game = make_game(1)
    app.replay = replay
    app.recording = None if replay else Recording(1)
    return app


def live(down=(), pressed=()):
    """Stub pyxel for one live frame: `down` codes are held, `pressed` codes were pressed on this frame."""
    return (
        mock.patch.object(platform, "sample", return_value=()),
        mock.patch.multiple(
            pyxel,
            create=True,
            btn=lambda k: k in down,
            btnp=lambda k, *a, **kw: k in pressed,
            btnv=lambda a: 0,
        ),
    )


class AppEventsTest(unittest.TestCase):
    def step(self, app, down=(), pressed=()):
        a, b = live(down, pressed)
        with a, b:
            app.update()

    def test_posted_and_listened_events_are_recorded_and_replayed(self):
        app = app_for()
        app.post("profile {}")
        self.step(app)
        app.game.listen = True
        self.step(app, pressed=(pyxel.KEY_SPACE,))
        app.game.listen = False
        self.step(app, pressed=(pyxel.KEY_SPACE,))
        log = [("profile {}",), ("press key:SPACE",), ()]
        self.assertEqual(app.game.scene.events, log)
        again = app_for(Recording.from_json(app.recording.to_json()))
        for _ in range(4):
            self.step(again)
        self.assertEqual(again.game.scene.events, log)

    def test_boot_events_go_into_frame_zero_of_a_live_run_only(self):
        calls = []

        class Booting(App):
            def boot(self):
                calls.append(1)
                return ["profile saved"]

        def build(replay=None):
            fake = SimpleNamespace(setup=lambda: None, update=lambda track, cues: None)
            config = AppConfig("T", 64, 64, {}, replays="r", pad_mappings=None)
            with (
                mock.patch.multiple(pyxel, create=True, init=mock.DEFAULT, run=mock.DEFAULT, rndi=lambda a, b: 3,
                                    integer_scale=mock.DEFAULT, mouse=mock.DEFAULT),
                mock.patch("atexit.register"),
            ):  # fmt: skip
                return Booting(config, make_game, fake, fake, replay=replay)

        app = build()
        self.step(app)
        self.step(app)
        self.assertEqual(app.game.scene.events, [("profile saved",), ()])
        self.assertEqual(app.recording.events, {0: ("profile saved",)})
        calls.clear()
        again = build(Recording.from_json(app.recording.to_json()))
        self.step(again)
        self.assertEqual((calls, again.game.scene.events), ([], [("profile saved",)]))

    def test_the_device_follows_the_input(self):
        app = app_for()
        self.assertEqual(app.device, "keyboard")
        self.step(app, down=(pyxel.GAMEPAD1_BUTTON_A,))
        self.assertEqual(app.device, "pad")
        self.step(app)
        self.assertEqual(app.device, "pad")
        self.step(app, down=(pyxel.KEY_Z,))
        self.assertEqual(app.device, "keyboard")
        self.assertEqual(app.recording.frames, [A, 0, A])


class VolumeTest(unittest.TestCase):
    def test_set_volume_scales_the_gains_after_setup(self):
        channels = [SimpleNamespace(gain=0.125) for _ in range(4)]
        with mock.patch.object(pyxel, "channels", channels, create=True):
            audio = AudioManager({}, {})
            audio.setup()
            audio.set_volume(0.5, 1.0)
            self.assertEqual([c.gain for c in channels], [0.0625, 0.0625, 0.0625, 0.125])
            audio.set_volume(0.0, 0.2)
            self.assertEqual([c.gain for c in channels], [0.0, 0.0, 0.0, 0.025])
            audio.set_volume(2.0, -1.0)  # Clamped to 0..1.
            self.assertEqual([c.gain for c in channels], [0.125, 0.125, 0.125, 0.0])

    def test_layers_are_effect_channels(self):
        channels = [SimpleNamespace(gain=1.0) for _ in range(4)]
        with mock.patch.object(pyxel, "channels", channels, create=True):
            audio = AudioManager({}, {}, priority=False, layers=2)
            audio.set_volume(0.0, 1.0)
        self.assertEqual([c.gain for c in channels], [0.0, 0.0, 1.0, 1.0])


if __name__ == "__main__":
    unittest.main()


class KeyTableTest(unittest.TestCase):
    def test_table_maps_names_to_pyxel_codes(self):
        from nightengine.bindings import Binding, Bindings

        b = Bindings({A: Binding(("Z", "RETURN"), ("A",))})
        self.assertEqual(keys.table(b), {A: (pyxel.KEY_Z, pyxel.KEY_RETURN, pyxel.GAMEPAD1_BUTTON_A)})
        self.assertEqual(keys.table(b, pad=2)[A][-1], pyxel.GAMEPAD2_BUTTON_A)
        with self.assertRaisesRegex(ValueError, "NOPE"):
            keys.table(Bindings({A: Binding(("NOPE",))}))

    def test_the_app_follows_the_game_bindings(self):
        from nightengine.bindings import Binding, Bindings

        app = app_for()
        self.assertIs(app.key_table(), app.config.keys)
        app.game.bindings = Bindings({A: Binding(("SPACE",), ("B",))})
        table = app.key_table()
        self.assertEqual(table, {A: (pyxel.KEY_SPACE, pyxel.GAMEPAD1_BUTTON_B)})
        self.assertIs(app.key_table(), table)  # Built once for one Bindings object.
        a, b = live(down=(pyxel.KEY_SPACE,))
        with a, b:
            app.update()
        self.assertEqual(app.recording.frames, [A])


class WritesTest(unittest.TestCase):
    def test_a_live_run_saves_the_writes_and_a_replay_does_not(self):
        from nightengine.host.app import drain_writes
        from nightengine.store import MemoryStore

        game, store = Game(), MemoryStore()
        game.write("save", "1")
        drain_writes(game, store, live=False)
        self.assertEqual((store.data, game.writes), ({}, []))
        game.write("save", "2")
        game.write("settings", "3")
        drain_writes(game, store, live=True)
        self.assertEqual((store.data, game.writes), ({"save": "2", "settings": "3"}, []))
        game.write("save", "4")
        drain_writes(game, None, live=True)
        self.assertEqual(game.writes, [])

    def test_the_app_drains_after_each_frame(self):
        from nightengine.store import MemoryStore

        app = app_for()
        app.store = MemoryStore()
        app.game.scene.update = lambda game, inp: game.write("save", str(game.frame))
        a, b = live()
        with a, b:
            app.update()
        self.assertEqual(app.store.data, {"save": "0"})
