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

=head1  Lua boolean & coercion

=head2 Synopsis

    % prove 101-boolean.t

=head2 Description

=cut

]]

require 'Test.More'

plan(24)

error_like(function () return -true end,
           "^[^:]+:%d+: attempt to perform arithmetic on a %w+ value",
           "-true")

error_like(function () return #true end,
           "^[^:]+:%d+: attempt to get length of a boolean value",
           "#true")

is(not false, true, "not false")

error_like(function () return true + 10 end,
           "^[^:]+:%d+: attempt to perform arithmetic on a boolean value",
           "true + 10")

error_like(function () return true - 2 end,
           "^[^:]+:%d+: attempt to perform arithmetic on a boolean value",
           "true - 2")

error_like(function () return true * 3.14 end,
           "^[^:]+:%d+: attempt to perform arithmetic on a boolean value",
           "true * 3.14")

error_like(function () return true / -7 end,
           "^[^:]+:%d+: attempt to perform arithmetic on a boolean value",
           "true / -7")

error_like(function () return true % 4 end,
           "^[^:]+:%d+: attempt to perform arithmetic on a boolean value",
           "true % 4")

error_like(function () return true ^ 3 end,
           "^[^:]+:%d+: attempt to perform arithmetic on a boolean value",
           "true ^ 3")

error_like(function () return true .. 'end' end,
           "^[^:]+:%d+: attempt to concatenate a boolean value",
           "true .. 'end'")

is(true == true, true, "true == true")

is(true ~= false, true, "true ~= false")

is(true == 1, false, "true == 1")

is(true ~= 1, true, "true ~= 1")

error_like(function () return true < false end,
           "^[^:]+:%d+: attempt to compare two boolean values",
           "true < false")

error_like(function () return true <= false end,
           "^[^:]+:%d+: attempt to compare two boolean values",
           "true <= false")

error_like(function () return true > false end,
           "^[^:]+:%d+: attempt to compare two boolean values",
           "true > false")

error_like(function () return true >= false end,
           "^[^:]+:%d+: attempt to compare two boolean values",
           "true >= false")

error_like(function () return true < 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "true < 0")

error_like(function () return true <= 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "true <= 0")

error_like(function () return true > 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "true > 0")

error_like(function () return true >= 0 end,
           "^[^:]+:%d+: attempt to compare %w+ with %w+",
           "true >= 0")

error_like(function () a = true; b = a[1]; end,
           "^[^:]+:%d+: attempt to index",
           "index")

error_like(function () a = true; a[1] = 1; end,
           "^[^:]+:%d+: attempt to index",
           "index")

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
