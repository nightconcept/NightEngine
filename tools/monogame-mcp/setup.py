"""Install the locked MCP SDK beside the shared MonoGame adapter."""

import argparse
import shutil
import subprocess
from pathlib import Path

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--out", type=Path, default=Path(".tools/monogame-mcp"))
args = parser.parse_args()
args.out.mkdir(parents=True, exist_ok=True)
for name in ("package.json", "package-lock.json"):
    shutil.copy2(Path(__file__).with_name(name), args.out / name)
subprocess.run(["npm", "ci", "--ignore-scripts", "--prefix", str(args.out)], check=True)
