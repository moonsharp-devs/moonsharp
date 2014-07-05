
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--

--[[
    require 'Test.More'
    require 'socket'
    local conn = socket.connect(host, port)
    require 'Test.Builder.SocketOutput'.init(conn)
    -- now, as usual
    plan(...)
    ...
--]]

local assert = assert

local tb = require 'Test.Builder'.new()
local m = getmetatable(tb)
_ENV = nil

function m.init (sock)
    tb:output(sock)
    tb:failure_output(sock)
    tb:todo_output(sock)
end

function m.puts (sock, str)
    assert(sock:send(str))
end

return m
--
-- Copyright (c) 2011-2012 Francois Perrad
--
-- This library is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--
