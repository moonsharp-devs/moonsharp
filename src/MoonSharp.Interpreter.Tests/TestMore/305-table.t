#! /usr/bin/lua
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--
-- Copyright (C) 2009-2013, Perrad Francois
--
-- This code is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--

--[[

=head1 Lua Table Library

=head2 Synopsis

    % prove 305-table.t

=head2 Description

Tests Lua Table Library

See "Lua 5.2 Reference Manual", section 6.5 "Table Manipulation",
L<http://www.lua.org/manual/5.2/manual.html#6.5>.

See "Programming in Lua", section 19 "The Table Library".

=cut

--]]

require 'Test.More'

plan(44)

t = {'a','b','c','d','e'}
is(table.concat(t), 'abcde', "function concat")
is(table.concat(t, ','), 'a,b,c,d,e')
is(table.concat(t, ',',2), 'b,c,d,e')
is(table.concat(t, ',', 2, 4), 'b,c,d')
is(table.concat(t, ',', 4, 2), '')

t = {'a','b',3,'d','e'}
is(table.concat(t,','), 'a,b,3,d,e', "function concat (number)")

t = {'a','b','c','d','e'}
error_like(function () table.concat(t, ',', 2, 7) end,
           "^[^:]+:%d+: invalid value %(nil%) at index 6 in table for 'concat'",
           "function concat (out of range)")

t = {'a','b',true,'d','e'}
error_like(function () table.concat(t, ',') end,
           "^[^:]+:%d+: invalid value %(boolean%) at index 3 in table for 'concat'",
           "function concat (non-string)")

a = {10, 20, 30}
table.insert(a, 1, 15)
is(table.concat(a,','), '15,10,20,30', "function insert")
t = {}
table.insert(t, 'a')
is(table.concat(t, ','), 'a')
table.insert(t, 'b')
is(table.concat(t, ','), 'a,b')
table.insert(t, 1, 'c')
is(table.concat(t, ','), 'c,a,b')
table.insert(t, 2, 'd')
is(table.concat(t, ','), 'c,d,a,b')
table.insert(t, 5, 'e')
is(table.concat(t, ','), 'c,d,a,b,e')

if jit then
    todo("LuaJIT TODO. table.insert.", 2)
end
error_like(function () table.insert(t, 7, 'f') end,
           "^[^:]+:%d+: bad argument #2 to 'insert' %(position out of bounds%)",
           "function insert (out of bounds)")

error_like(function () table.insert(t, -9, 'f') end,
           "^[^:]+:%d+: bad argument #2 to 'insert' %(position out of bounds%)",
           "function insert (out of bounds)")

error_like(function () table.insert(t, 2, 'g', 'h')  end,
           "^[^:]+:%d+: wrong number of arguments to 'insert'",
           "function insert (too many arg)")

if (platform and platform.compat) or jit then
    t = {}
    is(table.maxn(t), 0, "function maxn")
    t[1] = 'a'
    t[2] = 'b'
    is(table.maxn(t), 2)
    t[6] = 'g'
    is(table.maxn(t), 6)
    a = {}
    a[10000] = 1
    is(table.maxn(a), 10000)
else
    is(table.maxn, nil, "maxn (removed)")
    skip("maxn (removed)", 3)
end

t = table.pack("abc", "def", "ghi")
eq_array(t, {
    "abc",
    "def",
    "ghi"
}, "function pack")
is(t.n, 3)

t = table.pack()
eq_array(t, {}, "function pack (no element)")
is(t.n, 0)

t = {}
a = table.remove(t)
is(a, nil, "function remove")
t = {'a','b','c','d','e'}
a = table.remove(t)
is(a, 'e')
is(table.concat(t, ','), 'a,b,c,d')
a = table.remove(t,3)
is(a, 'c')
is(table.concat(t, ','), 'a,b,d')
a = table.remove(t,1)
is(a, 'a')
is(table.concat(t, ','), 'b,d')

if jit then
    todo("LuaJIT TODO. table.remove.", 1)
end
error_like(function () table.remove(t,7) end,
           "^[^:]+:%d+: bad argument #1 to 'remove' %(position out of bounds%)",
           "function remove (out of bounds)")

lines = {
    luaH_set = 10,
    luaH_get = 24,
    luaH_present = 48,
}
a = {}
for n in pairs(lines) do a[#a + 1] = n end
table.sort(a)
output = {}
for _, n in ipairs(a) do
    table.insert(output, n)
end
eq_array(output, {'luaH_get', 'luaH_present', 'luaH_set'}, "function sort")

function pairsByKeys (t, f)
    local a = {}
    for n in pairs(t) do a[#a + 1] = n end
    table.sort(a, f)
    local i = 0     -- iterator variable
    return function ()  -- iterator function
        i = i + 1
        return a[i], t[a[i]]
    end
end

output = {}
for name, line in pairsByKeys(lines) do
    table.insert(output, name)
    table.insert(output, line)
end
eq_array(output, {'luaH_get', 24, 'luaH_present', 48, 'luaH_set', 10}, "function sort")

output = {}
for name, line in pairsByKeys(lines, function (a, b) return a < b end) do
    table.insert(output, name)
    table.insert(output, line)
end
eq_array(output, {'luaH_get', 24, 'luaH_present', 48, 'luaH_set', 10}, "function sort")



function permgen (a, n)
    n = n or #a
    if n <= 1 then
        coroutine.yield(a)
    else
        for i=1,n do
            a[n], a[i] = a[i], a[n]
            permgen(a, n - 1)
            a[n], a[i] = a[i], a[n]
        end
    end
end

function permutations (a)
    local co = coroutine.create(function () permgen(a) end)
    return function ()
               local code, res = coroutine.resume(co)
               return res
           end
end

local t = {}
output = {}
for _, v in ipairs{'a', 'b', 'c', 'd', 'e', 'f', 'g'} do
    table.insert(t, v)
    local ref = table.concat(t, ' ')
    table.insert(output, ref)
    local n = 0
    for p in permutations(t) do
        local c = {}
        for i, v in ipairs(p) do
            c[i] = v
        end
        table.sort(c)
        assert(ref == table.concat(c, ' '), table.concat(p, ' '))
        n = n + 1
    end
    table.insert(output, n)
end

eq_array(output, {
    'a', 1,
    'a b', 2,
    'a b c', 6,
    'a b c d', 24,
    'a b c d e', 120,
    'a b c d e f', 720,
    'a b c d e f g', 5040,
}, "function sort (all permutations)")

--[[

MoonSharp: Sort callbacks work --]]


error_like(function ()
    local t = { 1 }
    table.sort( { t, t, t, t, }, function (a, b) return a[1] == b[1] end )
           end,
           "^[^:]+:%d+: invalid order function for sorting",
           "function sort (bad func)")


eq_array({table.unpack({})}, {}, "function unpack")
eq_array({table.unpack({'a'})}, {'a'})
eq_array({table.unpack({'a','b','c'})}, {'a','b','c'})
eq_array({(table.unpack({'a','b','c'}))}, {'a'})
eq_array({table.unpack({'a','b','c','d','e'},2,4)}, {'b','c','d'})
eq_array({table.unpack({'a','b','c'},2,4)}, {'b','c'})

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:


