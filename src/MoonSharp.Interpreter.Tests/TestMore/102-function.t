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

=head1 Lua function & coercion

=head2 Synopsis

    % prove 102-function.t

=head2 Description

=cut

--]]

require 'Test.More'

plan(51)

f = function () return 1 end

error_like(function () return -f end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "-f")

error_like(function () f = print; return -f end,
           "^[^:]+:%d+: attempt to perform arithmetic on")

error_like(function () return #f end,
           "^[^:]+:%d+: attempt to get length of",
           "#f")

error_like(function () f = print; return #f end,
           "^[^:]+:%d+: attempt to get length of")

is(not f, false, "not f")

is(not print, false)

error_like(function () return f + 10 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "f + 10")

error_like(function () f = print; return f + 10 end,
           "^[^:]+:%d+: attempt to perform arithmetic on")

error_like(function () return f - 2 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "f - 2")

error_like(function () f = print; return f - 2 end,
           "^[^:]+:%d+: attempt to perform arithmetic on")

error_like(function () return f * 3.14 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "f * 3.14")

error_like(function () f = print; return f * 3.14 end,
           "^[^:]+:%d+: attempt to perform arithmetic on")

error_like(function () return f / -7 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "f / -7")

error_like(function () f = print; return f / -7 end,
           "^[^:]+:%d+: attempt to perform arithmetic on")

error_like(function () return f % 4 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "f % 4")

error_like(function () f = print; return f % 4 end,
           "^[^:]+:%d+: attempt to perform arithmetic on")

error_like(function () return f ^ 3 end,
           "^[^:]+:%d+: attempt to perform arithmetic on",
           "f ^ 3")

error_like(function () f = print; return f ^ 3 end,
           "^[^:]+:%d+: attempt to perform arithmetic on")

error_like(function () return f .. 'end' end,
           "^[^:]+:%d+: attempt to concatenate",
           "f .. 'end'")

error_like(function () f = print; return f .. 'end' end,
           "^[^:]+:%d+: attempt to concatenate")

g = f
is(f == g, true, "f == f")

g = print
is(g == print, true)

g = function () return 2 end
is(f ~= g, true, "f ~= g")
h = type
is(f ~= h, true)

is(print ~= g, true)
is(print ~= h, true)

is(f == 1, false, "f == 1")

is(print == 1, false)

is(f ~= 1, true, "f ~= 1")

is(print ~= 1, true)

error_like(function () return f < g end,
           "^[^:]+:%d+: attempt to compare two function values",
           "f < g")

error_like(function () f = print; g = type; return f < g end,
           "^[^:]+:%d+: attempt to compare two function values")

error_like(function () return f <= g end,
           "^[^:]+:%d+: attempt to compare two function values",
           "f <= g")

error_like(function () f = print; g = type; return f <= g end,
           "^[^:]+:%d+: attempt to compare two function values")

error_like(function () return f > g end,
           "^[^:]+:%d+: attempt to compare two function values",
           "f > g")

error_like(function () f = print; g = type; return f > g end,
           "^[^:]+:%d+: attempt to compare two function values")

error_like(function () return f >= g end,
           "^[^:]+:%d+: attempt to compare two function values",
           "f >= g")

error_like(function () f = print; g = type; return f >= g end,
           "^[^:]+:%d+: attempt to compare two function values")

error_like(function () return f < 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "f < 0")

error_like(function () f = print; return f < 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+")

error_like(function () return f <= 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "f <= 0")

error_like(function () f = print; return f <= 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+")

error_like(function () return f > 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "f > 0")

error_like(function () f = print; return f > 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+")

error_like(function () return f > 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "f >= 0")

error_like(function () f = print; return f >= 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+")

error_like(function () a = f; b = a[1]; end,
           "^[^:]+:%d+: attempt to index",
           "index")

error_like(function () a = print; b = a[1]; end,
           "^[^:]+:%d+: attempt to index")

error_like(function () a = f; a[1] = 1; end,
           "^[^:]+:%d+: attempt to index",
           "index")

error_like(function () a = print; a[1] = 1; end,
           "^[^:]+:%d+: attempt to index")

t = {}
t[print] = true
ok(t[print])

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
