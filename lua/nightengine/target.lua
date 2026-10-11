--- Build targets: where the game runs, at what size, and with which controls (see target.py).
--
-- A staged game holds `nightengine_targets.lua` (the game's `[tool.nightengine.targets.*]` tables) and, in a
-- build, `nightengine_build.lua` (the build's target). `current()` reads them. Without a build, a run picks a
-- target with NIGHTENGINE_TARGET or `--target`, and falls back to `desktop`.
local pointer = require("nightengine.pointer")

local M = {}
M.KEYBOARD, M.MOUSE, M.TOUCH = "keyboard", "mouse", "touch"
M.CONTROLS = { M.KEYBOARD, M.MOUSE, M.TOUCH }
M.NAMES = { "desktop", "web", "android" }

local Target = {}
Target.__index = Target
M.Target = Target

--- `controls` is a list. Width and height 0 mean the game's own size.
function M.new(name, width, height, controls)
    local set = {}
    for _, c in ipairs(controls or M.CONTROLS) do
        local known = false
        for _, k in ipairs(M.CONTROLS) do
            known = known or k == c
        end
        assert(known, "unknown control " .. tostring(c))
        set[c] = true
    end
    width, height = width or 0, height or 0
    assert(width >= 0 and height >= 0 and (width == 0) == (height == 0), "set both width and height, or neither")
    return setmetatable({ name = name or "desktop", width = width, height = height, controls = set }, Target)
end

function Target:has(control)
    return self.controls[control] == true
end

--- A phone build: touch is the only control.
function Target:touch_only()
    local n = 0
    for _ in pairs(self.controls) do
        n = n + 1
    end
    return n == 1 and self:has(M.TOUCH)
end

function Target:takes(kind)
    if kind == pointer.MOUSE then
        return self:has(M.MOUSE) or self:touch_only()
    end
    return self:has(M.TOUCH)
end

function Target:accepts(p)
    return self:takes(p.kind)
end

function Target:sized(width, height)
    if self.width ~= 0 then
        return self
    end
    return M.new(self.name, width, height, self:control_list())
end

--- The controls, sorted, like the Python side's `sorted(controls)`.
function Target:control_list()
    local list = {}
    for c in pairs(self.controls) do
        list[#list + 1] = c
    end
    table.sort(list)
    return list
end

function Target:to_dict()
    return { name = self.name, width = self.width, height = self.height, controls = self:control_list() }
end

function M.from_dict(data)
    return M.new(data.name or "desktop", data.width or 0, data.height or 0, data.controls or M.CONTROLS)
end

--- One target of the game by name, from the staged `nightengine_targets` module.
function M.named(name)
    local ok, tables = pcall(require, "nightengine_targets")
    local table_ = ok and tables[name]
    if table_ then
        local data = { name = name }
        for k, v in pairs(table_) do
            data[k] = v
        end
        return M.from_dict(data)
    end
    return M.new(name)
end

--- The target this run is: the build's, else `name` (from --target), else NIGHTENGINE_TARGET, else desktop.
function M.current(name)
    local ok, built = pcall(require, "nightengine_build")
    if ok and type(built) == "table" and built.target then
        return M.from_dict(built.target)
    end
    return M.named(name or os.getenv("NIGHTENGINE_TARGET") or "desktop")
end

return M
