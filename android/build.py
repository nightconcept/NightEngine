"""Build a game into an Android APK: a full-screen WebView that runs the game's offline web build.

The game describes the app in its pyproject.toml, next to `[tool.nightengine.web]`:

    [tool.nightengine.android]
    app_id = "dev.example.mygame"        # the Android application id
    name = "My Game"                     # optional, the web title
    orientation = "landscape"            # landscape | portrait | any
    icon = "assets/icon.png"             # optional, a square PNG (512 px is plenty)
    keystore = "android/debug.keystore"  # optional, see below

The app runs as the `android` target (`[tool.nightengine.targets.android]`, see target.py).
The version comes from `[project] version`. `build` writes the Gradle project to `<out>/project`, puts the offline
web build in its assets, runs Gradle, and copies the APK to `<out>/<slug>-<version>.apk`.

The APK is signed with a debug key. Android installs an update only over an APK with the same key, so a game
keeps one debug keystore in its repo (`keystore`, made by `python -m nightengine.android keystore`). Without one,
Gradle uses this machine's own debug key.

Needs JDK 17 or later (`JAVA_HOME` or `java` on the PATH) and the Android SDK (`ANDROID_HOME`) with
"platforms;android-35". Gradle is downloaded into the cache (see web/runtime.py) unless `gradle` is on the PATH.
"""

import argparse
import os
import re
import shutil
import stat
import subprocess
import tomllib
import zipfile
from dataclasses import dataclass
from pathlib import Path

from ..web import build as web
from ..web.runtime import cache_dir, fetch

TEMPLATE = Path(__file__).resolve().parent / "template"
AGP_VERSION = "8.7.3"
GRADLE_VERSION = "8.11.1"
GRADLE_URL = f"https://services.gradle.org/distributions/gradle-{GRADLE_VERSION}-bin.zip"
ORIENTATIONS = {"landscape": "sensorLandscape", "portrait": "sensorPortrait", "any": "fullSensor"}
SIGNING = """
    signingConfigs {
        debug {
            storeFile file("{path}")
            storePassword "android"
            keyAlias "androiddebugkey"
            keyPassword "android"
        }
    }
"""


@dataclass(frozen=True)
class AndroidConfig:
    app_id: str
    name: str
    version: str
    orientation: str = "landscape"
    icon: str | None = None
    keystore: str | None = None

    @property
    def slug(self) -> str:
        return re.sub(r"[^a-z0-9]+", "-", self.name.lower()).strip("-") or "game"

    @property
    def version_code(self) -> int:
        """1.2.3 -> 10203. Android needs a whole number that grows with each release."""
        parts = [int(p) for p in re.findall(r"\d+", self.version)[:3]] + [0, 0, 0]
        return max(1, parts[0] * 10000 + parts[1] * 100 + parts[2])


def read_config(root: Path) -> AndroidConfig:
    data = tomllib.loads((root / "pyproject.toml").read_text())
    table = data.get("tool", {}).get("nightengine", {}).get("android")
    if table is None:
        raise ValueError(f"{root / 'pyproject.toml'} has no [tool.nightengine.android] table")
    if table.get("orientation", "landscape") not in ORIENTATIONS:
        raise ValueError(f"orientation must be one of {', '.join(ORIENTATIONS)}")
    name = table.get("name") or web.read_config(root).title
    version = data.get("project", {}).get("version", "0.0.1")
    return AndroidConfig(**{**table, "name": name, "version": version})


def xml_text(s: str) -> str:
    return s.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;").replace("'", "\\'").replace('"', '\\"')


def write_project(root: Path, project: Path, config: AndroidConfig) -> Path:
    """Copy the template, fill it in, and put the offline web build in its assets."""
    if project.exists():
        shutil.rmtree(project)
    shutil.copytree(TEMPLATE, project)
    icon = ""
    if config.icon:
        target = project / "app" / "src" / "main" / "res" / "drawable-nodpi" / "ic_launcher.png"
        target.parent.mkdir(parents=True)
        shutil.copyfile(root / config.icon, target)
        icon = 'android:icon="@drawable/ic_launcher"'
    signing = ""
    if config.keystore:
        signing = SIGNING.replace("{path}", (root / config.keystore).resolve().as_posix())
        signing += "\n    buildTypes { debug { signingConfig signingConfigs.debug } }\n"
    fills = {
        "SLUG": config.slug, "AGP_VERSION": AGP_VERSION, "APP_ID": config.app_id, "NAME": xml_text(config.name),
        "VERSION_CODE": str(config.version_code), "VERSION_NAME": config.version,
        "ORIENTATION": ORIENTATIONS[config.orientation], "ICON": icon, "SIGNING": signing,
    }  # fmt: skip
    for path in project.rglob("*"):
        if path.is_file() and path.suffix in (".gradle", ".xml"):
            text = path.read_text()
            for key, value in fills.items():
                text = text.replace("{{" + key + "}}", value)
            path.write_text(text)
    web.build(root, project / "app" / "src" / "main" / "assets" / "web", offline=True, target="android")
    return project


def sdk_dir() -> Path:
    for name in ("ANDROID_HOME", "ANDROID_SDK_ROOT"):
        if os.environ.get(name):
            return Path(os.environ[name])
    raise RuntimeError("Set ANDROID_HOME to the Android SDK (with platforms;android-35)")


def gradle() -> str:
    """`gradle` from the PATH, or a pinned distribution downloaded into the cache."""
    found = shutil.which("gradle")
    if found:
        return found
    home = cache_dir() / f"gradle-{GRADLE_VERSION}"
    binary = home / "bin" / "gradle"
    if not binary.is_file():
        archive = fetch(GRADLE_URL, cache_dir() / f"gradle-{GRADLE_VERSION}-bin.zip")
        with zipfile.ZipFile(archive) as zf:
            zf.extractall(cache_dir())
        binary.chmod(binary.stat().st_mode | stat.S_IXUSR | stat.S_IXGRP | stat.S_IXOTH)
    return str(binary)


def build(root: Path, out: Path, config: AndroidConfig | None = None) -> Path:
    root, out = root.resolve(), out.resolve()
    config = config or read_config(root)
    project = write_project(root, out / "project", config)
    (project / "local.properties").write_text(f"sdk.dir={sdk_dir().as_posix()}\n")
    subprocess.run([gradle(), "-p", str(project), "--no-daemon", "--console=plain", "assembleDebug"], check=True)
    apk = out / f"{config.slug}-{config.version}.apk"
    shutil.copyfile(project / "app" / "build" / "outputs" / "apk" / "debug" / "app-debug.apk", apk)
    return apk


def keystore(path: Path):
    """Make a debug keystore with the standard debug passwords. Commit it so every machine signs alike."""
    if path.exists():
        raise FileExistsError(f"{path} already exists")
    path.parent.mkdir(parents=True, exist_ok=True)
    java_home = os.environ.get("JAVA_HOME")
    keytool = str(Path(java_home) / "bin" / "keytool") if java_home else "keytool"
    subprocess.run(
        [keytool, "-genkeypair", "-keystore", str(path), "-storepass", "android", "-alias", "androiddebugkey",
         "-keypass", "android", "-keyalg", "RSA", "-keysize", "2048", "-validity", "10000",
         "-dname", "CN=Android Debug,O=Android,C=US"],
        check=True,
    )  # fmt: skip


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(prog="python -m nightengine.android", description=__doc__,
                                     formatter_class=argparse.RawDescriptionHelpFormatter)  # fmt: skip
    commands = parser.add_subparsers(dest="command", required=True)
    b = commands.add_parser("build", help="build a debug-signed APK")
    b.add_argument("--out", type=Path, default=Path("dist/android"))
    b.add_argument("--root", type=Path, default=Path("."), help="the game's root (holds pyproject.toml)")
    p = commands.add_parser("project", help="write the Gradle project only (to open it in Android Studio)")
    p.add_argument("--out", type=Path, default=Path("dist/android"))
    p.add_argument("--root", type=Path, default=Path("."))
    k = commands.add_parser("keystore", help="make a debug keystore to commit")
    k.add_argument("path", type=Path)
    args = parser.parse_args(argv)
    if args.command == "build":
        print(f"Built {build(args.root, args.out)}")
    elif args.command == "project":
        root = args.root.resolve()
        print(f"Wrote {write_project(root, args.out.resolve() / 'project', read_config(root))}")
    else:
        keystore(args.path)
        print(f"Wrote {args.path}")
    return 0
