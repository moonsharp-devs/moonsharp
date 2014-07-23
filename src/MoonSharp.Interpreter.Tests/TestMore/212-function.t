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

=head1 Lua functions

=head2 Synopsis

    % prove 212-function.t

=head2 Description

See "Lua 5.2 Reference Manual", section 3.4.10 "Function Definitions",
L<http://www.lua.org/manual/5.2/manual.html#3.4.10>.

See "Programming in Lua", section 5 "Functions".

=cut

--]]

require 'Test.More'

plan(63)

--[[ add ]]
function add (a)
    local sum = 0
    for i,v in ipairs(a) do
        sum = sum + v
    end
    return sum
end

t = { 10, 20, 30, 40 }
is(add(t), 100, "add")

--[[ f ]]
function f(a, b) return a or b end

is(f(3), 3, "f")
is(f(3, 4), 3)
is(f(3, 4, 5), 3)

--[[ incCount ]]
count = 0

function incCount (n)
    n = n or 1
    count = count + n
end

is(count, 0, "inCount")
incCount()
is(count, 1)
incCount(2)
is(count, 3)
incCount(1)
is(count, 4)

--[[ maximum ]]
function maximum (a)
    local mi = 1                -- maximum index
    local m = a[mi]             -- maximum value
    for i,val in ipairs(a) do
        if val > m then
            mi = i
            m = val
        end
    end
    return m, mi
end

local m, mi = maximum({8,10,23,12,5})
is(m, 23, "maximum")
is(mi, 3)

--[[ call by value ]]
function f (n)
    n = n - 1
    return n
end

a = 12
is(a, 12, "call by value")
b = f(a)
is(b, 11)
is(a, 12)
c = f(12)
is(c, 11)
is(a, 12)

--[[ call by ref ]]
function f (t)
    t[#t+1] = 'end'
    return t
end

a = { 'a', 'b', 'c' }
is(table.concat(a, ','), 'a,b,c', "call by ref")
b = f(a)
is(table.concat(b, ','), 'a,b,c,end')
is(table.concat(a, ','), 'a,b,c,end')

--[[ var args ]]
local function g(a, b, ...)
    local arg = {...}
    is(a, 3, "vararg")
    is(b, nil)
    is(#arg, 0)
    is(arg[1], nil)
end
g(3)

local function g(a, b, ...)
    local arg = {...}
    is(a, 3)
    is(b, 4)
    is(#arg, 0)
    is(arg[1], nil)
end
g(3, 4)

local function g(a, b, ...)
    local arg = {...}
    is(a, 3)
    is(b, 4)
    is(#arg, 2)
    is(arg[1], 5)
    is(arg[2], 8)
end
g(3, 4, 5, 8)

--[[ var args ]]
local function g(a, b, ...)
    local c, d, e = ...
    is(a, 3, "var args")
    is(b, nil)
    is(c, nil)
    is(d, nil)
    is(e, nil)
end
g(3)

local function g(a, b, ...)
    local c, d, e = ...
    is(a, 3)
    is(b, 4)
    is(c, nil)
    is(d, nil)
    is(e, nil)
end
g(3, 4)

local function g(a, b, ...)
    local c, d, e = ...
    is(a, 3)
    is(b, 4)
    is(c, 5)
    is(d, 8)
    is(e, nil)
end

--[[ var args ]]
local function g(a, b, ...)
    is(#{a, b, ...}, 1, "varargs")
end
g(3)

local function g(a, b, ...)
    is(#{a, b, ...}, 2)
end
g(3, 4)

local function g(a, b, ...)
    is(#{a, b, ...}, 4)
end
g(3, 4, 5, 8)

--[[ var args ]]
function f() return 1, 2 end
function g() return 'a', f() end
function h() return f(), 'b' end
function k() return 'c', (f()) end

x, y = f()
is(x, 1, "var args")
is(y, 2)
x, y, z = g()
is(x, 'a')
is(y, 1)
is(z, 2)
x, y = h()
is(x, 1)
is(y, 'b')
x, y, z = k()
is(x, 'c')
is(y, 1)
is(z, nil)


--[[ invalid var args ]]
f, msg = load [[
function f ()
    print(...)
end
]]
like(msg, "^[^:]+:%d+: cannot use '...' outside a vararg function", "invalid var args")

--[[ tail call ]]
output = {}
local function foo (n)
    output[#output+1] = n
    if n > 0 then
        return foo(n -1)
    end
    return 'end', 0
end

eq_array({foo(3)}, {'end', 0}, "tail call")
eq_array(output, {3, 2, 1, 0})

--[[ no tail call ]]
output = {}
local function foo (n)
    output[#output+1] = n
    if n > 0 then
        return (foo(n -1))
    end
    return 'end', 0
end

is(foo(3), 'end', "no tail call")
eq_array(output, {3, 2, 1, 0})

--[[ no tail call ]]
output = {}
local function foo (n)
    output[#output+1] = n
    if n > 0 then
        foo(n -1)
    end
end

is(foo(3), nil, "no tail call")
eq_array(output, {3, 2, 1, 0})

--[[ sub name ]]
local function f () return 1 end
is(f(), 1, "sub name")

local function f () return 2 end
is(f(), 2)

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
