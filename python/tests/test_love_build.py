"""LÖVE archives use game metadata and omit executable development tools."""

import tempfile
import unittest
import zipfile
from pathlib import Path
from unittest import mock

from nightengine.love import build


class LoveBuildTest(unittest.TestCase):
    def test_archive_uses_game_metadata_and_excludes_dev_bridge(self):
        with tempfile.TemporaryDirectory() as tmp:
            root = Path(tmp)
            (root / "pyproject.toml").write_text("""
[project]
name = "toy"
version = "1.2.3"
[tool.nightengine.web]
title = "Toy Game"
width = 320
height = 180
[tool.nightengine.targets.desktop]
controls = ["keyboard"]
""")
            source = root / "love"
            for name in (
                "main.lua",
                "nightengine/core.lua",
                "nightengine/host/app.lua",
                "nightengine/host/dev/mcp_bridge.lua",
                "spec/test.lua",
                "lovepilot/mcp_bridge.lua",
            ):
                path = source / name
                path.parent.mkdir(parents=True, exist_ok=True)
                path.write_text("-- toy\n")
            out = root / "dist"
            out.mkdir()
            with mock.patch.object(build, "ROOT", root):
                archive = build.build_love(out, "1.2.3")
            self.assertEqual(archive.name, "toy-game-1.2.3.love")
            with zipfile.ZipFile(archive) as zf:
                self.assertEqual(
                    set(zf.namelist()),
                    {"main.lua", "nightengine/core.lua", "nightengine/host/app.lua", "nightengine_build.lua"},
                )
                module = zf.read("nightengine_build.lua").decode()
                self.assertIn("width = 320, height = 180", module)
                self.assertIn('"keyboard"', module)


if __name__ == "__main__":
    unittest.main()
