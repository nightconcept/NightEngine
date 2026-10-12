"""Build the pinned LovePilot MCP server for a game's local development tools."""

import argparse
import subprocess
from pathlib import Path

REPO = "https://github.com/InfiniteLoopRD/lovepilot.git"
COMMIT = "cb2ddc8"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--out", type=Path, default=Path(".tools/lovepilot"))
    out = parser.parse_args().out.resolve()
    if not out.exists():
        out.parent.mkdir(parents=True, exist_ok=True)
        subprocess.run(["git", "clone", REPO, str(out)], check=True)
    subprocess.run(["git", "fetch", "origin"], cwd=out, check=True)
    subprocess.run(["git", "checkout", "--detach", COMMIT], cwd=out, check=True)
    for command in (["npm", "ci"], ["npm", "run", "build"]):
        subprocess.run(command, cwd=out, check=True)
    assert (out / "build" / "index.js").is_file(), "LovePilot server build is missing"


if __name__ == "__main__":
    main()
