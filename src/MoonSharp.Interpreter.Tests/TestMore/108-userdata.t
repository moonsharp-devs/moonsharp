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

=head1 Lua userdata & coercion

=head2 Synopsis

    % prove 108-userdata.t

=head2 Description

=cut

--]]

require 'Test.More'

plan(25)

u = io.stdin

error_like(function () return -u end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "-u")

error_like(function () return #u end,
           "^[^:]+:%d+: attempt to get length of",
           "#u")

is(not u, false, "not u")

error_like(function () return u + 10 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "u + 10")

error_like(function () return u - 2 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "u - 2")

error_like(function () return u * 3.14 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "u * 3.14")

error_like(function () return u / 7 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "u / 7")

error_like(function () return u % 4 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "u % 4")

error_like(function () return u ^ 3 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "u ^ 3")

error_like(function () return u .. 'end' end,
           "^[^:]+:%d+: attempt to concatenate",
           "u .. 'end'")

is(u == u, true, "u == u")

v = io.stdout
is(u ~= v, true, "u ~= v")

is(u == 1, false, "u == 1")

is(u ~= 1, true, "u ~= 1")

error_like(function () return u < v end,
           "^[^:]+:%d+: attempt to compare two userdata values",
           "u < v")

error_like(function () return u <= v end,
           "^[^:]+:%d+: attempt to compare two userdata values",
           "u <= v")

error_like(function () return u > v end,
           "^[^:]+:%d+: attempt to compare two userdata values",
           "u > v")

error_like(function () return u >= v end,
           "^[^:]+:%d+: attempt to compare two userdata values",
           "u >= v")

error_like(function () return u < 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "u < 0")

error_like(function () return u <= 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "u <= 0")

error_like(function () return u > 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "u > 0")

error_like(function () return u > 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "u >= 0")

is(u[1], nil, "index")

error_like(function () u[1] = 1 end,
           "^[^:]+:%d+: attempt to index",
           "index")

t = {}
t[u] = true
ok(t[u])

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
