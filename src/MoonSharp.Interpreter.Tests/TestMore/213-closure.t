#! /usr/bin/lua
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--
-- Copyright (C) 2009-2010, Perrad Francois
--
-- This code is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--

--[[

=head1 Lua closures

=head2 Synopsis

    % prove 213-closure.t

=head2 Description

See "Lua 5.2 Reference Manual", section 3.5 "Visibility Rules",
L<http://www.lua.org/manual/5.2/manual.html#3.5>.

See "Programming in Lua", section 6.1 "Closures".

=cut

--]]

require 'Test.More'

plan(15)

--[[ inc ]]
local counter = 0

function inc (x)
    counter = counter + x
    return counter
end

is(inc(1), 1, "inc")
is(inc(2), 3)

--[[ newCounter ]]
function newCounter ()
    local i = 0
    return function ()  -- anonymous function
               i = i + 1
               return i
           end
end

c1 = newCounter()
is(c1(), 1, "newCounter")
is(c1(), 2)

c2 = newCounter()
is(c2(), 1)
is(c1(), 3)
is(c2(), 2)

--[[
The loop creates ten closures (that is, ten instances of the anonymous
function). Each of these closures uses a different y variable, while all
of them share the same x.
]]
a = {}
local x = 20
for i=1,10 do
    local y = 0
    a[i] = function () y=y+1; return x+y end
end

is(a[1](), 21, "ten closures")
is(a[1](), 22)
is(a[2](), 21)


--[[ add ]]
function add(x)
    return function (y) return (x + y) end
end

f = add(2)
type_ok(f, 'function', "add")
is(f(10), 12)
g = add(5)
is(g(1), 6)
is(g(10), 15)
is(f(1), 3)

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
