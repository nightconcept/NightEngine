local t = require("nightengine.testing")
local json = require("nightengine.json")
local class = require("nightengine.class")
local scene = require("nightengine.scene")
local Game = require("nightengine.core")
local inputs = require("nightengine.inputs")
local here = debug.getinfo(1, "S").source:sub(2):match("^(.*)[/\\]")
t.test("shared frame input and cue contract", function()
    local f = assert(io.open(here .. "/../../contract/behavior.json", "rb"))
    local fixture = json.decode(f:read("*a"))
    f:close()
    local Counter = class("Counter", scene.Scene)
    function Counter:update(game, inp)
        if inp:down(inputs.A) then
            game.count = game.count + 1
        end
        if inp:hit(inputs.A) then
            game:cue("act")
        end
    end
    local game = Game.new(7)
    game.count = 0
    game:push(Counter.new())
    local states = { { game.frame, game.count, game.cues } }
    for _, code in ipairs(fixture.codes) do
        game:step(code)
        states[#states + 1] = { game.frame, game.count, game.cues }
    end
    t.eq(states, fixture.states)
end)
