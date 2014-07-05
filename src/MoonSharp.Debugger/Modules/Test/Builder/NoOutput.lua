
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--

local io = require 'io'
local pairs = pairs
local require = require

_ENV = nil
local m = {}

function m.create ()
    local tb = require 'Test.Builder'.create()
    tb:output(io.tmpfile())
    tb:failure_output(io.tmpfile())
    tb:todo_output(io.tmpfile())

    function tb:read (stream)
        if     stream == 'out' then
            local f = self:output()
            f:seek 'set'
            local out = f:read '*a'
            f:close()
            self:output(io.tmpfile())
            return out
        elseif stream == 'err' then
            local f = self:failure_output()
            f:seek 'set'
            local out = f:read '*a'
            f:close()
            self:failure_output(io.tmpfile())
            return out
        elseif stream == 'todo' then
            local f = self:todo_output()
            f:seek 'set'
            local out = f:read '*a'
            f:close()
            self:todo_output(io.tmpfile())
            return out
        else
            self:output():close()
            self:output(io.tmpfile())
            self:failure_output():close()
            self:failure_output(io.tmpfile())
            self:todo_output():close()
            self:todo_output(io.tmpfile())
        end
    end

    return tb
end

return m
--
-- Copyright (c) 2009-2012 Francois Perrad
--
-- This library is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--
