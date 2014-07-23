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

=head1 Lua Basic Library

=head2 Synopsis

    % prove 301-basic.t

=head2 Description

Tests Lua Basic Library

See "Lua 5.2 Reference Manual", section 6.1 "Basic Functions",
L<http://www.lua.org/manual/5.2/manual.html#6.1>.

=cut

--]]

require 'Test.More'

plan(168)

if jit then
    like(_VERSION, '^Lua 5%.1$', "variable _VERSION")
else
    like(_VERSION, '^Lua 5%.2$', "variable _VERSION")
end

v, msg = assert('text', "assert string")
is(v, 'text', "function assert")
is(msg, "assert string")
v, msg = assert({}, "assert table")
is(msg, "assert table")

error_like(function () assert(false, "ASSERTION TEST") end,
           "^[^:]+:%d+: ASSERTION TEST",
           "function assert(false, msg)")

error_like(function () assert(false) end,
           "^[^:]+:%d+: assertion failed!",
           "function assert(false)")

error_like(function () assert(false, nil) end,
           "^[^:]+:%d+: assertion failed!",
           "function assert(false, nil)")

is(collectgarbage('stop'), 0, "function collectgarbage 'stop/restart/collect'")
if jit then
    skip("LuaJIT. gc isrunning", 1)
else
    is(collectgarbage('isrunning'), false)
end
is(collectgarbage('step'), false)
is(collectgarbage('restart'), 0)
if jit then
    skip("LuaJIT. gc isrunning", 1)
else
    is(collectgarbage('isrunning'), true)
end
is(collectgarbage('step'), false)
is(collectgarbage('collect'), 0)
is(collectgarbage('setpause', 10), 200)
is(collectgarbage('setstepmul', 200), 200)
is(collectgarbage(), 0)
if jit then
    skip("LuaJIT. gc mode gen/inc", 4)
else
    is(collectgarbage('generational'), 0)
    is(collectgarbage('step'), false)
    is(collectgarbage('incremental'), 0)
    is(collectgarbage('setmajorinc'), 200)
end

type_ok(collectgarbage('count'), 'number', "function collectgarbage 'count'")

error_like(function () collectgarbage('unknown') end,
           "^[^:]+:%d+: bad argument #1 to 'collectgarbage' %(invalid option 'unknown'%)",
           "function collectgarbage (invalid)")

f = io.open('lib1.lua', 'w')
f:write[[
function norm (x, y)
    return (x^2 + y^2)^0.5
end

function twice (x)
    return 2*x
end
]]
f:close()
dofile('lib1.lua')
n = norm(3.4, 1.0)
like(twice(n), '^7%.088', "function dofile")

os.remove('lib1.lua') -- clean up

error_like(function () dofile('no_file.lua') end,
           "cannot open no_file.lua: No such file or directory",
           "function dofile (no file)")

f = io.open('foo.lua', 'w')
f:write[[?syntax error?]]
f:close()
error_like(function () dofile('foo.lua') end,
           "^foo%.lua:%d+:",
           "function dofile (syntax error)")
os.remove('foo.lua') -- clean up

a = {'a','b','c'}
local f, v, s = ipairs(a)
type_ok(f, 'function', "function ipairs")
type_ok(v, 'table')
is(s, 0)
s, v = f(a, s)
is(s, 1)
is(v, 'a')
s, v = f(a, s)
is(s, 2)
is(v, 'b')
s, v = f(a, s)
is(s, 3)
is(v, 'c')
s, v = f(a, s)
is(s, nil)
is(v, nil)

t = { [[
function bar (x)
    return x
end
]] }
i = 0
function reader ()
    i = i + 1
    return t[i]
end
f, msg = load(reader)
if msg then
    diag(msg)
end
type_ok(f, 'function', "function load(reader)")
is(bar, nil)
f()
is(bar('ok'), 'ok')
bar = nil

t = { [[
function baz (x)
    return x
end
]] }
i = -1
function reader ()
    i = i + 1
    return t[i]
end
f, msg = load(reader)
if msg then
    diag(msg)
end
type_ok(f, 'function', "function load(pathological reader)")
f()
is(baz, nil)

t = { [[?syntax error?]] }
i = 0
f, msg = load(reader, "errorchunk")
is(f, nil, "function load(syntax error)")
like(msg, "^%[string \"errorchunk\"%]:%d+:")

f = load(function () return nil end)
type_ok(f, 'function', "when reader returns nothing")

f, msg = load(function () return {} end)
is(f, nil, "reader function must return a string")
like(msg, "reader function must return a string")

f = load([[
function bar (x)
    return x
end
]])
is(bar, nil, "function load(str)")
f()
is(bar('ok'), 'ok')
bar = nil

env = {}
f = load([[
function bar (x)
    return x
end
]], "from string", 't', env)
is(env.bar, nil, "function load(str)")
f()
is(env.bar('ok'), 'ok')

f, msg = load([[?syntax error?]], "errorchunk")
is(f, nil, "function load(syntax error)")
like(msg, "^%[string \"errorchunk\"%]:%d+:")

f, msg = load([[print 'ok']], "chunk txt", 'b')
like(msg, "attempt to load")
is(f, nil, "mode")

f, msg = load("\x1bLua", "chunk bin", 't')
like(msg, "attempt to load")
is(f, nil, "mode")

f = io.open('foo.lua', 'w')
f:write'\xEF\xBB\xBF' -- BOM
f:write[[
function foo (x)
    return x
end
]]
f:close()
f = loadfile('foo.lua')
is(foo, nil, "function loadfile")
f()
is(foo('ok'), 'ok')

f, msg = loadfile('foo.lua', 'b')
like(msg, "attempt to load")
is(f, nil, "mode")

env = {}
f = loadfile('foo.lua', 't', env)
is(env.foo, nil, "function loadfile")
f()
is(env.foo('ok'), 'ok')

os.remove('foo.lua') -- clean up

f, msg = loadfile('no_file.lua')
is(f, nil, "function loadfile (no file)")
is(msg, "cannot open no_file.lua: No such file or directory")

f = io.open('foo.lua', 'w')
f:write[[?syntax error?]]
f:close()
f, msg = loadfile('foo.lua')
is(f, nil, "function loadfile (syntax error)")
like(msg, '^foo%.lua:%d+:')
os.remove('foo.lua') -- clean up

if (platform and platform.compat) or jit then
    ok(loadstring[[i = i + 1]], "function loadstring")
else
    is(loadstring, nil, "function loadstring (removed)")
    loadstring = load
end

f = loadstring([[i = i + 1]])
i = 0
f()
is(i, 1, "function loadstring")
f()
is(i, 2)

i = 32
local i = 0
f = loadstring([[i = i + 1; return i]])
g = function () i = i + 1; return i end
is(f(), 33, "function loadstring")
is(g(), 1)

f, msg = loadstring([[?syntax error?]])
is(f, nil, "function loadstring (syntax error)")
like(msg, '^%[string "%?syntax error%?"%]:%d+:')

t = {'a','b','c'}
a = next(t, nil)
is(a, 1, "function next (array)")
a = next(t, 1)
is(a, 2)
a = next(t, 2)
is(a, 3)
a = next(t, 3)
is(a, nil)

error_like(function () a = next() end,
           "^[^:]+:%d+: bad argument #1 to 'next' %(table expected, got no value%)",
           "function next (no arg)")

error_like(function () a = next(t, 6) end,
           "invalid key to 'next'",
           "function next (invalid key)")

t = {'a','b','c'}
a = next(t, 2)
is(a, 3, "function next (unorderer)")
a = next(t, 1)
is(a, 2)
a = next(t, 3)
is(a, nil)

t = {}
a = next(t, nil)
is(a, nil, "function next (empty table)")

a = {'a','b','c'}
local f, v, s = pairs(a)
type_ok(f, 'function', "function pairs")
type_ok(v, 'table')
is(s, nil)
s = f(v, s)
is(s, 1)
s = f(v, s)
is(s, 2)
s = f(v, s)
is(s, 3)
s = f(v, s)
is(s, nil)

r = pcall(assert, true)
is(r, true, "function pcall")
r, msg = pcall(assert, false, 'catched')
is(r, false)
is(msg, 'catched')
r = pcall(assert)
is(r, false)

t = {}
a = t
is(rawequal(nil, nil), true, "function rawequal -> true")
is(rawequal(false, false), true)
is(rawequal(3, 3), true)
is(rawequal('text', 'text'), true)
is(rawequal(t, a), true)
is(rawequal(print, print), true)

is(rawequal(nil, 2), false, "function rawequal -> false")
is(rawequal(false, true), false)
is(rawequal(false, 2), false)
is(rawequal(3, 2), false)
is(rawequal(3, '2'), false)
is(rawequal('text', '2'), false)
is(rawequal('text', 2), false)
is(rawequal(t, {}), false)
is(rawequal(t, 2), false)
is(rawequal(print, format), false)
is(rawequal(print, 2), false)

is(rawlen("text"), 4, "function rawlen (string)")
is(rawlen({ 'a', 'b', 'c'}), 3, "function rawlen (table)")
error_like(function () a = rawlen(true) end,
           "^[^:]+:%d+: bad argument #1 to 'rawlen' %(table ",
           "function rawlen (bad arg)")

t = {a = 'letter a', b = 'letter b'}
is(rawget(t, 'a'), 'letter a', "function rawget")

t = {}
is(rawset(t, 'a', 'letter a'), t, "function rawset")
is(t.a, 'letter a')

error_like(function () t = {}; rawset(t, nil, 42) end,
           "^table index is nil",
           "function rawset (table index is nil)")

is(select('#'), 0, "function select")
is(select('#','a','b','c'), 3)
eq_array({select(1,'a','b','c')}, {'a','b','c'})
eq_array({select(3,'a','b','c')}, {'c'})
eq_array({select(5,'a','b','c')}, {})
eq_array({select(-1,'a','b','c')}, {'c'})
eq_array({select(-2,'a','b','c')}, {'b', 'c'})
eq_array({select(-3,'a','b','c')}, {'a', 'b', 'c'})

error_like(function () select(0,'a','b','c') end,
           "^[^:]+:%d+: bad argument #1 to 'select' %(index out of range%)",
           "function select (out of range)")

error_like(function () select(-4,'a','b','c') end,
           "^[^:]+:%d+: bad argument #1 to 'select' %(index out of range%)",
           "function select (out of range)")

is(type("Hello world"), 'string', "function type")
is(type(10.4*3), 'number')
is(type(print), 'function')
is(type(type), 'function')
is(type(true), 'boolean')
is(type(nil), 'nil')
is(type(io.stdin), 'userdata')
is(type(type(X)), 'string')

a = nil
is(type(a), 'nil', "function type")
a = 10
is(type(a), 'number')
a = "a string!!"
is(type(a), 'string')
a = print
is(type(a), 'function')
is(type(function () end), 'function')

error_like(function () type() end,
           "^[^:]+:%d+: bad argument #1 to 'type' %(value expected%)",
           "function type (no arg)")

is(tonumber('text12'), nil, "function tonumber")
is(tonumber('12text'), nil)
is(tonumber(3.14), 3.14)
is(tonumber('3.14'), 3.14)
is(tonumber('  3.14  '), 3.14)
is(tonumber(111, 2), 7)
is(tonumber('111', 2), 7)
is(tonumber('  111  ', 2), 7)
a = {}
is(tonumber(a), nil)

error_like(function () tonumber() end,
           "^[^:]+:%d+: bad argument #1 to 'tonumber' %(value expected%)",
           "function tonumber (no arg)")

error_like(function () tonumber('111', 200) end,
           "^[^:]+:%d+: bad argument #2 to 'tonumber' %(base out of range%)",
           "function tonumber (bad base)")

is(tostring('text'), 'text', "function tostring")
is(tostring(3.14), '3.14')
is(tostring(nil), 'nil')
is(tostring(true), 'true')
is(tostring(false), 'false')
like(tostring({}), '^table: 0?[Xx]?%x+$')
like(tostring(print), '^function: 0?[Xx]?[builtin]*#?%x+$')

error_like(function () tostring() end,
           "^[^:]+:%d+: bad argument #1 to 'tostring' %(value expected%)",
           "function tostring (no arg)")

if (platform and platform.compat) or jit then
    type_ok(unpack, 'function', "function unpack")
else
    is(unpack, nil, "function unpack (removed)")
end

if jit then
    error_like(function () xpcall(assert, nil) end,
               "bad argument #2 to 'xpcall' %(function expected, got nil%)",
               "function xpcall")
    error_like(function () xpcall(assert) end,
               "bad argument #2 to 'xpcall' %(function expected, got no value%)",
               "function xpcall")
    diag("LuaJIT intentional. xpcall")
else
    is(xpcall(assert, nil), false, "function xpcall")
    error_like(function () xpcall(assert) end,
               "^[^:]+:%d+: bad argument #2 to 'xpcall' %(value expected%)",
               "function xpcall (no arg)")
end

function backtrace ()
    return 'not a back trace'
end
r, msg = xpcall(assert, backtrace)
is(r, false, "function xpcall (backtrace)")
is(msg, 'not a back trace')

r = xpcall(assert, backtrace, true)
is(r, true, "function xpcall")

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
