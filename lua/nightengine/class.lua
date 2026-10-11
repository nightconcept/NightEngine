--- Classes: a table of methods with `new`, and single inheritance.
--
--     local Scene = class("Scene")
--     local Title = class("Title", Scene)
--     function Title:init(a) Scene.init(self); self.a = a end
--     local t = Title.new(1)
--     class.is(t, Scene) --> true
local class = {}

local function new(cls, ...)
    local o = setmetatable({}, cls)
    o:init(...)
    return o
end

setmetatable(class, {
    __call = function(_, name, parent)
        local cls = { name = name, super = parent }
        cls.__index = cls
        cls.new = function(...)
            return new(cls, ...)
        end
        if not parent then
            cls.init = function() end
        end
        return setmetatable(cls, { __index = parent })
    end,
})

--- True if `obj` is an instance of `cls` or of a subclass.
function class.is(obj, cls)
    if type(obj) ~= "table" then
        return false
    end
    local c = getmetatable(obj)
    while c do
        if c == cls then
            return true
        end
        c = c.super
    end
    return false
end

--- The class name of an instance, like Python's type(obj).__name__.
function class.name(obj)
    local c = getmetatable(obj)
    return c and c.name
end

return class
