import unittest

from nightengine import UP, A, Game, Input, Scene, SceneStack


class Base(Scene):
    pass


class Overlay(Scene):
    overlay = True


class Other(Base):
    pass


class SceneStackTest(unittest.TestCase):
    def test_list_like_access(self):
        a, b = Base(), Overlay()
        stack = SceneStack(a)
        stack.push(b)
        self.assertEqual((stack[-1], stack[-2], stack.top), (b, a, b))
        self.assertEqual((len(stack), stack.index(b), list(stack)), (2, 1, [a, b]))
        self.assertIn(a, stack)

    def test_pop_removes_from_anywhere_and_ignores_missing(self):
        a, b, c = Base(), Base(), Base()
        stack = SceneStack(a, b, c)
        stack.pop(b)
        stack.pop(b)
        self.assertEqual(list(stack), [a, c])

    def test_replace_keeps_the_same_object(self):
        stack = SceneStack(Base())
        items = stack._items
        new = Base()
        stack.replace(new)
        self.assertIs(stack._items, items)
        self.assertEqual(list(stack), [new])

    def test_find_returns_the_topmost_instance(self):
        a, b, o = Base(), Other(), Overlay()
        stack = SceneStack(a, b, o)
        self.assertIs(stack.find(Base), b)
        self.assertIs(stack.find(Other), b)
        self.assertIsNone(SceneStack(a).find(Other))

    def test_visible_goes_from_the_topmost_opaque_scene(self):
        a, b, o1, o2 = Base(), Base(), Overlay(), Overlay()
        self.assertEqual(SceneStack(a, b, o1, o2).visible(), [b, o1, o2])
        self.assertEqual(SceneStack(a, b).visible(), [b])
        self.assertEqual(SceneStack(o1, o2).visible(), [o1, o2])
        self.assertEqual(SceneStack().visible(), [])


class Recorder(Scene):
    def __init__(self, log):
        self.log = log

    def update(self, game, inp):
        self.log.append(("scene", game.frame, inp.held, game.fx.shake))
        game.cue("hit")


class Ordered(Game):
    input_mask = UP | A

    def __init__(self, log):
        super().__init__(5)
        self.log = log
        self.scenes.replace(Recorder(log))

    def before_scene(self, inp: Input):
        self.log.append(("before", self.frame))

    def after_scene(self):
        self.log.append(("after", self.frame))


class GameTest(unittest.TestCase):
    def test_step_order_and_cues(self):
        log = []
        game = Ordered(log)
        game.fx.shake = 2
        cues = game.step(UP | 0x80)
        self.assertEqual(cues, ["hit"])
        self.assertEqual(log, [("before", 0), ("scene", 0, UP, 1), ("after", 0)])
        self.assertEqual(game.frame, 1)
        self.assertEqual(game.step(0), ["hit"])  # Cues are cleared each frame.

    def test_fade_in_rule(self):
        game = Ordered([])
        game.fx.fade_in = 10
        game.step(0)
        self.assertEqual((game.fx.fade_in, game.fx.fade), (9, 0.45))

    def test_rng_is_seeded(self):
        self.assertEqual(Game(3).rng.random(), Game(3).rng.random())

    def test_scene_music_and_push_pop(self):
        class Tune(Scene):
            def music(self, game):
                return "tune"

        game = Game()
        game.scenes.replace(Base())
        t = Tune()
        game.push(t)
        self.assertEqual((game.scene, game.music), (t, "tune"))
        game.pop(t)
        self.assertIsNone(game.music)

    def test_repeat_configuration_reaches_the_tracker(self):
        class Menu(Game):
            repeat_buttons = (UP,)
            repeat_delay = 3
            repeat_rate = 2

        t = Menu().tracker
        self.assertEqual([t.feed(UP).nav(UP) for _ in range(6)], [True, False, True, False, True, False])


if __name__ == "__main__":
    unittest.main()
