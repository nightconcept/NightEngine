# Recipes are single commands, so PowerShell runs them on Windows.
set windows-shell := ["pwsh", "-NoLogo", "-NoProfile", "-Command"]

check:
    uv run --project python python -m unittest discover -s python/tests -t python
    mise x -- luajit tests/lua.lua

lint:
    mise x -- ruff check python tools examples
    mise x -- ruff format --check python tools examples
    mise x -- stylua --check lua tests/lua tests/lua.lua examples/lua-love

format:
    mise x -- ruff check --fix python tools examples
    mise x -- ruff format python tools examples
    mise x -- stylua lua tests/lua tests/lua.lua examples/lua-love

typecheck:
    mise x -- ty check

build:
    uv build python --out-dir dist
    uv run --project python python -m nightengine.web build --root examples/python-pyxel --out dist/python-example
    uv run --project python python -m nightengine.love build love --root examples/lua-love --out dist/lua-example
