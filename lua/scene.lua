--- The scene protocol and the scene stack. Only the top scene updates; the renderer draws `visible()` bottom-up.
local class = require("nightengine.class")

local M = {}

local Scene = class("Scene")
Scene.overlay = false -- True when the scenes below should still be drawn.
function Scene:update(game, inp) end
function Scene:music(game)
    return nil
end
M.Scene = Scene

--- A list of scenes, bottom first. `replace` changes it in place, so references to it stay valid.
local SceneStack = class("SceneStack")
M.SceneStack = SceneStack

function SceneStack:init(...)
    self.items = { ... }
end

function SceneStack:top()
    return self.items[#self.items]
end

function SceneStack:push(scene)
    self.items[#self.items + 1] = scene
end

function SceneStack:pop(scene)
    for i = #self.items, 1, -1 do
        if self.items[i] == scene then
            table.remove(self.items, i)
            return
        end
    end
end

function SceneStack:replace(...)
    self.items = { ... }
end

function SceneStack:find(cls)
    for i = #self.items, 1, -1 do
        if class.is(self.items[i], cls) then
            return self.items[i]
        end
    end
end

function SceneStack:visible()
    local start = #self.items
    while start > 1 and self.items[start].overlay do
        start = start - 1
    end
    local out = {}
    for i = math.max(start, 1), #self.items do
        out[#out + 1] = self.items[i]
    end
    return out
end

function SceneStack:len()
    return #self.items
end

return M
