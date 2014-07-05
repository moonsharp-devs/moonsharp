
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--

local tconcat = require 'table'.concat
local setmetatable = setmetatable

local tb = require 'Test.Builder'.new()

_ENV = nil
local m = {}

function m.new (_type)
    local o = setmetatable({ type = _type }, { __index = m })
    o:reset()
    return o
end

function m:write (...)
    self.got = self.got .. tconcat({...})
end

function m:reset ()
    self.got = ''
    self.wanted = {}
end

function m:expect (...)
    local arg = {...}
    local wanted = self.wanted
    for i = 1, #arg do
        wanted[#wanted+1] = arg[i]
    end
end

function m:check ()
    local got = self.got
    local wanted = tconcat(self.wanted, "\n")
    if wanted ~= '' then
        wanted = wanted .. "\n"
    end
    return got == wanted
end

function m:complaint ()
    local type = self.type
    local got = self.got
    local wanted = tconcat(self.wanted, "\n")
    if wanted ~= '' then
        wanted = wanted .. "\n"
    end
    return type .. " is:"
     .. "\n" .. got
     .. "\nnot:"
     .. "\n" .. wanted
     .. "\nhas expected"
end

return m
--
-- Copyright (c) 2009-2012 Francois Perrad
--
-- This library is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--
