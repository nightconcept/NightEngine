"""Build a LÖVE game into release files from its pyproject.toml.

python -m nightengine.love build [love] [windows] [macos] [android] [--out dist/love]

- love:    <slug>-<version>.love, the game for any system with LÖVE 11.5 installed.
- windows: <slug>-<version>-windows.zip, love.exe with the game fused in, and its DLLs. Builds on any system.
- macos:   <slug>-<version>-macos.zip, <title>.app. Builds on macOS (it signs the app ad hoc).
- android: <slug>-<version>-love.apk, the LÖVE APK with the game inside, unpacked and packed again with apktool
           and signed with android/debug.keystore. Needs Java 17+ and the Android SDK build-tools.

The LÖVE release files and apktool are downloaded once into ~/.cache/nightengine (or $NIGHTENGINE_CACHE).
Each build gets `nightengine_build.lua`, so the game knows its target ([tool.nightengine.targets] in pyproject).
"""

import argparse
import os
import plistlib
import re
import shutil
import subprocess
import sys
import tomllib
import urllib.request
import zipfile
from pathlib import Path

ROOT = Path.cwd()
LOVE = "11.5"
RELEASES = f"https://github.com/love2d/love/releases/download/{LOVE}"
# apktool 2: version 3 dropped renameManifestPackage.
APKTOOL = "https://github.com/iBotPeaches/Apktool/releases/download/v2.12.1/apktool_2.12.1.jar"
SKIP = {"spec", "tests", "lovepilot", "dev", ".DS_Store", "__pycache__"}  # Not shipped: tests, and the dev bridge.


def cache() -> Path:
    return Path(os.environ.get("NIGHTENGINE_CACHE") or Path.home() / ".cache" / "nightengine")


def fetch(url: str) -> Path:
    path = cache() / url.rsplit("/", 1)[1]
    if not path.is_file():
        path.parent.mkdir(parents=True, exist_ok=True)
        print(f"download {url}")
        with urllib.request.urlopen(url, timeout=300) as response, open(f"{path}.part", "wb") as file:
            shutil.copyfileobj(response, file)
        Path(f"{path}.part").replace(path)
    return path


def project() -> dict:
    return tomllib.loads((ROOT / "pyproject.toml").read_text())


def slug() -> str:
    title = project()["tool"]["nightengine"]["web"]["title"]
    return re.sub(r"[^a-z0-9]+", "-", title.lower()).strip("-") or "game"


def title() -> str:
    return project()["tool"]["nightengine"]["web"]["title"]


def target_lua(name: str) -> str:
    """nightengine_build.lua: the build's target, from pyproject.toml, at the game's 640x480."""
    table = project()["tool"]["nightengine"].get("targets", {}).get(name, {})
    controls = ", ".join(f'"{c}"' for c in sorted(table.get("controls", ["keyboard", "mouse", "touch"])))
    screen = project()["tool"]["nightengine"]["web"]
    width, height = table.get("width", screen["width"]), table.get("height", screen["height"])
    return (
        "-- Written by nightengine.love. Do not edit.\n"
        f'return {{ target = {{ name = "{name}", width = {width}, height = {height}, '
        f"controls = {{ {controls} }} }} }}\n"
    )


def game_zip(out: Path, target: str) -> Path:
    """The .love file: love/ with its links followed, plus the build's target."""
    path = out / f"{slug()}-{target}.love"
    with zipfile.ZipFile(path, "w", zipfile.ZIP_DEFLATED) as zf:
        files = []
        for folder, _, names in os.walk(ROOT / "love", followlinks=True):  # love/nightengine is a link.
            files += [Path(folder) / name for name in names]
        for file in sorted(files):
            rel = file.relative_to(ROOT / "love")
            if not SKIP & set(rel.parts):
                zf.write(file, rel.as_posix())
        zf.writestr("nightengine_build.lua", target_lua(target))
    return path


def build_love(out: Path, version: str) -> Path:
    path = out / f"{slug()}-{version}.love"
    shutil.copyfile(game_zip(out, "desktop"), path)
    return path


def build_windows(out: Path, version: str) -> Path:
    game = game_zip(out, "desktop")
    work = out / "windows"
    shutil.rmtree(work, ignore_errors=True)
    with zipfile.ZipFile(fetch(f"{RELEASES}/love-{LOVE}-win64.zip")) as zf:
        zf.extractall(work)
    folder = next(work.iterdir())
    app = work / slug()
    folder.rename(app)
    (app / f"{slug()}.exe").write_bytes((app / "love.exe").read_bytes() + game.read_bytes())
    for name in ("love.exe", "lovec.exe", "love.ico", "game.ico", "changes.txt", "readme.txt"):
        (app / name).unlink(missing_ok=True)
    path = out / f"{slug()}-{version}-windows.zip"
    shutil.make_archive(str(path.with_suffix("")), "zip", work, slug())
    return path


def build_macos(out: Path, version: str) -> Path:
    if sys.platform != "darwin":
        raise RuntimeError("the macOS app builds on macOS")
    game = game_zip(out, "desktop")
    work = out / "macos"
    shutil.rmtree(work, ignore_errors=True)
    work.mkdir(parents=True)
    subprocess.run(["ditto", "-x", "-k", str(fetch(f"{RELEASES}/love-{LOVE}-macos.zip")), str(work)], check=True)
    app = work / f"{title().rstrip('!')}.app"
    (work / "love.app").rename(app)
    shutil.copyfile(game, app / "Contents" / "Resources" / f"{slug()}.love")
    plist = app / "Contents" / "Info.plist"
    info = plistlib.loads(plist.read_bytes())
    info.update(CFBundleName=title(), CFBundleIdentifier=project()["tool"]["nightengine"]["android"]["app_id"],
                CFBundleShortVersionString=version, CFBundleVersion=version)  # fmt: skip
    info.pop("UTExportedTypeDeclarations", None)  # The app opens its own game, not .love files.
    info.pop("CFBundleDocumentTypes", None)
    plist.write_bytes(plistlib.dumps(info))
    subprocess.run(["codesign", "--force", "--deep", "--sign", "-", str(app)], check=True)
    path = out / f"{slug()}-{version}-macos.zip"
    path.unlink(missing_ok=True)
    subprocess.run(["ditto", "-c", "-k", "--keepParent", str(app), str(path)], check=True)
    return path


def sdk_tool(name: str) -> str:
    home = os.environ.get("ANDROID_HOME") or os.environ.get("ANDROID_SDK_ROOT")
    home = Path(home) if home else Path.home() / "Library" / "Android" / "sdk"
    tools = sorted((home / "build-tools").glob("*"), key=lambda p: [int(n) for n in re.findall(r"\d+", p.name)])
    if not tools:
        raise RuntimeError(f"no Android build-tools in {home}; set ANDROID_HOME")
    return str(tools[-1] / name)


def build_android(out: Path, version: str) -> Path:
    config = project()["tool"]["nightengine"]["android"]
    game = game_zip(out, "android")
    work = out / "android"
    shutil.rmtree(work, ignore_errors=True)
    java = shutil.which("java") or "java"
    apktool = [java, "-jar", str(fetch(APKTOOL))]
    subprocess.run([*apktool, "d", "-f", "-q", "-o", str(work), str(fetch(f"{RELEASES}/love-{LOVE}-android.apk"))],
                   check=True)  # fmt: skip
    (work / "assets").mkdir(exist_ok=True)
    shutil.copyfile(game, work / "assets" / "game.love")
    manifest = work / "AndroidManifest.xml"
    text = manifest.read_text()
    # The app runs its own game: no filter that opens other games, and no game picker.
    view = r"\s*<intent-filter>(?:(?!</intent-filter>).)*?action\.VIEW(?:(?!</intent-filter>).)*</intent-filter>"
    text = re.sub(view, "", text, flags=re.S)
    text = re.sub(r'android:label="[^"]*"', f'android:label="{title()}"', text)
    orientations = {"landscape": "sensorLandscape", "portrait": "sensorPortrait", "any": "fullSensor"}
    orientation = orientations[config.get("orientation", "landscape")]
    text = re.sub(r'android:screenOrientation="[^"]*"', f'android:screenOrientation="{orientation}"', text)
    # Names that must be unique on a phone, so this app installs next to LÖVE and other games made from it.
    text = text.replace("org.love2d.android.DYNAMIC_RECEIVER", f"{config['app_id']}.DYNAMIC_RECEIVER")
    text = text.replace('android:authorities="org.love2d.android.', f'android:authorities="{config["app_id"]}.')
    manifest.write_text(text)
    for bools in (work / "res").glob("values*/bools.xml"):
        values = bools.read_text()
        values = re.sub(r'<bool name="embed">\w+</bool>', '<bool name="embed">true</bool>', values)
        values = re.sub(r'<bool name="selector_active">\w+</bool>', '<bool name="selector_active">false</bool>', values)
        bools.write_text(values)
    yml = work / "apktool.yml"
    meta = re.sub(r"\n\s*renameManifestPackage: .*", "", yml.read_text())
    meta = meta.replace("packageInfo:\n", f"packageInfo:\n  renameManifestPackage: {config['app_id']}\n")
    meta = re.sub(r"versionName: .*", f"versionName: {version}", meta)
    # The Pyxel APK's rule (nightengine/android): same app id and key, so either one updates the other.
    major, minor, patch = ([int(n) for n in re.findall(r"\d+", version)[:3]] + [0, 0, 0])[:3]
    meta = re.sub(r"versionCode: .*", f"versionCode: {max(1, major * 10000 + minor * 100 + patch)}", meta)
    yml.write_text(meta)
    if config.get("icon"):
        for icon in list((work / "res").rglob("love.png")) + list((work / "res").rglob("ic_launcher*.png")):
            shutil.copyfile(ROOT / config["icon"], icon)
    unsigned = out / "unsigned.apk"
    subprocess.run([*apktool, "b", "-q", "-o", str(unsigned), str(work)], check=True)
    aligned = out / "aligned.apk"
    subprocess.run([sdk_tool("zipalign"), "-f", "-p", "4", str(unsigned), str(aligned)], check=True)
    path = out / f"{slug()}-{version}-love.apk"
    keys = ["--ks", str(ROOT / config["keystore"]), "--ks-pass", "pass:android", "--key-pass", "pass:android"]
    signer = [sdk_tool("apksigner"), "sign", *keys, "--ks-key-alias", "androiddebugkey"]
    subprocess.run([*signer, "--out", str(path), str(aligned)], check=True)
    unsigned.unlink()
    aligned.unlink()
    return path


BUILDS = {"love": build_love, "windows": build_windows, "macos": build_macos, "android": build_android}


def main(argv: list[str] | None = None) -> int:
    global ROOT
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--root", type=Path, default=Path.cwd())
    parser.add_argument("builds", nargs="*", choices=[*BUILDS, []], help="default: all that build on this system")
    parser.add_argument("--out", type=Path, default=Path("dist/love"))
    argv = sys.argv[1:] if argv is None else argv
    if argv and argv[0] == "build":
        argv = argv[1:]
    args = parser.parse_args(argv)
    ROOT = args.root.resolve()
    names = args.builds or [n for n in BUILDS if n != "macos" or sys.platform == "darwin"]
    args.out.mkdir(parents=True, exist_ok=True)
    version = project()["project"]["version"]
    for name in names:
        print(f"built {BUILDS[name](args.out.resolve(), version)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
