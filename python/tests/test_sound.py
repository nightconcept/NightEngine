import unittest

from nightengine import sound


class SoundTest(unittest.TestCase):
    def test_ch_joins_bars(self):
        self.assertEqual(sound.ch("t", "5", "n", 22, "c3 e3", "g3 r"), ("c3 e3 g3 r", "t", "5", "n", 22))

    def test_good_tables_have_no_errors(self):
        sfx = {"a": ("c3e3g3c4r", "p", "4", "n", 3), "b": ("c#4a#0", "s", "5", "n", 4)}
        music = {"m": [sound.ch("t", "5", "n", 20, "c3 r e3", "g3 a#2")]}
        self.assertEqual(sound.note_errors(sfx, music), [])

    def test_note_errors_catch_c5(self):
        errors = sound.note_errors({"boom": ("c3c5", "n", "7", "f", 4)}, {})
        self.assertEqual(errors, ["sfx boom: bad note 'c5'"])
        errors = sound.note_errors({}, {"m": [sound.ch("t", "5", "n", 20, "c3 c5")]})
        self.assertEqual(errors, ["music m channel 0: bad note 'c5'"])

    def test_note_errors_catch_junk(self):
        self.assertTrue(sound.note_errors({"x": ("c3zz", "n", "7", "f", 4)}, {}))
        self.assertTrue(sound.note_errors({}, {"m": [("c3 h3", "t", "5", "n", 4)]}))

    def test_slots_used(self):
        sfx = {"a": (), "b": ()}
        music = {"m1": [(), (), ()], "m2": [(), ()]}
        self.assertEqual(sound.slots_used(sfx, music), 7)
        self.assertLessEqual(sound.slots_used(sfx, music), sound.MAX_SOUNDS)


if __name__ == "__main__":
    unittest.main()
