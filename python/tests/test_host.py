"""Host tests. They import the pyxel modules but never open a window: every pyxel call is stubbed."""

import contextlib
import importlib
import unittest
from unittest import mock

import pyxel

from nightengine import DOWN, UP, A, Canvas, Game, Scene
from nightengine.host import assets, ui
from nightengine.host.app import App, AppConfig
from nightengine.host.audio import AudioManager
from nightengine.host.keys import read_buttons
from nightengine.host.renderer import Renderer, no_shake, shake_x, shake_xy
from nightengine.pacing import InputLatch, Pacer
from nightengine.pointer import Pointer, PointerPhase
from nightengine.replay import Recording


def stub(*names, **values):
    """Patch pyxel attributes with mocks (or values). Works before pyxel.init."""
    stack = contextlib.ExitStack()
    mocks = {n: stack.enter_context(mock.patch.object(pyxel, n, create=True)) for n in names}
    for n, v in values.items():
        stack.enter_context(mock.patch.object(pyxel, n, v, create=True))
    return stack, mocks


class ImportTest(unittest.TestCase):
    def test_every_host_module_imports(self):
        for name in ("keys", "gamepad", "assets", "ui", "audio", "renderer", "app", "frames", "platform", "diagnostic"):
            importlib.import_module(f"nightengine.host.{name}")


class KeysTest(unittest.TestCase):
    def test_read_buttons_combines_bindings(self):
        keys = {UP: (1, 2), DOWN: (3,), A: (4, 5)}
        down = {2, 4}
        with mock.patch.object(pyxel, "btn", side_effect=lambda k: k in down):
            self.assertEqual(read_buttons(keys), UP | A)
        down.clear()
        with mock.patch.object(pyxel, "btn", side_effect=lambda k: k in down):
            self.assertEqual(read_buttons(keys), 0)


class FakeImage:
    def __init__(self):
        self.pixels = {}

    def pset(self, x, y, c):
        self.pixels[(x, y)] = c


class AssetsTest(unittest.TestCase):
    def test_shelf_packs_left_to_right_and_wraps_rows(self):
        banks = [FakeImage()]
        with mock.patch.object(pyxel, "images", banks, create=True):
            shelf = assets.Shelf(0, v=10, width=20)
            a = shelf.add(Canvas(8, 8, 3))
            b = shelf.add(Canvas(8, 4, 4))
            c = shelf.add(Canvas(8, 6, 5))  # 16 + 8 > 20: wraps below the tallest piece.
            d = shelf.add(Canvas(4, 4, 6))
        self.assertEqual(a, assets.Region(0, 0, 10, 8, 8))
        self.assertEqual(b, assets.Region(0, 8, 10, 8, 4))
        self.assertEqual(c, assets.Region(0, 0, 18, 8, 6))
        self.assertEqual(d, assets.Region(0, 8, 18, 4, 4))
        self.assertEqual(banks[0].pixels[(0, 10)], 3)
        self.assertEqual(banks[0].pixels[(8, 10)], 4)

    def test_pack_and_pack_variants_keep_names_and_order(self):
        with mock.patch.object(pyxel, "images", [FakeImage()], create=True):
            shelf = assets.Shelf(0)
            regions = assets.pack({"ship": Canvas(16, 16), "gem": Canvas(8, 8)}, shelf)
            variants = assets.pack_variants({"grass": [Canvas(8, 8), Canvas(8, 8)]}, shelf)
        self.assertEqual(list(regions), ["ship", "gem"])
        self.assertEqual(regions["gem"].u, 16)
        self.assertEqual([r.u for r in variants["grass"]], [24, 32])


class UiTest(unittest.TestCase):
    def setUp(self):
        self.stack, self.m = stub("text", "blt", "pal", "dither", "rect", Font=mock.MagicMock(), width=100, height=60)
        self.stack.__enter__()
        ui.setup("font.bdf", 7, 1)
        self.region = assets.Region(1, 8, 16, 10, 12)

    def tearDown(self):
        self.stack.__exit__(None, None, None)

    def test_text_uses_default_colors_and_shadow(self):
        ui.text(5, 6, "hi")
        self.assertEqual([c.args[:4] for c in self.m["text"].call_args_list], [(6, 7, "hi", 1), (5, 6, "hi", 7)])
        self.m["text"].reset_mock()
        ui.text(5, 6, "hi", col=9, shadow=None)
        self.assertEqual(self.m["text"].call_count, 1)
        self.assertEqual(self.m["text"].call_args.args[3], 9)

    def test_center_uses_cx_or_the_screen(self):
        with mock.patch.object(ui, "width", return_value=20):
            ui.center(0, "x")
            self.assertEqual(self.m["text"].call_args.args[0], 40)
            ui.center(0, "x", cx=30)
            self.assertEqual(self.m["text"].call_args.args[0], 20)

    def test_wrap(self):
        with mock.patch.object(ui, "width", side_effect=len):
            self.assertEqual(ui.wrap("aa bb cc\ndd", 5), ["aa bb", "cc", "dd"])

    def test_the_three_anchors(self):
        ui.blt(self.region, 20, 30)
        self.assertEqual(self.m["blt"].call_args.args[:3], (20, 30, 1))
        ui.blt_center(self.region, 20, 30)
        self.assertEqual(self.m["blt"].call_args.args[:2], (15, 24))
        ui.blt_feet(self.region, 20, 30)
        self.assertEqual(self.m["blt"].call_args.args[:2], (15, 18))

    def test_flip_makes_the_width_negative(self):
        ui.blt(self.region, 0, 0, flip=True)
        self.assertEqual(self.m["blt"].call_args.args[5], -10)
        ui.blt_center(self.region, 0, 0, flip=True, scale=2)
        self.assertEqual(self.m["blt"].call_args.args[5], -10)
        self.assertEqual(self.m["blt"].call_args.kwargs, {"scale": 2})

    def test_solid_anchor_and_alpha(self):
        ui.solid(self.region, 20, 30, 5, anchor="feet", alpha=0.5)
        self.assertEqual(self.m["blt"].call_args.args[:2], (15, 18))
        self.assertEqual(self.m["pal"].call_count, 33)  # 32 remaps and one reset.
        self.assertEqual([c.args for c in self.m["dither"].call_args_list], [(0.5,), (1,)])
        self.m["dither"].reset_mock()
        ui.solid(self.region, 20, 30, 5)
        self.assertEqual(self.m["dither"].call_count, 0)
        self.assertEqual(self.m["blt"].call_args.args[:2], (15, 24))

    def test_fade(self):
        ui.fade(0)
        self.assertEqual(self.m["rect"].call_count, 0)
        ui.fade(2, col=3)
        self.assertEqual(self.m["rect"].call_args.args, (0, 0, 100, 60, 3))
        self.assertEqual(self.m["dither"].call_args_list[0].args, (1.0,))

    def test_big_text_draws_into_the_scratch_row(self):
        img = mock.MagicMock()
        ui.setup("font.bdf", 7, 1, scratch=(2, 224))
        with (
            mock.patch.object(pyxel, "images", [None, None, img], create=True),
            mock.patch.object(ui, "width", return_value=10),
        ):
            ui.big_text("hi", 50, 20, 2, 7, 1)
        img.rect.assert_called_once_with(0, 224, 256, 14, 0)
        self.assertEqual(self.m["blt"].call_args.args[2:5], (2, 0, 224))


class A1(Scene):
    pass


class A2(A1):
    pass


class Over(Scene):
    overlay = True


class RendererTest(unittest.TestCase):
    def make(self, **kw):
        self.log = []
        table = {
            A1: lambda s, g, t: self.log.append(("a1", t)),
            Over: lambda s, g, t: self.log.append(("over", t)),
        }
        return Renderer(table, (0, 0xFFFFFF), lambda: self.log.append("bake"), "font.bdf", (7, 1), **kw)

    def draw(self, renderer, game):
        stack, m = stub("camera", "dither", "rect", width=10, height=10)
        with stack:
            renderer.draw(game)
        return [c.args for c in m["camera"].call_args_list], m

    def test_draws_visible_scenes_bottom_up_and_resets_camera(self):
        game = Game()
        game.scenes.replace(A1(), A1(), Over())
        game.frame = 5
        cameras, _ = self.draw(self.make(), game)
        self.assertEqual(self.log, [("a1", 5), ("over", 5)])
        self.assertEqual(cameras, [(0, 0), ()])

    def test_subclass_uses_the_base_class_draw_function(self):
        game = Game()
        game.scenes.replace(A2())
        self.draw(self.make(), game)
        self.assertEqual(self.log, [("a1", 0)])

    def test_unknown_scene_names_the_type(self):
        game = Game()
        game.scenes.replace(Scene())
        with self.assertRaisesRegex(KeyError, "Scene"):
            self.draw(self.make(), game)

    def test_on_registers_a_draw_function_and_the_table_is_shared(self):
        table = {}
        renderer = Renderer(table, (), lambda: None, "f", (1, 2))

        @renderer.on(A1)
        def draw_a1(scene, game, t):
            self.log = ["drawn"]

        self.assertIs(renderer.table, table)
        self.assertIn(A1, table)
        table[Over] = lambda s, g, t: None  # A game may add to its table after building the renderer.
        self.assertIn(Over, renderer.table)
        self.assertIs(draw_a1, table[A1])

    def test_shake_formulas(self):
        game = Game()
        game.fx.shake = 5  # y needs a strong shake, above 10.
        self.assertEqual([shake_xy(game, t) for t in range(4)], [(-2, 0), (-1, 0), (0, 0), (1, 0)])
        game.fx.shake = 11
        self.assertEqual([shake_xy(game, t) for t in range(4)], [(-2, -1), (-1, 0), (0, 1), (1, -1)])
        game.fx.shake = 5
        self.assertEqual([shake_x(game, t) for t in range(4)], [(-2, 0), (-1, 0), (0, 0), (1, 0)])
        game.fx.shake = 0
        self.assertEqual((shake_xy(game, 1), shake_x(game, 1), no_shake(game, 1)), ((0, 0),) * 3)

    def test_draw_applies_the_shake_function(self):
        game = Game()
        game.scenes.replace(A1())
        game.fx.shake = 11
        game.frame = 2
        cameras, _ = self.draw(self.make(shake=shake_xy), game)
        self.assertEqual(cameras[0], (0, 1))

    def test_fade_is_drawn_last_only_when_asked(self):
        game = Game()
        game.scenes.replace(A1())
        game.fx.fade = 0.5
        _, m = self.draw(self.make(fade=True), game)
        self.assertEqual(m["dither"].call_args_list[0].args, (0.5,))
        _, m = self.draw(self.make(), game)
        self.assertEqual(m["rect"].call_count, 0)

    def test_setup_sets_palette_bakes_and_loads_the_font(self):
        renderer = self.make(scratch=(2, 224))
        stack, m = stub(Font=mock.MagicMock())
        colors = []
        with stack, mock.patch.object(pyxel, "colors", colors, create=True):
            renderer.setup()
        self.assertEqual((colors, self.log), ([0, 0xFFFFFF], ["bake"]))
        self.assertEqual(ui._scratch, (2, 224))


SFX = {"boom": ("c2", "n", "7", "f", 4), "hit": ("c4", "n", "3", "f", 2), "shot": ("a4", "n", "2", "f", 1)}
MUSIC = {"field": [("c3 e3", "t", "5", "n", 20), ("c2 r", "t", "5", "n", 20)], "win": [("c3", "t", "5", "n", 20)]}


class AudioTest(unittest.TestCase):
    def make(self, **kw):
        manager = AudioManager(SFX, MUSIC, once=("win",), **kw)
        self.sounds = mock.MagicMock()
        self.musics = mock.MagicMock()
        stack, self.m = stub("play", "playm", "stop", "play_pos")
        stack.enter_context(mock.patch.object(pyxel, "sounds", self.sounds, create=True))
        stack.enter_context(mock.patch.object(pyxel, "musics", self.musics, create=True))
        self.addCleanup(stack.close)
        manager.setup()
        return manager

    def played(self):
        return [c.args for c in self.m["play"].call_args_list]

    def test_setup_assigns_slots_in_order(self):
        manager = self.make()
        self.assertEqual(manager.sfx_ids, {"boom": 0, "hit": 1, "shot": 2})
        self.assertEqual(manager.music_ids, {"field": 0, "win": 1})
        self.sounds.__getitem__.assert_any_call(4)  # The last music channel.
        self.sounds.__getitem__.return_value.set.assert_any_call("c3e3", "t", "5", "n", 20)
        self.musics.__getitem__.return_value.set.assert_any_call([3], [4])

    def test_tracks_loop_unless_once_and_changes_stop_the_music_channels(self):
        manager = self.make()
        manager.update("field", [])
        manager.update("field", [])
        manager.update("win", [])
        self.assertEqual([c.args for c in self.m["playm"].call_args_list], [(0,), (1,)])
        self.assertEqual([c.kwargs for c in self.m["playm"].call_args_list], [{"loop": True}, {"loop": False}])
        self.assertEqual(self.m["stop"].call_count, 6)  # Channels 0 to 2, on each of two changes.

    def test_priority_plays_the_most_important_cue(self):
        manager = self.make(priority=True)
        self.m["play_pos"].return_value = None
        manager.update(None, ["shot", "boom", "nope", "hit"])
        self.assertEqual(self.played(), [(3, 0)])

    def test_priority_skips_a_cue_under_a_more_important_effect_that_still_plays(self):
        manager = self.make(priority=True)
        self.m["play_pos"].return_value = None
        manager.update(None, ["boom"])
        self.m["play_pos"].return_value = (3, 1)  # Still playing.
        manager.update(None, ["shot"])
        self.assertEqual(self.played(), [(3, 0)])
        manager.update(None, ["boom"])  # An equal or higher priority cue may restart.
        self.assertEqual(len(self.played()), 2)
        self.m["play_pos"].return_value = None  # Finished: a low priority cue plays again.
        manager.update(None, ["shot"])
        self.assertEqual(self.played()[-1], (3, 2))

    def test_without_priority_only_the_first_cue_plays_and_nothing_is_checked(self):
        manager = self.make(priority=False)
        self.m["play_pos"].return_value = (3, 1)
        manager.update(None, ["shot", "boom"])
        manager.update(None, ["nope", "boom"])  # The first cue is unknown, so nothing plays.
        manager.update(None, ["boom"])
        self.m["play_pos"].assert_not_called()
        self.assertEqual(self.played(), [(3, 2), (3, 0)])

    def test_layers_play_different_cues_together_on_their_own_channels(self):
        manager = self.make(priority=False, layers=2)
        manager.update(None, ["shot", "nope", "shot", "boom", "hit"])
        self.assertEqual(self.played(), [(3, 2), (2, 0)])
        with self.assertRaises(ValueError):
            AudioManager(SFX, MUSIC, layers=2)  # priority is on by default.
        with self.assertRaises(ValueError):
            AudioManager(SFX, MUSIC, priority=False, layers=5)


class AppConfigTest(unittest.TestCase):
    def test_defaults(self):
        config = AppConfig("T", 192, 256, {}, replays="r")
        self.assertEqual((config.fps, config.label_xy, config.integer_scale), (60, (4, 4), True))

    def test_vsync_is_off_by_default(self):
        self.assertFalse(AppConfig("T", 192, 256, {}, replays="r").vsync)


class FakeClock:
    def __init__(self, hz: float):
        self.dt, self.now = 1 / hz, -1 / hz

    def __call__(self) -> float:
        self.now += self.dt
        return self.now


def display_app(hz: float = 120, replay: Recording | None = None) -> App:
    """An App in display mode with no window: the game, the renderer, the audio, and the input are fakes."""
    app = App.__new__(App)
    app.config = AppConfig("T", 192, 256, {}, replays="r")
    app.game = mock.MagicMock(writes=[], quit_requested=False, frame=0)
    app.renderer, app.audio, app.replay = mock.MagicMock(alpha=1.0, smooth=False), mock.MagicMock(), replay
    app.recording = None if replay else Recording(1)
    app.pacer, app.latch, app.clock, app.vsync = Pacer(), InputLatch(), FakeClock(hz), True
    app.fit = mock.MagicMock()
    app.inputs = []  # What read_input gives on each display frame.
    app.read_input = lambda: app.inputs.pop(0) if app.inputs else (0, (), ())
    return app


def touch(phase: PointerPhase) -> Pointer:
    return Pointer(1, 5, 6, 5, 6, phase)


class DisplayModeTest(unittest.TestCase):
    def setUp(self):
        self.stack, _ = stub(btnp=lambda *a: False, btn=lambda *a: False)
        self.stack.__enter__()

    def tearDown(self):
        self.stack.__exit__(None, None, None)

    def frames(self, app: App, n: int) -> list[float]:
        alphas = []
        for _ in range(n):
            app.update()
            alphas.append(app.renderer.alpha)
        return alphas

    def test_120_hz_steps_every_second_frame_and_records_one_input_for_each_step(self):
        app = display_app(120)
        alphas = self.frames(app, 5)
        self.assertEqual(alphas, [1.0, 0.5, 1.0, 0.5, 1.0])
        self.assertEqual(app.game.step.call_count, 3)
        self.assertEqual(len(app.recording.frames), 3)

    def test_a_key_held_only_in_a_frame_with_no_step_is_recorded(self):
        app = display_app(120)
        app.inputs = [(0, (), ()), (0, (), ()), (A, (), ("press key:Z",)), (0, (), ())]
        self.frames(app, 4)  # Steps on frames 1, 2, and 4; frame 3 runs none.
        self.assertEqual(app.recording.frames, [0, 0, A])
        self.assertEqual(app.recording.events_at(2), ("press key:Z",))

    def test_later_steps_in_one_frame_get_the_same_buttons_held_pointers_and_no_events(self):
        app = display_app(30)  # Two steps in each display frame.
        app.inputs = [(0, (), ()), (A, (touch(PointerPhase.PRESSED),), ("e",))]
        self.frames(app, 2)
        first, second = app.game.step.call_args_list[-2:]
        self.assertEqual(first.args, (A, (touch(PointerPhase.PRESSED),), ("e",)))
        self.assertEqual(second.args, (A, (touch(PointerPhase.HELD),), ()))

    def test_a_replay_plays_one_recorded_frame_for_each_step(self):
        replay = Recording(1)
        for code in (1, 2, 4):
            replay.add(code)
        app = display_app(120, replay)
        app.game.frame = 0

        def step(code, pointers, events):
            app.game.frame += 1

        app.game.step.side_effect = step
        self.frames(app, 5)
        self.assertEqual([c.args[0] for c in app.game.step.call_args_list], [1, 2, 4])

    def test_a_frame_with_no_step_skips_the_draw_unless_the_renderer_blends(self):
        app = display_app(120)
        self.frames(app, 3)  # The third frame runs no step.
        app.draw()
        app.renderer.draw.assert_not_called()
        app.renderer.smooth = True
        app.draw()
        app.renderer.draw.assert_called_once()

    def test_set_vsync_does_nothing_without_the_fork(self):
        app = display_app()
        app.vsync = False
        with mock.patch.object(pyxel, "NE_PACING", False, create=True):
            app.set_vsync(True)
        self.assertFalse(app.vsync)

    def test_set_vsync_stays_off_when_the_driver_refuses(self):
        app = display_app()
        with stub(NE_PACING=True, vsync=lambda on: 0)[0]:
            app.set_vsync(True)
        self.assertFalse(app.vsync)
        with stub(NE_PACING=True, vsync=lambda on: -1)[0]:
            app.set_vsync(True)
        self.assertTrue(app.vsync)


if __name__ == "__main__":
    unittest.main()
