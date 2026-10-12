"""Build the pinned love2d-mcp server for a game's local development tools."""

import argparse
import subprocess
from pathlib import Path

REPO = "https://forge.solivan.dev/nightconcept/love2d-mcp.git"
COMMIT = "160ebfe53a8602f1b442fc6f7e5ab65567e58c8a"


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--out", type=Path, default=Path(".tools/love2d-mcp"))
    out = parser.parse_args().out.resolve()
    if not out.exists():
        out.parent.mkdir(parents=True, exist_ok=True)
        subprocess.run(["git", "clone", REPO, str(out)], check=True)
    subprocess.run(["git", "fetch", "origin"], cwd=out, check=True)
    subprocess.run(["git", "checkout", "--detach", COMMIT], cwd=out, check=True)
    for command in (["npm", "ci"], ["npm", "run", "build"]):
        subprocess.run(command, cwd=out, check=True)
    assert (out / "build" / "index.js").is_file(), "love2d-mcp server build is missing"


if __name__ == "__main__":
    main()
