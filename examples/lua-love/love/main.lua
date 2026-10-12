local class = require("nightengine.class")
local Game = require("nightengine.core")
local Scene = require("nightengine.scene").Scene
local inputs = require("nightengine.inputs")
local Counter = class("Counter", Scene)
function Counter:update(game, inp)
    if inp:hit(inputs.A) then
        game.count = game.count + 1
    end
end
local Example = class("Example", Game)
function Example:init(seed, target)
    Game.init(self, seed, target)
    self.count = 0
    self:push(Counter.new())
end
local renderer = { setup = function() end }
function renderer.draw(game)
    love.graphics.clear(0.1, 0.1, 0.2)
    love.graphics.setColor(1, 1, 1)
    love.graphics.print("NightEngine: press Space (" .. game.count .. ")", 20, 20)
end
local app = require("nightengine.host.app").configure({
    width = 320,
    height = 180,
    fps = 30,
    make_game = Example.new,
    renderer = renderer,
    input = require("nightengine.host.input").new({ keys = { { inputs.A, { "space" } } } }),
    audio = require("nightengine.host.audio").new({}, 4),
    snapshot = function(game)
        return { game.frame, game.count }
    end,
    inspect = function(game)
        return { count = game.count }
    end,
})
love.run = app.run
