local t = require("nightengine.testing")
local controls = require("nightengine.host.controls")
local trace = require("nightengine.trace")

t.test("host controls preserve game state and queued deterministic steps", function()
    local game = {
        frame = 12,
        scene = function()
            return { name = "title" }
        end,
    }
    local app = { game = game, paused = false, steps = 0 }
    local ne = controls.new(app, {
        inspect = function(g)
            return { score = g.frame * 2 }
        end,
        snapshot = function(g)
            return { g.frame }
        end,
        buttons = { A = 16 },
    })
    t.eq(ne.state(), { frame = 12, scene = "title", paused = false, score = 24, hash = trace.digest({ 12 }) })
    t.eq(ne.pause(), 12)
    t.eq(ne.step(2), 14)
    t.eq(ne.step(), 15)
    t.eq(app.steps, 3)
    t.eq(ne.resume(), 12)
    t.eq(app.paused, false)
    t.eq(app.steps, 0)
    ne.hold(16, 3)
    t.eq(app.held, { code = 16, frames = 3 })
    t.raises(function()
        ne.step(-1)
    end)
    t.raises(function()
        ne.hold(16, 0)
    end)
    app.replay = {}
    t.raises(function()
        ne.hold(16)
    end)
    app.replay, game.input_mask = nil, 16
    t.raises(function()
        ne.hold(32)
    end)
end)

t.test("host records injected input and runs only queued frames while paused", function()
    _G.love = {
        graphics = {},
        system = {
            getOS = function()
                return "Linux"
            end,
        },
        keyboard = {
            isDown = function()
                return false
            end,
        },
    }
    local app = require("nightengine.host.app").configure({
        input = {
            buttons = function()
                return 1
            end,
            pointers = function()
                return {}
            end,
        },
        audio = { play = function() end },
        renderer = {},
    })
    local recorded = {}
    app.game = {
        frame = 0,
        step = function(self, code)
            self.frame = self.frame + 1
            t.eq(code, 17)
            return {}
        end,
    }
    app.recording = {
        add = function(_, code)
            recorded[#recorded + 1] = code
        end,
    }
    app.fit = function() end
    app.paused, app.steps, app.held = true, 2, { code = 16, frames = 2 }
    app.update()
    app.update()
    app.update()
    t.eq(app.game.frame, 2)
    t.eq(recorded, { 17, 17 })
    package.loaded["nightengine.host.app"] = nil
end)

t.test("host input keeps configured bindings, one-frame pulses, and pointer lifecycle", function()
    local contact, x, y = true, 4, 5
    _G.love = {
        keyboard = {
            isDown = function(key)
                return key == "space"
            end,
        },
        joystick = {
            getJoysticks = function()
                return {}
            end,
        },
        touch = {
            getTouches = function()
                return contact and { "finger" } or {}
            end,
            getPosition = function()
                return x, y
            end,
        },
    }
    local input = require("nightengine.host.input").new({ keys = { { 16, { "space" } } }, pulses = { escape = 32 } })
    input.keypressed("escape")
    t.eq(input.buttons(), 48)
    t.eq(input.buttons(), 16)
    local p = require("nightengine.pointer")
    local identity = function(a, b)
        return a, b
    end
    local pressed = input.pointers(identity, false)[1]
    t.eq({ pressed.id, pressed.phase }, { 1, p.PRESSED })
    x = 6
    t.eq(input.pointers(identity, false)[1].phase, p.MOVED)
    t.eq(input.pointers(identity, false)[1].phase, p.HELD)
    contact = false
    local released = input.pointers(identity, false)[1]
    t.eq({ released.x, released.start_x, released.phase }, { 6, 4, p.RELEASED })
end)

t.test("host audio suppresses duplicate cues and interrupts occupied channels", function()
    local played, stopped = 0, 0
    local function source()
        return {
            clone = source,
            play = function()
                played = played + 1
            end,
            stop = function()
                stopped = stopped + 1
            end,
        }
    end
    _G.love = {
        audio = {
            newSource = function(path, kind)
                t.eq(path, "assets/sfx/jump.wav")
                t.eq(kind, "static")
                return source()
            end,
        },
    }
    local audio = require("nightengine.host.audio").new({ "jump" }, 1)
    audio.setup()
    audio.play({ "jump", "jump", "unknown" })
    audio.play({ "jump" })
    t.eq({ played, stopped }, { 2, 1 })
end)

t.test("host uses target size when the game has no screen override", function()
    _G.love = {
        system = {
            getOS = function()
                return "Linux"
            end,
        },
        graphics = {
            newCanvas = function(w, h)
                t.eq({ w, h }, { 320, 240 })
                return { setFilter = function() end }
            end,
        },
    }
    package.loaded["nightengine.host.app"] = nil
    local app = require("nightengine.host.app").configure({})
    app.game = {
        screen_size = function()
            return nil
        end,
    }
    app.target, app.canvas, app.sized = { width = 320, height = 240 }, nil, true
    app.fit()
    package.loaded["nightengine.host.app"] = nil
end)

t.test("host hash uses the configured snapshot and rejects missing snapshots", function()
    local app = { game = { frame = 3 } }
    local ne = controls.new(app, {
        snapshot = function(g)
            return { g.frame }
        end,
    })
    t.eq(ne.hash(), trace.digest({ 3 }))
    t.raises(function()
        controls.new(app, {}).hash()
    end)
end)

t.test("target selection accepts game configuration without loading game modules", function()
    local target = require("nightengine.target")
    local definitions = { phone = { width = 320, height = 240, controls = { "touch" } } }
    local configured = target.current("phone", definitions)
    t.eq(configured:to_dict(), { name = "phone", width = 320, height = 240, controls = { "touch" } })
    t.eq(definitions.phone.name, nil)
end)
