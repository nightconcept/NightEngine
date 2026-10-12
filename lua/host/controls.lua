--- Runtime inspection and deterministic input injection; game details are callbacks.
local bit = require("bit")
local trace = require("nightengine.trace")
local M = {}
local function frames(n)
    assert(type(n) == "number" and n >= 1 and n == math.floor(n), "frames must be a positive integer")
    return n
end
function M.new(app, config)
    local ne = {
        app = app,
        buttons = config.buttons or {},
        game = function()
            return app.game
        end,
    }
    function ne.state()
        local game = app.game
        local out = config.inspect and config.inspect(game) or {}
        out.frame, out.scene, out.paused = game.frame, game:scene().name, app.paused
        if config.snapshot then
            out.hash = ne.hash()
        end
        return out
    end
    function ne.hash()
        assert(config.snapshot, "a snapshot callback is required")
        return trace.digest(config.snapshot(app.game))
    end
    function ne.pause()
        app.paused = true
        return app.game.frame
    end
    function ne.resume()
        app.paused, app.steps = false, 0
        return app.game.frame
    end
    function ne.step(n)
        app.paused, app.steps = true, app.steps + frames(n or 1)
        return app.game.frame + app.steps
    end
    function ne.hold(code, n)
        assert(type(code) == "number" and code >= 0 and code == math.floor(code), "input must be a nonnegative integer")
        assert(not app.replay, "cannot inject input into a replay")
        local mask = app.game.input_mask or config.input_mask
        assert(not mask or bit.band(code, mask) == code, "input contains unsupported buttons")
        app.held = { code = code, frames = frames(n or 1) }
        return true
    end
    function ne.shot(path)
        app.render()
        local png = app.canvas:newImageData():encode("png"):getString()
        if path then
            local f = assert(io.open(path, "wb"))
            assert(f:write(png))
            assert(f:close())
            return path
        end
        assert(love.filesystem.write("shot.png", png))
        return love.filesystem.getSaveDirectory() .. "/shot.png"
    end
    function ne.save(path)
        app.save(path)
        return path or love.filesystem.getSaveDirectory() .. "/last.json"
    end
    if config.debug then
        config.debug(ne)
    end
    return ne
end
return M
