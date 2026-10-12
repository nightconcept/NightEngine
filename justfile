# Recipes are single commands, so PowerShell runs them on Windows.
set windows-shell := ["pwsh", "-NoLogo", "-NoProfile", "-Command"]

check:
    uv run --project python python tools/parity.py --check
    uv run --project python python -m unittest discover -s python/tests -t python
    mise x -- luajit tests/lua.lua
    mise x -- dotnet run --project csharp/NightEngine.Tests

lint:
    mise x -- ruff check python tools examples
    mise x -- ruff format --check python tools examples
    mise x -- stylua --check lua tests/lua tests/lua.lua examples/lua-love
    mise x -- dotnet format csharp/NightEngine.Tests/NightEngine.Tests.csproj --verify-no-changes

format:
    mise x -- ruff check --fix python tools examples
    mise x -- ruff format python tools examples
    mise x -- stylua lua tests/lua tests/lua.lua examples/lua-love

typecheck:
    mise x -- ty check

build:
    uv build python --out-dir dist
    mise x -- dotnet pack csharp/NightEngine.Core -c Release -o dist/csharp
    uv run --project python python -m nightengine.web build --root examples/python-pyxel --out dist/python-example
    uv run --project python python -m nightengine.love build love --root examples/lua-love --out dist/lua-example

# Check parity.toml against the code and rewrite docs/concepts/parity.md. `--release` fails on open decisions.
parity *args:
    uv run --project python python tools/parity.py {{args}}

docs:
    mise x -- pnpm --dir site build

docs-dev:
    mise x -- pnpm --dir site dev
