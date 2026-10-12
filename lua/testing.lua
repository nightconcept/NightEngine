--- Test helpers: a Driver that plays a headless Game through button codes (see testing.py), and a small test
-- runner for plain LuaJIT. A spec file returns nothing; it calls `test(name, fn)` and the checks below.
local pointer = require("nightengine.pointer")

local M = {}

local Driver = {}
Driver.__index = Driver
M.Driver = Driver

function M.driver(game)
    return setmetatable({ game = game }, Driver)
end

function Driver:step(code, frames, pointers)
    for _ = 1, frames or 1 do
        self.game:step(code or 0, pointers)
    end
end

function Driver:tap(x, y, pid, kind)
    pid = pid or 0
    self:step(0, 1, { pointer.new(pid, x, y, x, y, pointer.PRESSED, kind) })
    self:step(0, 1, { pointer.new(pid, x, y, x, y, pointer.RELEASED, kind) })
end

function Driver:press(button)
    self:step(button)
    self:step(0)
end

function Driver:run_until(predicate, limit, code)
    for _ = 1, limit or 5000 do
        if predicate() then
            return
        end
        self:step(type(code) == "function" and code() or code or 0)
    end
    error("condition never held", 2)
end

-- The runner ---------------------------------------------------------------

local tests = {}

function M.test(name, fn)
    tests[#tests + 1] = { name = name, fn = fn }
end

local function show(v)
    if type(v) == "string" then
        return string.format("%q", v)
    end
    return tostring(v)
end

local function deep_eq(a, b)
    if type(a) ~= "table" or type(b) ~= "table" then
        return a == b
    end
    for k, v in pairs(a) do
        if not deep_eq(v, b[k]) then
            return false
        end
    end
    for k in pairs(b) do
        if a[k] == nil then
            return false
        end
    end
    return true
end

function M.eq(actual, expected, msg)
    if not deep_eq(actual, expected) then
        error(string.format("%sexpected %s, got %s", msg and msg .. ": " or "", show(expected), show(actual)), 2)
    end
end

function M.ok(value, msg)
    if not value then
        error(msg or "expected a true value", 2)
    end
end

function M.raises(fn, msg)
    if pcall(fn) then
        error(msg or "expected an error", 2)
    end
end

--- Run every `*_spec.lua` file under the directories. Returns the number of failures.
function M.run(dirs, filter)
    local failures, count = 0, 0
    for _, dir in ipairs(dirs) do
        local files = {}
        local list = io.popen('find "' .. dir .. '" -name "*_spec.lua" | sort')
        for line in list:lines() do
            files[#files + 1] = line
        end
        list:close()
        for _, file in ipairs(files) do
            tests = {}
            local chunk = assert(loadfile(file))
            chunk()
            for _, t in ipairs(tests) do
                if not filter or (file .. " " .. t.name):find(filter, 1, true) then
                    count = count + 1
                    local ok, err = xpcall(t.fn, debug.traceback)
                    if not ok then
                        failures = failures + 1
                        io.write(string.format("FAIL %s: %s\n%s\n", file, t.name, err))
                    end
                end
            end
        end
    end
    io.write(string.format("%d tests, %d failed\n", count, failures))
    return failures
end

return M
