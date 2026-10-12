import tempfile
import unittest
from dataclasses import dataclass
from pathlib import Path

from nightengine import ContentError, Registry, check_refs, read_dir, read_json, records


class RegistryTest(unittest.TestCase):
    def setUp(self):
        self.moves = Registry("move")

        @self.moves.register("sine")
        def sine():
            return "sine"

        self.sine = sine

    def test_register_returns_the_function_and_looks_up_by_name(self):
        self.assertEqual(self.moves["sine"](), "sine")
        self.assertIn("sine", self.moves)
        self.assertNotIn("zigzag", self.moves)
        self.assertEqual(self.moves.names(), ["sine"])

    def test_missing_name_error_names_the_kind_and_known_names(self):
        self.moves.register("dive")(lambda: None)
        with self.assertRaises(KeyError) as ctx:
            self.moves["zigzag"]
        message = str(ctx.exception)
        self.assertIn("move", message)
        self.assertIn("zigzag", message)
        self.assertIn("sine", message)
        self.assertIn("dive", message)

    def test_missing_lists_unregistered_names_once(self):
        self.assertEqual(self.moves.missing(["sine", "b", "a", "b"]), ["a", "b"])
        self.assertEqual(self.moves.missing(["sine"]), [])


@dataclass(frozen=True)
class Enemy:
    id: str
    hp: int
    path: tuple[tuple[int, int], ...] = ()
    kind: str = "walker"


class ContentTest(unittest.TestCase):
    def test_read_json_drops_doc(self):
        with tempfile.TemporaryDirectory() as tmp:
            p = Path(tmp) / "a.json"
            p.write_text('{"_doc": "notes", "x": {"hp": 1}}')
            self.assertEqual(read_json(p), {"x": {"hp": 1}})

    def test_read_dir_keys_by_stem_in_order(self):
        with tempfile.TemporaryDirectory() as tmp:
            for name in ("b", "a"):
                (Path(tmp) / f"{name}.json").write_text(f'{{"name": "{name}"}}')
            (Path(tmp) / "skip.txt").write_text("no")
            self.assertEqual(read_dir(tmp), {"a": {"name": "a"}, "b": {"name": "b"}})

    def test_records_builds_frozen_typed_records_with_nested_conversion(self):
        table = {"bat": {"hp": 2, "path": [[1, 2], [3, 4]]}, "imp": {"hp": 5}}
        out = records(table, Enemy, convert={"path": lambda v: tuple(tuple(p) for p in v)})
        self.assertEqual(out["bat"], Enemy("bat", 2, ((1, 2), (3, 4))))
        self.assertEqual(out["imp"].id, "imp")
        with self.assertRaises(AttributeError):
            out["imp"].hp = 1

    def test_records_rename_and_custom_key(self):
        @dataclass(frozen=True)
        class Skill:
            name: str
            defense: int

        out = records({"guard": {"def": 3}}, Skill, key="name", rename={"def": "defense"})
        self.assertEqual(out["guard"], Skill("guard", 3))

    def test_records_error_names_the_record(self):
        with self.assertRaises(ContentError) as ctx:
            records({"bat": {"hp": 1, "wings": 2}}, Enemy)
        self.assertIn("bat", str(ctx.exception))

    def test_check_refs_finds_strings_and_lists(self):
        table = {
            "a": {"kind": "walker", "drops": ["gem", "ghost"]},
            "b": {"kind": "flyer", "drops": []},
        }
        self.assertEqual(check_refs(table, "kind", {"walker"}, "enemy"), ["enemy: b.kind names missing 'flyer'"])
        self.assertEqual(check_refs(table, "drops", {"gem"}, "enemy"), ["enemy: a.drops names missing 'ghost'"])

    def test_check_refs_reads_record_attributes(self):
        table = records({"bat": {"hp": 1, "kind": "nope"}}, Enemy)
        self.assertEqual(len(check_refs(table, "kind", ["walker"], "enemy")), 1)
        self.assertEqual(check_refs(table, "kind", ["nope"], "enemy"), [])


if __name__ == "__main__":
    unittest.main()
