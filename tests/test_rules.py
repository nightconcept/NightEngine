"""The engine rules, checked by parsing every file: where pyxel may be imported, and no outside packages."""

import ast
import sys
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
HOST_TESTS = {  # They stub pyxel.
    Path("tests/test_host.py"),
    Path("tests/test_platform.py"),
    Path("tests/test_gamepad.py"),
}
ALLOWED = set(sys.stdlib_module_names) | {"pyxel", "nightengine"}  # The engine knows no game.


def imports(path: Path) -> set[str]:
    """The top-level package of every import in a file."""
    found = set()
    for node in ast.walk(ast.parse(path.read_text())):
        if isinstance(node, ast.Import):
            found |= {a.name.split(".")[0] for a in node.names}
        elif isinstance(node, ast.ImportFrom) and node.level == 0 and node.module:
            found.add(node.module.split(".")[0])
    return found


class RulesTest(unittest.TestCase):
    def sources(self):
        return [p for p in ROOT.rglob("*.py") if "__pycache__" not in p.parts]

    def test_pyxel_only_in_host(self):
        """Host code and the host tests (which stub pyxel) may import it. Nothing else may."""
        for path in self.sources():
            rel = path.relative_to(ROOT)
            if "host" not in rel.parts[:-1] and rel not in HOST_TESTS:
                self.assertNotIn("pyxel", imports(path), f"{rel} imports pyxel")

    def test_only_stdlib_pyxel_and_itself(self):
        """The engine is agnostic: it never imports a game or any other third-party package."""
        for path in self.sources():
            rel = path.relative_to(ROOT)
            allowed = ALLOWED | ({"js"} if "host" in rel.parts[:-1] else set())  # The browser, in host/ only.
            outside = imports(path) - allowed
            self.assertFalse(outside, f"{rel} imports {sorted(outside)}")

    def test_core_has_no_wall_clock_or_global_random(self):
        for path in self.sources():
            if "tests" in path.parts or "host" in path.parts:
                continue
            self.assertFalse(imports(path) & {"time", "datetime"}, f"{path.relative_to(ROOT)} reads the clock")
            tree = ast.parse(path.read_text())
            for node in ast.walk(tree):
                if isinstance(node, ast.Attribute) and isinstance(node.value, ast.Name) and node.value.id == "random":
                    self.fail(f"{path.relative_to(ROOT)} uses the random module state")


if __name__ == "__main__":
    unittest.main()
