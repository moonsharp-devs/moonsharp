#! /usr/bin/lua
--
-- lua-TestMore : <http://fperrad.github.com/lua-TestMore/>
--
-- Copyright (C) 2010-2013, Perrad Francois
--
-- This code is licensed under the terms of the MIT/X11 license,
-- like Lua itself.
--

--[[

=head1 Lua Grammar

=head2 Synopsis

    % prove 204-grammar.t

=head2 Description

See "Lua 5.2 Reference Manual", section 9 "The Complete Syntax of Lua",
L<http://www.lua.org/manual/5.2/manual.html#9>.

=cut

--]]

require 'Test.More'

plan(6)

--[[ empty statement ]]
f, msg = load [[; a = 1]]
type_ok(f, 'function', "empty statement")

--[[ orphan break ]]
f, msg = load [[
function f()
    print "before"
    do
        print "inner"
        break
    end
    print "after"
end
]]
if jit then
    like(msg, "^[^:]+:%d+: no loop to break", "orphan break")
else
    like(msg, "^[^:]+:%d+: <break> at line 5 not inside a loop", "orphan break")
end

--[[ break anywhere ]]
lives_ok( [[
function f()
    print "before"
    while true do
        print "inner"
        break
        print "break"
    end
    print "after"
end
]], "break anywhere")

--[[ goto ]]
f, msg = load [[
::label::
goto unknown
]]
if jit then
    like(msg, ":%d+: undefined label 'unknown'", "unknown goto")
else
    like(msg, ":%d+: no visible label 'unknown' for <goto> at line %d+", "unknown goto")
end

f, msg = load [[
::label::
goto label
::label::
]]
if jit then
    like(msg, ":%d+: duplicate label 'label'", "duplicate label")
else
    like(msg, ":%d+: label 'label' already defined on line %d+", "duplicate label")
end

f, msg = load [[
::e::
goto f
local x
::f::
goto e
]]
if jit then
    like(msg, ":%d+: <goto f> jumps into the scope of local 'x'", "bad goto")
else
    like(msg, ":%d+: <goto f> at line %d+ jumps into the scope of local 'x'", "bad goto")
end

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
