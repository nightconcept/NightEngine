local t = require("nightengine.testing")
local inputs = require("nightengine.inputs")
local json = require("nightengine.json")
local here = debug.getinfo(1, "S").source:sub(2):match("^(.*)[/\\]")

local f = assert(io.open(here .. "/../fixtures/dev.json", "rb"))
local fixture = json.decode(f:read("*a"))
f:close()

t.test("just dev pairs become host events and read back the same", function()
    local events = {}
    for i, pair in ipairs(fixture.pairs) do
        events[i] = inputs.dev_event(pair)
    end
    t.eq(events, fixture.events)
    for _, pair in ipairs(fixture.invalid) do
        t.eq(pcall(inputs.dev_event, pair), false)
    end
    t.eq(inputs.input(0, 0, 0, {}, fixture.frame):dev(), fixture.dev)
    t.eq(inputs.input():dev(), {})
end)
