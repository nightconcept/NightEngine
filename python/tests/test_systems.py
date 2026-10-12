import unittest

from nightengine import Systems


class Step:
    def __init__(self, log, name, result=None):
        self.log, self.name, self.result = log, name, result

    def update(self, scene, game, inp):
        self.log.append((self.name, scene, game, inp))
        return self.result


class SystemsTest(unittest.TestCase):
    def test_runs_in_order_and_passes_arguments(self):
        log = []
        systems = Systems(Step(log, "a"), Step(log, "b"), Step(log, "c", False))
        self.assertFalse(systems.update("scene", "game", "inp"))
        self.assertEqual(log, [(n, "scene", "game", "inp") for n in "abc"])

    def test_true_ends_the_frame_early(self):
        log = []
        systems = Systems(Step(log, "a"), Step(log, "b", True), Step(log, "c"))
        self.assertTrue(systems.update(None, None, None))
        self.assertEqual([entry[0] for entry in log], ["a", "b"])

    def test_len_and_iter(self):
        systems = Systems(Step([], "a"), Step([], "b"))
        self.assertEqual((len(systems), [s.name for s in systems]), (2, ["a", "b"]))


if __name__ == "__main__":
    unittest.main()
