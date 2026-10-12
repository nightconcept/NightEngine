--- The base of every game: scene stack, input tracker, screen effects, seeded RNG, and sound cues (see core.py).
--
-- No `love` here: the renderer reads a Game, and tests drive one headless. A game subclasses Game, sets the class
-- fields below, adds its own state, and starts its first scene. `step` runs one frame.
local class = require("nightengine.class")
local inputs = require("nightengine.inputs")
local Rng = require("nightengine.rng")
local ScreenFx = require("nightengine.fx")
local scene = require("nightengine.scene")
local target = require("nightengine.target")

local Game = class("Game")
Game.input_mask = inputs.ALL
Game.repeat_buttons = {}
Game.repeat_delay = inputs.REPEAT_DELAY
Game.repeat_rate = inputs.REPEAT_RATE

function Game:init(seed, target_)
    self.seed = seed or 0
    self.target = target_ or target.new()
    self.rng = Rng.new(self.seed) -- The only source of randomness in game rules.
    self.frame = 0
    self.tracker = inputs.InputTracker.new(self.input_mask, self.repeat_buttons, self.repeat_delay, self.repeat_rate)
    self.scenes = scene.SceneStack.new()
    self.fx = ScreenFx.new()
    self.cues = {}
    self.quit_requested = false
end

function Game:scene()
    return self.scenes:top()
end

function Game:push(s)
    self.scenes:push(s)
end

function Game:pop(s)
    self.scenes:pop(s)
end

--- The screen size the game wants now as (w, h), or nil for the target's.
function Game:screen_size()
    return nil
end

function Game:music()
    return self:scene():music(self)
end

function Game:cue(name)
    self.cues[#self.cues + 1] = name
end

function Game:before_scene(inp) end
function Game:after_scene() end

--- Advance one frame with the held buttons in `code` and this frame's pointers. Returns the sound cues.
function Game:step(code, pointers, events)
    self.cues = {}
    local kept = {}
    for _, p in ipairs(pointers or {}) do
        if self.target:accepts(p) then
            kept[#kept + 1] = p
        end
    end
    local inp = self.tracker:feed(code, kept, events)
    self.fx:tick()
    self:before_scene(inp)
    self:scene():update(self, inp)
    self:after_scene()
    self.frame = self.frame + 1
    return self.cues
end

return Game
