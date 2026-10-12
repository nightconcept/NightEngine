"""Build the pinned LovePilot MCP server for a game's local development tools."""

import argparse
import shutil
import subprocess
from pathlib import Path

REPO = "https://github.com/InfiniteLoopRD/lovepilot.git"
COMMIT = "cb2ddc8e16ab669d4fa51d4ccf67012dc3472912"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--out", type=Path, default=Path(".tools/lovepilot"))
    out = parser.parse_args().out.resolve()
    if not out.exists():
        out.parent.mkdir(parents=True, exist_ok=True)
        subprocess.run(["git", "clone", REPO, str(out)], check=True)
    subprocess.run(["git", "fetch", "origin"], cwd=out, check=True)
    # This generated checkout owns the lock overlay; restore it before switching pinned source.
    subprocess.run(["git", "restore", "package-lock.json"], cwd=out, check=True)
    subprocess.run(["git", "checkout", "--detach", COMMIT], cwd=out, check=True)
    shutil.copyfile(Path(__file__).with_name("package-lock.json"), out / "package-lock.json")
    for command in (["npm", "ci"], ["npm", "run", "build"]):
        subprocess.run(command, cwd=out, check=True)
    assert (out / "build" / "index.js").is_file(), "LovePilot server build is missing"


if __name__ == "__main__":
    main()
