# Workflow

## Platforms and tools

- Supported development platforms: Linux x64, macOS arm64 and Windows x64.
- Run `mise install` to install every tool in `mise.toml`.
- `mise.lock` pins the tool downloads for the three platforms.
  After you change `mise.toml`, run `mise lock`.
- On Windows, just runs recipes in PowerShell (`pwsh`).
- On Linux, LÖVE is an AppImage. The AppImage needs FUSE 2 (`libfuse2`).

## Branches and commits

- Start shared engine changes from `main`.
- Use Conventional Commits: `feat:`, `fix:`, `docs:`, `test:`, `style:`, `refactor:`, `chore:`.
- Integration games (Unicycle and Nightrunner) pin a verified engine commit.
  They do not follow `main` automatically.

## Verify a change

1. Run `just check`. It runs the Python unittest suite and `tests/lua.lua` on LuaJIT.
2. Run `just lint`. It runs ruff check, ruff format check and StyLua check.
   If it fails on format, run `just format`.
3. If you change builders or packaging, run `just build`.
4. If you change the browser pointer bridge, run
   `node --test python/src/nightengine/web/tests/pointer.test.mjs`.
5. If you change `tools/love2d-mcp/`, run
   `python3 tools/love2d-mcp/setup.py --out .tools/love2d-mcp` and
   `node tools/love2d-mcp/check.mjs`.

CI on GitHub runs on push to `main`, on `v*` tags and on pull requests.
It installs tools with `jdx/mise-action`, so CI and local runs use the same versions.

- `check` runs `just check` on Linux x64, macOS arm64 and Windows x64.
- `lint` runs `just lint` and the pointer bridge test on Linux.
- `build` runs `just build` on Linux and uploads the archives.
- `devtools` runs the love2d-mcp install and smoke check on Linux.

Forgejo has one Linux runner. `.forgejo/workflows/ci.yml` replaces `.github/workflows/` on Forgejo.
It runs the same `check`, `lint`, `build` and `devtools` jobs on Linux only. It does not upload archives.
It runs on push to `main` and on `v*` tags. Keep its jobs in agreement with the GitHub Linux jobs.

The Lua spec runner fails when it finds no specs. A broken spec search cannot pass with zero tests.

## Type checks

- `just typecheck` runs ty with `ty.toml`. It checks `python/src`, `python/tests`, `tools` and `examples`.
- ty reads the environment in `python/.venv`. Run `uv sync --project python` first.
- The code has known ty errors. Do not add new errors. CI does not run ty yet.

## Mirror

- GitHub (`nightconcept/NightEngine`) is the primary repository. Push to GitHub only.
- `.github/workflows/mirror.yml` copies all branches and tags to
  `forge.solivan.dev/nightconcept/NightEngine` after each push or delete.
- The workflow needs the `FORGEJO_MIRROR_TOKEN` secret. The token needs write access to the Forgejo repository.
- The mirror overwrites Forgejo. Do not push to Forgejo directly.

## Host and path changes

Host or path changes can break games that local checks do not cover.

1. Pin the candidate engine commit in an integration game.
2. Run that game's release builds.
3. Unicycle's GitHub release builds cover Pyxel web, Android, Linux and Windows,
   plus LÖVE archives, Windows, macOS and Android.
4. Ship the engine change after those builds pass.

## Releases

- The engine version is in `python/pyproject.toml`.
- Add an entry to `CHANGELOG.md` for each release.
- Tag releases as `v<version>`, for example `v0.1.2`.

## Execution plans

- Put a multi-step plan in `docs/exec-plans/active/` as `NNN-<slug>.plan.md`.
- Move it to `docs/exec-plans/completed/` when it is complete.
- Update the table in `docs/exec-plans/active/README.md`.
