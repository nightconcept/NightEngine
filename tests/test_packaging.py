"""The Android project and the desktop build, without the Android SDK, Gradle, PyInstaller, or the network."""

import tempfile
import unittest
from pathlib import Path
from unittest import mock

from nightengine.android import build as android
from nightengine.desktop import build as desktop
from nightengine.tests.test_web import fake_fetch, make_game
from nightengine.web import runtime

ANDROID = """
[tool.nightengine.android]
app_id = "dev.example.toy"
orientation = "landscape"
icon = "assets/icon.png"
keystore = "android/debug.keystore"
"""


def android_game(root: Path):
    make_game(root)
    text = (root / "pyproject.toml").read_text().replace('name = "toy"', 'name = "toy"\nversion = "1.2.3"')
    (root / "pyproject.toml").write_text(text + ANDROID)
    (root / "assets" / "icon.png").write_bytes(b"png")


class AndroidTest(unittest.TestCase):
    def test_config_reads_the_tables_and_the_version(self):
        with tempfile.TemporaryDirectory() as tmp:
            android_game(Path(tmp))
            config = android.read_config(Path(tmp))
        self.assertEqual((config.app_id, config.name, config.version, config.slug), ("dev.example.toy", "Toy Game",
                                                                                    "1.2.3", "toy-game"))  # fmt: skip
        self.assertEqual(config.version_code, 10203)
        self.assertEqual(android.AndroidConfig("a", "b", "0.0.0").version_code, 1)

    def test_a_bad_orientation_or_no_table_is_a_clear_error(self):
        with tempfile.TemporaryDirectory() as tmp:
            make_game(Path(tmp))
            with self.assertRaisesRegex(ValueError, "tool.nightengine.android"):
                android.read_config(Path(tmp))
            path = Path(tmp) / "pyproject.toml"
            path.write_text(path.read_text() + '[tool.nightengine.android]\napp_id = "x"\norientation = "up"\n')
            with self.assertRaisesRegex(ValueError, "orientation"):
                android.read_config(Path(tmp))

    def test_project_is_filled_in_with_the_offline_build_icon_and_signing(self):
        with tempfile.TemporaryDirectory() as tmp, mock.patch.object(runtime, "fetch", fake_fetch):
            root = Path(tmp) / "game"
            root.mkdir()
            android_game(root)
            with mock.patch.dict("os.environ", {"NIGHTENGINE_CACHE": str(Path(tmp) / "cache")}):
                project = android.write_project(root, Path(tmp) / "out" / "project", android.read_config(root))
            gradle = (project / "app" / "build.gradle").read_text()
            manifest = (project / "app" / "src" / "main" / "AndroidManifest.xml").read_text()
            strings = (project / "app" / "src" / "main" / "res" / "values" / "strings.xml").read_text()
            web = project / "app" / "src" / "main" / "assets" / "web"
            self.assertIn('applicationId "dev.example.toy"', gradle)
            self.assertIn("versionCode 10203", gradle)
            self.assertIn(str((root / "android" / "debug.keystore").resolve()), gradle)
            self.assertIn('android:screenOrientation="sensorLandscape"', manifest)
            self.assertIn('android:icon="@drawable/ic_launcher"', manifest)
            self.assertIn(">Toy Game<", strings)
            for text in (gradle, manifest, strings, (project / "settings.gradle").read_text()):
                self.assertNotIn("{{", text)
            self.assertTrue((web / "pyxel" / "pyxel.js").is_file() and (web / "game.pyxapp").is_file())

    def test_sdk_must_be_set(self):
        with mock.patch.dict("os.environ", {}, clear=True), self.assertRaisesRegex(RuntimeError, "ANDROID_HOME"):
            android.sdk_dir()


class DesktopTest(unittest.TestCase):
    def test_names(self):
        self.assertEqual(desktop.slug("Unicycle!"), "unicycle")
        self.assertRegex(desktop.target(), r"^(windows|macos|linux)-\w+")

    def test_pyinstaller_is_needed(self):
        with (
            mock.patch("importlib.util.find_spec", return_value=None),
            self.assertRaisesRegex(RuntimeError, "PyInstaller"),
        ):
            desktop.build(Path("."), Path("dist"))


if __name__ == "__main__":
    unittest.main()
