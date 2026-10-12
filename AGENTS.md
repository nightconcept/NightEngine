# NightEngine

Nightcircle's engine supports Python/Pyxel and Lua/LÖVE equally.
Read docs/architecture.md and docs/compatibility.md before changing interfaces.
Keep cores deterministic, free of runtime imports and game knowledge.
Python lives in python/src/nightengine; Lua's package root is lua/ directly.
Keep shared fixtures and both implementations compatible.
Run just check and just lint. Use integration game release builds before shipping
host or path changes. Commit messages use Conventional Commits.
Preserve observability tools and licenses. Exclude the Lua dev bridge from game
release archives. Develop shared engine changes from main; games pin verified commits.
