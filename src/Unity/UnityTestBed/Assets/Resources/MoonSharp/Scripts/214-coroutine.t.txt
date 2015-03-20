#! /usr/bin/lua
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--
-- Copyright (C) 2009-2011, Perrad Francois
--
-- This code is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--

--[[

=head1 Lua coroutines

=head2 Synopsis

    % prove 214-coroutine.t

=head2 Description

See "Lua 5.2 Reference Manual", section 2.6 "Coroutines",
L<http://www.lua.org/manual/5.2/manual.html#2.6>.

See "Programming in Lua", section 9 "Coroutines".

=cut

--]]

require 'Test.More'

plan(30)

--[[ ]]
output = {}

function foo1 (a)
    output[#output+1] = "foo " .. a
    return coroutine.yield(2*a)
end

co = coroutine.create(function (a,b)
        output[#output+1] = "co-body " .. a .." " .. b
        local r = foo1(a+1)
        output[#output+1] = "co-body " .. r
        local r, s = coroutine.yield(a+b, a-b)
        output[#output+1] = "co-body " .. r .. " " .. s
        return b, 'end'
    end)

eq_array({coroutine.resume(co, 1, 10)}, {true, 4}, "foo1")
eq_array({coroutine.resume(co, 'r')}, {true, 11, -9})
eq_array({coroutine.resume(co, "x", "y")}, {true, 10, 'end'})
eq_array({coroutine.resume(co, "x", "y")}, {false, "cannot resume dead coroutine"})
eq_array(output, {
    'co-body 1 10',
    'foo 2',
    'co-body r',
    'co-body x y',
})

--[[ ]]
co = coroutine.create(function ()
        output = 'hi'
    end)
like(co, '^thread: 0?[Xx]?%x+$', "basics")

is(coroutine.status(co), 'suspended')
output = ''
coroutine.resume(co)
is(output, 'hi')
is(coroutine.status(co), 'dead')

error_like(function () coroutine.create(true) end,
           "^[^:]+:%d+: bad argument #1 to 'create' %(function expected, got boolean%)")

error_like(function () coroutine.resume(true) end,
           "^[^:]+:%d+: bad argument #1 to 'resume' %(coroutine expected, got boolean%)")

error_like(function () coroutine.status(true) end,
           "^[^:]+:%d+: bad argument #1 to 'status' %(coroutine expected, got boolean%)")

--[[ ]]
output = {}
co = coroutine.create(function ()
        for i=1,10 do
            output[#output+1] = i
            coroutine.yield()
        end
    end)

coroutine.resume(co)
thr, ismain = coroutine.running(co)
type_ok(thr, 'thread', "running")
is(ismain, true, "running")
is(coroutine.status(co), 'suspended', "basics")
coroutine.resume(co)
coroutine.resume(co)
coroutine.resume(co)
coroutine.resume(co)
coroutine.resume(co)
coroutine.resume(co)
coroutine.resume(co)
coroutine.resume(co)
coroutine.resume(co)
coroutine.resume(co)
eq_array({coroutine.resume(co)}, {false, 'cannot resume dead coroutine'})
eq_array(output, {1,2,3,4,5,6,7,8,9,10})

--[[ ]]
co = coroutine.create(function (a,b)
        coroutine.yield(a + b, a - b)
    end)

eq_array({coroutine.resume(co, 20, 10)}, {true, 30, 10}, "basics")

--[[ ]]
co = coroutine.create(function ()
        return 6, 7
    end)

eq_array({coroutine.resume(co)}, {true, 6, 7}, "basics")

--[[ ]]
co = coroutine.wrap(function(...)
  return pcall(function(...)
    return coroutine.yield(...)
  end, ...)
end)
eq_array({co("Hello")}, {"Hello"})
eq_array({co("World")}, {true, "World"})

--[[ ]]
co = coroutine.wrap(function(...)
  function backtrace ()
    return 'not a back trace'
  end
  return xpcall(function(...)
    return coroutine.yield(...)
  end, backtrace, ...)
end)
eq_array({co("Hello")}, {"Hello"})
eq_array({co("World")}, {true, "World"})


--[[ ]]
local output = {}
co = coroutine.wrap(function()
  while true do
    local t = setmetatable({}, {
      __eq = function(...)
        return coroutine.yield(...)
      end}
    )
    local t2 = setmetatable({}, getmetatable(t))
    output[#output+1] = t == t2
  end
end)
co()
co(true)
co(false)
eq_array(output, {true, false})

--[[ ]]
co = coroutine.wrap(print)
type_ok(co, 'function')

error_like(function () coroutine.wrap(true) end,
           "^[^:]+:%d+: bad argument #1 to 'wrap' %(function expected, got boolean%)")

co = coroutine.wrap(function () error"in coro" end)
error_like(function () co() end,
           "^[^:]+:%d+: in coro$")

--[[ ]]
co = coroutine.create(function ()
        error "in coro"
    end)
r, msg = coroutine.resume(co)
is(r, false)
like(msg, "in coro$")

--[[ ]]
error_like(function () coroutine.yield() end,
           "attempt to yield")


-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
