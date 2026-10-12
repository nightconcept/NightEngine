"""Saved files and runtime bindings. The tests use a temp folder, never the user's data folder."""

import json
import tempfile
import unittest
from pathlib import Path

from nightengine import DOWN, MENU, UP, A, B, Game
from nightengine.bindings import Binding, Bindings
from nightengine.store import FileStore, MemoryStore, load_json


class FileStoreTest(unittest.TestCase):
    def test_write_makes_the_folder_and_reads_back(self):
        with tempfile.TemporaryDirectory() as tmp:
            store = FileStore(Path(tmp) / "Vendor" / "Game")
            self.assertIsNone(store.read("settings"))
            store.write("settings", '{"music": 7}')
            store.write("settings", '{"music": 3}')
            self.assertEqual(store.read("settings"), '{"music": 3}')
            self.assertEqual(sorted(p.name for p in store.folder.iterdir()), ["settings.json"])

    def test_a_bad_file_gives_the_default_and_is_moved_aside(self):
        with tempfile.TemporaryDirectory() as tmp:
            store = FileStore(tmp)
            store.write("save", "{not json")
            self.assertEqual(load_json(store, "save", {"a": 1}), {"a": 1})
            self.assertIsNone(store.read("save"))
            self.assertEqual((Path(tmp) / "save.bad.json").read_text(), "{not json")

    def test_load_json(self):
        store = MemoryStore({"ok": '{"x": 2}', "null": "null", "bad": "]"})
        self.assertEqual(load_json(store, "ok"), {"x": 2})
        self.assertEqual(load_json(store, "missing", 5), 5)
        self.assertEqual(load_json(store, "null", 6), 6)
        self.assertEqual(load_json(None, "ok", 7), 7)
        self.assertEqual(load_json(store, "bad", 8), 8)
        self.assertEqual(store.data["bad.bad"], "]")
        self.assertNotIn("bad", store.data)


class GameWriteTest(unittest.TestCase):
    def test_write_queues_files_and_bindings_start_none(self):
        game = Game()
        game.write("save", "{}")
        self.assertEqual(game.writes, [("save", "{}")])
        self.assertIsNone(game.bindings)


SHOOT, BACK = A, B
DEFAULTS = Bindings(
    {
        UP: Binding(("UP",), ("DPAD_UP",)),
        DOWN: Binding(("DOWN",), ("DPAD_DOWN",)),
        SHOOT: Binding(("Z", "RETURN"), ("A",)),
        BACK: Binding(("X", "BACKSPACE"), ("B",)),
        MENU: Binding(("ESCAPE",), ("START",)),
    },
    locked={(MENU, "key", "ESCAPE")},
    reserved={("key", "Q"), ("key", "UP"), ("key", "DOWN")},
)


class BindingsTest(unittest.TestCase):
    def test_assign_a_free_name(self):
        b = DEFAULTS.assign(SHOOT, "key", "SPACE")
        self.assertEqual(b.table[SHOOT], Binding(("SPACE", "RETURN"), ("A",)))
        self.assertEqual(DEFAULTS.primary(SHOOT, "key"), "Z")  # Frozen: the old table did not change.

    def test_a_taken_name_trades(self):
        b = DEFAULTS.assign(SHOOT, "key", "X")
        self.assertEqual((b.primary(SHOOT, "key"), b.primary(BACK, "key")), ("X", "Z"))
        self.assertEqual(b.table[BACK].keys, ("Z", "BACKSPACE"))
        p = DEFAULTS.assign(SHOOT, "pad", "B")
        self.assertEqual((p.primary(SHOOT, "pad"), p.primary(BACK, "pad")), ("B", "A"))

    def test_the_same_name_changes_nothing(self):
        self.assertIs(DEFAULTS.assign(SHOOT, "key", "Z"), DEFAULTS)

    def test_refusals(self):
        self.assertIsNone(DEFAULTS.assign(SHOOT, "key", "Q"))  # Reserved.
        self.assertIsNone(DEFAULTS.assign(SHOOT, "key", "UP"))  # Reserved.
        self.assertIsNone(DEFAULTS.assign(SHOOT, "key", "BACKSPACE"))  # Another action's alias.
        self.assertIsNone(DEFAULTS.assign(MENU, "key", "P"))  # The locked entry would move.
        self.assertIsNone(DEFAULTS.assign(SHOOT, "key", "ESCAPE"))  # A trade would move the locked entry.
        self.assertIsNotNone(DEFAULTS.assign(MENU, "pad", "BACK"))  # Only the key is locked.
        self.assertIsNone(DEFAULTS.assign(SHOOT, "mouse", "LEFT"))
        self.assertIsNone(DEFAULTS.assign(1 << 20, "key", "P"))

    def test_json_round_trip_keeps_primaries_and_takes_aliases_from_the_defaults(self):
        b = DEFAULTS.assign(SHOOT, "key", "X").assign(BACK, "pad", "Y")
        data = json.loads(json.dumps(b.to_json()))
        self.assertEqual(data[str(SHOOT)], {"key": "X", "pad": "A"})
        self.assertEqual(Bindings.from_json(data, DEFAULTS), b)

    def test_from_json_drops_bad_entries_and_fills_missing_ones(self):
        data = {str(SHOOT): {"key": "SPACE", "pad": 5}, "999": {"key": "P"}, str(BACK): "junk"}
        b = Bindings.from_json(data, DEFAULTS)
        self.assertEqual(b.table[SHOOT], Binding(("SPACE", "RETURN"), ("A",)))
        self.assertEqual(b.table[BACK], DEFAULTS.table[BACK])
        self.assertNotIn(999, b.table)
        self.assertEqual(Bindings.from_json("nonsense", DEFAULTS), DEFAULTS)

    def test_from_json_falls_back_per_kind_when_a_rule_breaks(self):
        twice = {str(SHOOT): {"key": "X", "pad": "Y"}}  # X is still BACK's primary: two actions on one key.
        b = Bindings.from_json(twice, DEFAULTS)
        self.assertEqual(b.primary(SHOOT, "key"), "Z")
        self.assertEqual(b.primary(SHOOT, "pad"), "Y")  # The pad kind was fine.
        self.assertEqual(Bindings.from_json({str(MENU): {"key": "P"}}, DEFAULTS).primary(MENU, "key"), "ESCAPE")
        self.assertEqual(Bindings.from_json({str(SHOOT): {"key": "Q"}}, DEFAULTS).primary(SHOOT, "key"), "Z")

    def test_with_binding_and_reserved(self):
        b = DEFAULTS.with_binding(UP, Binding(("W",), ("DPAD_UP",))).with_reserved({("key", "W")})
        self.assertEqual(b.primary(UP, "key"), "W")
        self.assertIsNone(b.assign(SHOOT, "key", "W"))
        self.assertIsNotNone(b.assign(SHOOT, "key", "UP"))


if __name__ == "__main__":
    unittest.main()
