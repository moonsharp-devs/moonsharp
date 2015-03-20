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

=head1 Lua tables

=head2 Synopsis

    % prove 221-table.t

=head2 Description

See "Programming in Lua", section 2.5 "Tables".

=cut

--]]

require 'Test.More'

plan(25)

--[[ ]]
a = {}
k = 'x'
a[k] = 10
a[20] = 'great'
is(a['x'], 10)
k = 20
is(a[k], 'great')
a['x'] = a ['x'] + 1
is(a['x'], 11)

--[[ ]]
a = {}
a['x'] = 10
b = a
is(b['x'], 10)
b['x'] = 20
is(a['x'], 20)
a = nil
b = nil

--[[ ]]
a = {}
for i=1,1000 do a[i] = i*2 end
is(a[9], 18)
a['x'] = 10
is(a['x'], 10)
is(a['y'], nil)

--[[ ]]
a = {}
x = 'y'
a[x] = 10
is(a[x], 10)
is(a.x, nil)
is(a.y, 10)

--[[ ]]
i = 10; j = '10'; k = '+10'
a = {}
a[i] = "one value"
a[j] = "another value"
a[k] = "yet another value"
is(a[j], "another value")
is(a[k], "yet another value")
is(a[tonumber(j)], "one value")
is(a[tonumber(k)], "one value")

t = { {'a','b','c'}, 10 }
is(t[2], 10)
is(t[1][3], 'c')
t[1][1] = 'A'
is(table.concat(t[1],','), 'A,b,c')

--[[ ]]
local tt
tt = { {'a','b','c'}, 10 }
is(tt[2], 10)
is(tt[1][3], 'c')
tt[1][1] = 'A'
is(table.concat(tt[1],','), 'A,b,c')

--[[ ]]
a = {}
error_like(function () a() end,
           "^[^:]+:%d+: attempt to call")

--[[ ]]
local tt
tt = { {'a','b','c'}, 10 }
is((tt)[2], 10)
is((tt[1])[3], 'c');
(tt)[1][2] = 'B'
(tt[1])[3] = 'C'
is(table.concat(tt[1],','), 'a,B,C')

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
