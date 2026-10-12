local t = require("nightengine.testing")
local json = require("nightengine.json")
local Rng = require("nightengine.rng")
local trace = require("nightengine.trace")
local inputs = require("nightengine.inputs")
local pointer = require("nightengine.pointer")
local replay = require("nightengine.replay")
local target = require("nightengine.target")
local scene = require("nightengine.scene")
local class = require("nightengine.class")
local Game = require("nightengine.core")

local here = debug.getinfo(1, "S").source:sub(2):match("^(.*)[/\\]")
local function read(path)
    local f = assert(io.open(path, "rb"))
    local s = f:read("*a")
    f:close()
    return s
end
local contract = json.decode(read(here .. "/../fixtures/rng.json"))

t.test("rng matches the Python vectors", function()
    for _, case in ipairs(contract.rng) do
        local rng = Rng.new(case.seed)
        for i, want in ipairs(case["randint(-5,150)"]) do
            t.eq(rng:randint(-5, 150), want, "seed " .. tostring(case.seed) .. " randint " .. i)
        end
        for i, want in ipairs(case["getrandbits(32)"]) do
            t.eq(rng:getrandbits(32), want, "seed " .. tostring(case.seed) .. " bits " .. i)
        end
    end
end)

t.test("xoshiro128** reference output", function()
    local rng = Rng.new(0)
    rng.s = { 1, 2, 3, 4 }
    t.eq(rng:next(), 11520)
end)

t.test("canon and digest match Python", function()
    local v = { { "abc", 1.5, 3, -0.0, true, trace.null, { 'q"x\\' } } }
    t.eq(trace.canon(v), contract.canon.text)
    t.eq(trace.digest(v), contract.canon.hash)
    t.eq(trace.crc32("123456789"), 0xCBF43926)
end)

t.test("input tracker edges and repeat", function()
    local tr = inputs.InputTracker.new(inputs.ALL, { inputs.DOWN }, 3, 2)
    local seen = {}
    for f = 1, 8 do
        local inp = tr:feed(inputs.DOWN)
        seen[f] = inp:nav(inputs.DOWN)
        if f == 1 then
            t.ok(inp:hit(inputs.DOWN))
        else
            t.ok(not inp:hit(inputs.DOWN))
        end
    end
    t.eq(seen, { true, false, true, false, true, false, true, false })
end)

t.test("recording text is the Python text", function()
    local rec = replay.new(7, nil, nil, 7, target.new("android", 640, 480, { "touch" }))
    rec:add(0x1000)
    rec:add(
        0,
        { pointer.new(1, 10, 20, 10, 20, pointer.PRESSED), pointer.new(0, 3, 4, 3, 4, pointer.HELD, pointer.MOUSE) }
    )
    local text = rec:to_json()
    t.eq(
        text,
        '{"seed":7,"inputs":"00010000000000","width":7,"pointers":{"1":[[1,10,20,10,20,"P"],[0,3,4,3,4,"H","m"]]},"target":{"name":"android","width":640,"height":480,"controls":["touch"]}}'
    )
    local back = replay.from_json(text)
    t.eq(back:to_json(), text)
    t.eq(back:at(1)[2].kind, pointer.MOUSE)
    t.eq(replay.digits_for(2 ^ 28 - 1), 7)
end)

t.test("scene stack and game step", function()
    local Title = class("Title", scene.Scene)
    function Title:update(game, inp)
        if inp:hit(inputs.A) then
            game:cue("ok")
        end
    end
    local Over = class("Over", scene.Scene)
    Over.overlay = true
    local g = Game.new(3)
    local title = Title.new()
    g.scenes:replace(title)
    g:push(Over.new())
    t.eq(#g.scenes:visible(), 2)
    t.eq(g.scenes:find(Title), title)
    g:pop(g:scene())
    t.eq(g:step(inputs.A), { "ok" })
    t.eq(g:step(inputs.A), {})
    t.eq(g.frame, 2)
end)

t.test("a desktop target drops touch, a phone takes the mouse", function()
    local desktop = target.new("desktop", 0, 0, { "keyboard" })
    local phone = target.new("android", 0, 0, { "touch" })
    local touch = pointer.new(1, 0, 0, 0, 0, pointer.PRESSED)
    local mouse = pointer.new(0, 0, 0, 0, 0, pointer.PRESSED, pointer.MOUSE)
    t.ok(not desktop:accepts(touch) and not desktop:accepts(mouse))
    t.ok(phone:touch_only() and phone:accepts(touch) and phone:accepts(mouse))
end)
