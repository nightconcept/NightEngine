"""The web build packages any game: its entry, packages, and assets, the pointer check, and the page."""

import tempfile
import unittest
import zipfile
from pathlib import Path

from nightengine.web.build import WebConfig, build, read_config

PYPROJECT = """
[project]
name = "toy"

[tool.nightengine.web]
title = "Toy Game"
width = 320
height = 180
packages = ["toy", "nightengine"]
assets = ["assets"]
"""


def make_game(root: Path):
    (root / "pyproject.toml").write_text(PYPROJECT)
    (root / "game.py").write_text("import toy\n")
    files = ["toy/__init__.py", "toy/scenes/title.py", "toy/tests/test_toy.py", "toy/__pycache__/x.py"]
    files += ["nightengine/__init__.py", "nightengine/host/diagnostic.py", "nightengine/tests/test_x.py"]
    for rel in files:
        (root / rel).parent.mkdir(parents=True, exist_ok=True)
        (root / rel).write_text("")
    (root / "assets" / "fonts").mkdir(parents=True)
    (root / "assets" / "fonts" / "font.bdf").write_text("font")


class WebBuildTest(unittest.TestCase):
    def test_config_is_read_from_pyproject(self):
        with tempfile.TemporaryDirectory() as tmp:
            make_game(Path(tmp))
            config = read_config(Path(tmp))
        self.assertEqual(config, WebConfig("Toy Game", 320, 180, "game.py", ("toy", "nightengine"), ("assets",)))

    def test_build_writes_the_apps_and_the_page(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp) / "game"
            root.mkdir()
            make_game(root)
            out = build(root, Path(tmp) / "dist")
            with zipfile.ZipFile(out / "game.pyxapp") as app:
                names = set(app.namelist())
                self.assertEqual(app.read("app/.pyxapp_startup_script").decode(), "game.py")
            self.assertTrue({"app/game.py", "app/toy/scenes/title.py", "app/assets/fonts/font.bdf",
                             "app/nightengine/host/diagnostic.py"} <= names)  # fmt: skip
            self.assertFalse([n for n in names if "/tests/" in n or "__pycache__" in n])
            with zipfile.ZipFile(out / "debug.pyxapp") as app:
                self.assertEqual(app.read("app/.pyxapp_startup_script").decode(), "debug.py")
                self.assertIn("nightengine.host.diagnostic", app.read("app/debug.py").decode())
            page = (out / "index.html").read_text()
            self.assertIn('data-width="320" data-height="180"', page)
            self.assertIn("<title>Toy Game</title>", page)
            self.assertNotIn("{{", page)
            self.assertTrue((out / "pointer.mjs").is_file() and (out / "launcher.mjs").is_file())

    def test_refuses_to_write_into_the_game(self):
        with tempfile.TemporaryDirectory() as tmp:
            make_game(Path(tmp))
            with self.assertRaises(ValueError):
                build(Path(tmp), Path(tmp))

    def test_missing_table_is_a_clear_error(self):
        with tempfile.TemporaryDirectory() as tmp:
            (Path(tmp) / "pyproject.toml").write_text("[project]\nname = 'x'\n")
            with self.assertRaisesRegex(ValueError, r"tool.nightengine.web"):
                read_config(Path(tmp))


if __name__ == "__main__":
    unittest.main()
