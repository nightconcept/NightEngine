check:
    uv run --project python python -m unittest discover -s python/tests -t python
    mise x -- luajit tests/lua.lua

lint:
    uvx ruff check python tools examples
    uvx ruff format --check python tools examples
    mise x -- stylua --check lua tests/lua tests/lua.lua examples/lua-love

build:
    uv build python --out-dir dist
    uv run --project python python -m nightengine.web build --root examples/python-pyxel --out dist/python-example
    uv run --project python python -m nightengine.love build love --root examples/lua-love --out dist/lua-example
