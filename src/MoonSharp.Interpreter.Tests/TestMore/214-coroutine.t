#! /usr/bin/lua
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--
-- Copyright (C) 2009, Perrad Francois
--
-- This code is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--

--[[

=head1 Lua coroutines

=head2 Synopsis

    % prove 214-coroutine.t

=head2 Description

See "Lua 5.1 Reference Manual", section 2.11 "Coroutines",
L<http://www.lua.org/manual/5.1/manual.html#2.11>.

See "Programming in Lua", section 9 "Coroutines".

=cut

--]]

require 'Test.More'

plan(14)

-- [=[ foo1 ]]
output = {}

function foo1 (a)
    table.insert(output, "foo " .. a)
    return coroutine.yield(2*a)
end

co = coroutine.create(function (a,b)
        table.insert(output, "co-body " .. a .." " .. b)
        local r = foo1(a+1)
        table.insert(output, "co-body " .. r)
        local r, s = coroutine.yield(a+b, a-b)
        table.insert(output, "co-body " .. r .. " " .. s)
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

--[[ ]]
output = {}
co = coroutine.create(function ()
        for i=1,10 do
            table.insert(output, i)
            coroutine.yield()
        end
    end)

coroutine.resume(co)
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

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
