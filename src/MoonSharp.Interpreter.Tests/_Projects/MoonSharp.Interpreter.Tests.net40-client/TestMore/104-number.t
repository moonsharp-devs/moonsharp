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

=head1 Lua number & coercion

=head2 Synopsis

    % prove 104-number.t

=head2 Description

=cut

--]]

require 'Test.More'

plan(54)

is(-1, -(1), "-1")

error_like(function () return #1 end,
           "^[^:]+:%d+: attempt to get length of a number value",
           "#1")

is(not 1, false, "not 1")

is(10 + 2, 12, "10 + 2")

is(2 - 10, -8, "2 - 10")

is(3.14 * 1, 3.14, "3.14 * 1")

is(-7 / 0.5, -14, "-7 / 0.5")

type_ok(1 / 0, 'number', "1 / 0")

is(-25 % 3, 2, "-25 % 3")

type_ok(1 % 0, 'number', "1 % 0")

error_like(function () return 10 + true end,
           "^[^:]+:%d+: attempt to perform arithmetic on a boolean value",
           "10 + true")

error_like(function () return 2 - nil end,
           "^[^:]+:%d+: attempt to perform arithmetic on a nil value",
           "2 - nil")

error_like(function () return 3.14 * false end,
           "^[^:]+:%d+: attempt to perform arithmetic on a boolean value",
           "3.14 * false")

error_like(function () return -7 / {} end,
           "^[^:]+:%d+: attempt to perform arithmetic on a table value",
           "-7 / {}")

error_like(function () return 3 ^ true end,
           "^[^:]+:%d+: attempt to perform arithmetic on a boolean value",
           "3 ^ true")

error_like(function () return -25 % false end,
           "^[^:]+:%d+: attempt to perform arithmetic on a boolean value",
           "-25 % false")

error_like(function () return 10 + 'text' end,
           "^[^:]+:%d+: attempt to perform arithmetic on a string value",
           "10 + 'text'")

error_like(function () return 2 - 'text' end,
           "^[^:]+:%d+: attempt to perform arithmetic on a string value",
           "2 - 'text'")

error_like(function () return 3.14 * 'text' end,
           "^[^:]+:%d+: attempt to perform arithmetic on a string value",
           "3.14 * 'text'")

error_like(function () return -7 / 'text' end,
           "^[^:]+:%d+: attempt to perform arithmetic on a string value",
           "-7 / 'text'")

error_like(function () return 25 % 'text' end,
           "^[^:]+:%d+: attempt to perform arithmetic on a string value",
           "25 % 'text'")

error_like(function () return 3 ^ 'text' end,
           "^[^:]+:%d+: attempt to perform arithmetic on a string value",
           "3 ^ 'text'")

is(10 + '2', 12, "10 + '2'")

is(2 - '10', -8, "2 - '10'")

is(3.14 * '1', 3.14, "3.14 * '1'")

is(-7 / '0.5', -14, "-7 / '0.5'")

is(-25 % '3', 2, "-25 % '3'")

is(3 ^ '3', 27, "3 ^ '3'")

is(1 .. 'end', '1end', "1 .. 'end'")

is(1 .. 2, '12', "1 .. 2")

error_like(function () return 1 .. true end,
           "^[^:]+:%d+: attempt to concatenate a %w+ value",
           "1 .. true")

is(1.0 == 1, true, "1.0 == 1")

is(1 ~= 2, true, "1 ~= 2")

is(1 == true, false, "1 == true")

is(1 ~= nil, true, "1 ~= nil")

is(1 == '1', false, "1 == '1'")

is(1 ~= '1', true, "1 ~= '1'")

is(1 < 0, false, "1 < 0")

is(1 <= 0, false, "1 <= 0")

is(1 > 0, true, "1 > 0")

is(1 >= 0, true, "1 >= 0")

error_like(function () return 1 < false end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "1 < false")

error_like(function () return 1 <= nil end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "1 <= nil")

error_like(function () return 1 > true end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "1 > true")

error_like(function () return 1 >= {} end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "1 >= {}")

error_like(function () return 1 < '0' end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "1 < '0'")

error_like(function () return 1 <= '0' end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "1 <= '0'")

error_like(function () return 1 > '0' end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "1 > '0'")

error_like(function () return 1 >= '0' end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "1 >= '0'")

is(tostring(1000000000), '1000000000', "number 1000000000")

is(tostring(1e9), '1000000000', "number 1e9")

is(tostring(1.0e+9), '1000000000', "number 1.0e+9")

error_like(function () a= 3.14; b = a[1]; end,
           "^[^:]+:%d+: attempt to index",
           "index")

error_like(function () a = 3.14; a[1] = 1; end,
           "^[^:]+:%d+: attempt to index",
           "index")

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
