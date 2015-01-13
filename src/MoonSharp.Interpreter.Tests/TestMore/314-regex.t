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

=head1 Lua Regex Compiler

=head2 Synopsis

    % prove 314-regex.t

=head2 Description

Tests Lua Regex

Individual tests are stored in the C<rx_*> files in the same directory;
There is one test per line: each test consists of the following
columns (separated by one *or more* tabs):

=over 4

=item pattern

The Lua regex to test.

=item target

The string that will be matched against the pattern. Use '' to indicate
an empty string.

=item result

The expected result of the match.

=item description

Description of the test.

=back

=cut

--]]

local test_patterns = {
	[===[(a.)..(..)		zzzabcdefzzz	ab\tef			basic match]===], 
	[===[(a(b(c))(d))		abcd		abcd\tbc\tc\td		nested match]===], 
	[===[((%w+))			abcd		abcd\tabcd		nested match]===], 
	[===[(a*(.)%w(%s*))		aa!b c		aa!b \t!\t 		nested match]===], 
	[===[(a?)..			abcd		a			opt]===], 
	[===[(A?)..			abcd		''			opt]===], 
	[===[()aa()			flaaap		3\t5			empty capture]===], 
	[===[(.)%1			bookkeeper	o			backreference]===], 
	[===[(%w+)%s+%1		hello hello	hello			backreference]===], 
	[===[(.*)x			123x		123			repeated dot capture]===], 
	[===[$(%w+)			$abc=		abc			not escaped]===], 
	[===[[c]			abcdef		c		character class]===], 
	[===[^[a]			abcdef		a		anchored character class]===], 
	[===[[^e]			abcdef		a		negated character class]===], 
	[===[^[a]?			abcdef		a		anchored optional character class]===], 
	[===[[^e]?			abcdef		a		negated optional character class]===], 
	[===[^[^e]			abcdef		a		anchored negated character class]===], 
	[===[^[^a]			abcdef		nil		anchored negated character class]===], 
	[===[[b-d]			abcdef		b		character range]===], 
	[===[[b-d]			abxxef		b		character range]===], 
	[===[[b-d]			axcxef		c		character range]===], 
	[===[[b-d]			axxdef		d		character range]===], 
	[===[[b-d]			axxxef		nil		character range]===], 
	[===[[^b-d]			abcdef		a		negated character range]===], 
	[===[[^b-d]			bbccdd		nil		negated character range]===], 
	[===[[-]			ab-def		-		unescaped hyphen]===], 
	[===[[%-]			ab-def		-		escaped hyphen]===], 
	[===[[%-]			abcdef		nil		escaped hyphen]===], 
	[===[[^%-]			---x--		x		negated escaped hyphen]===], 
	[===[[^%-]			------		nil		negated escaped hyphen]===], 
	[===[[%-+]			ab-def		-		escaped hyphen in range]===], 
	[===[[%-+]			ab+def		+		escaped hyphen in range]===], 
	[===[[%-+]			abcdef		nil		escaped hyphen in range]===], 
	[===[[+%-]			ab-def		-		escaped hyphen in range]===], 
	[===[[+%-]			ab+def		+		escaped hyphen in range]===], 
	[===[[+%-]			abcdef		nil		escaped hyphen in range]===], 
	[===[[^%-+]			---x--		x		negated escaped hyphen in range]===], 
	[===[[^%-+]			------		nil		negated escaped hyphen in range]===], 
	[===[[^+%-]			---x--		x		negated escaped hyphen in range]===], 
	[===[[^+%-]			------		nil		negated escaped hyphen in range]===], 
	[===[["\\]			\\		\		escaped backslash]===], 
	[===[[%]]			]		]		escaped close bracket]===], 
	[===[[%]			\\]]		/malformed pattern %(missing ']'%)/	unescaped backslash (or no closing brace)]===], 
	[===[ab\\cd			ab\092cd	ab\cd		literal match with backslash]===], 
	[===[%?			ab<?		?		literal match with question mark]===], 
	[===[[A-Z0-9]		abcdef		nil		two enumerated ranges]===], 
	[===[[A-Z0-9]		abcDef		D		two enumerated ranges]===], 
	[===[.			a		a		dot (.)]===], 
	[===[.			\n		\n		dot (.)]===], 
	[===[.			''		nil		dot (.)]===], 
	[===[a%s+f			abcdef		nil		whitespace (%s)]===], 
	[===[ab%s+cdef		ab  cdef	ab  cdef	whitespace (%s)]===], 
	[===[a%S+f			abcdef		abcdef		not whitespace (%S)]===], 
	[===[a%S+f			ab cdef		nil		not whitespace (%S)]===], 
	[===[^abc			abcdef		abc		start and end of string (^)]===], 
	[===[^abc			abc\ndef	abc		start and end of string (^)]===], 
	[===[^abc			def\nabc	nil		start and end of string (^)]===], 
	[===[def\n^abc		def\nabc	nil		start and end of string (^)]===], 
	[===[def$			abcdef		def		start and end of string ($)]===], 
	[===[def$			abc\ndef	def		start and end of string ($)]===], 
	[===[def$			def\nabc	nil		start and end of string ($)]===], 
	[===[def$\nabc		def\nabc	nil		start and end of string (^)]===], 
	[===[abc\n$			abc\n		abc\n		end of string ($)]===], 
	[===[abc$			abc\n		nil		end of string ($)]===], 
	[===[c\nd			abc\ndef	c\nd		newline (\n)]===], 
	[===[c\nd			abc\010def	c\nd		newline (\n)]===], 
	[===[c\n+d			abc\n\ndef	c\n\nd		newline (\n)]===], 
	[===[a\n+f			abcdef		nil		newline (\n)]===], 
	[===[b\nc			abc\ndef	nil		newline (\n)]===], 
	[===[c\td			abc\tdef	c\td		horizontal tab (\t)]===], 
	[===[c\td			abc\09def	c\td		horizontal tab (\t)]===], 
	[===[c\t+d			abc\t\tdef	c\t\td		horizontal tab (\t)]===], 
	[===[a\t+f			abcdef		nil		horizontal tab (\t)]===], 
	[===[b\tc			abc\tdef	nil		horizontal tab (\t)]===], 
	[===[c\rd			abc\rdef	c\rd		return (\r)]===], 
	[===[c\rd			abc\013def	c\rd		return (\r)]===], 
	[===[c\r+d			abc\r\rdef	c\r\rd		return (\r)]===], 
	[===[a\r+f			abcdef		nil		return (\r)]===], 
	[===[b\rc			abc\rdef	nil		return (\r)]===], 
	[===[c\fd			abc\fdef	c\fd		formfeed (\f)]===], 
	[===[c\fd			abc\012def	c\fd		formfeed (\f)]===], 
	[===[c\f+d			abc\f\fdef	c\f\fd		formfeed (\f)]===], 
	[===[a\f+f			abcdef		nil		formfeed (\f)]===], 
	[===[b\fc			abc\fdef	nil		formfeed (\f)]===], 
	[===[c\033d			abc!def		c!d		dec (\0)]===], 
	[===[c\033d			abc\033def	c!d		dec (\0)]===], 
	[===[c\033+d			abc!!def	c!!d		dec (\0)]===], 
	[===[a\033+f			abcdef		nil		dec (\0)]===], 
	[===[b\033c			abc!def		nil		dec (\0)]===], 
	[===[a%^d			a^d		a^d		escaped (useless)]===], 
	[===[a^d			a^d		a^d		not escaped]===], 
	[===[%^d			^d		^d		escaped]===], 
	[===[a%$d			a$d		a$d		escaped (useless)]===], 
	[===[a$d			a$d		a$d		not escaped]===], 
	[===[a%$			a$		a$		escaped]===], 
	[===[a%(d			a(d		a(d		escaped]===], 
	[===[a%)d			a)d		a)d		escaped]===], 
	[===[a%%d			a%d		a%d		escaped]===], 
	[===[a%			a%		/malformed pattern %(ends with '%%'%)/	not escaped]===], 
	[===[a%.d			a.d		a.d		escaped]===], 
	[===[a%.d			abd		nil		escaped]===], 
	[===[a%[d			a[d		a[d		escaped]===], 
	[===[a%]d			a]d		a]d		escaped]===], 
	[===[a%*d			a*d		a*d		escaped]===], 
	[===[*ad			*ad		*ad		not escaped]===], 
	[===[a%+d			a+d		a+d		escaped]===], 
	[===[a%-d			a-d		a-d		escaped]===], 
	[===[a%?d			a?d		a?d		escaped]===], 
	[===[a%yd			ayd		ayd		escaped]===], 
	[===[a%w+f			a=[ *f		nil		word character]===], 
	[===[a%w+f			abcdef		abcdef		word character]===], 
	[===[a%W+f			a&%- f		a&%- f		not word character]===], 
	[===[a%W+f			abcdef		nil		not word character]===], 
	[===[a%d+f			abcdef		nil		digit]===], 
	[===[ab%d+cdef		ab42cdef	ab42cdef	digit]===], 
	[===[a%D+f			abcdef		abcdef		not digit]===], 
	[===[a%D+f			ab0cdef		nil		not digit]===], 
	[===[a%l+f			aBCDEf		nil		lowercase letter]===], 
	[===[a%l+f			abcdef		abcdef		lowercase letter]===], 
	[===[a%L+f			a&2D f		a&2D f		not lowercase letter]===], 
	[===[a%L+f			aBCdEf		nil		not lowercase letter]===], 
	[===[a%u+f			abcdef		nil		uppercase letter]===], 
	[===[a%u+f			aBCDEf		aBCDEf		uppercase letter]===], 
	[===[a%U+f			a&2d f		a&2d f		not uppercase letter]===], 
	[===[a%U+f			a&2D f		nil		not uppercase letter]===], 
	[===[a%a+f			aBcDef		aBcDef		all letter]===], 
	[===[a%a+f			a=[ *f		nil		all letter]===], 
	[===[a%A+f			a&%- f		a&%- f		not all letter]===], 
	[===[a%A+f			abcdef		nil		not all letter]===], 
	[===[a%g+f			aBcDef		aBcDef		printable]===], 
	[===[a%g+f			a=[ *f		nil		printable]===], 
	[===[a%G+f			a \nf		a \nf		not printable]===], 
	[===[a%G+f			abcdef		nil		not printable]===], 
	[===[a%p+f			abcdef		nil		ponctuation]===], 
	[===[a%p+f			a,;:!f		a,;:!f		ponctuation]===], 
	[===[a%P+f			abcdef		abcdef		not ponctuation]===], 
	[===[a%P+f			adc:ef		nil		not ponctuation]===], 
	[===[a%c+f			abcdef		nil		control character]===], 
	[===[a%c+f			a\04\03\02f	a\04\03\02f	control character]===], 
	[===[a%C+f			abcdef		abcdef		not control character]===], 
	[===[a%C+f			abc\01ef	nil		not control character]===], 
	[===[a%x+f			axyzef		nil		hexadecimal]===], 
	[===[a%x+f			ab3Def		ab3Def		hexadecimal]===], 
	[===[a%X+f			abcdef		nil		not hexadecimal]===], 
	[===[a%X+f			axy;Zf		axy;Zf		not hexadecimal]===], 
	[===[a%z+f			abcdef		nil		zero (deprecated)]===], 
	[===[a\0+f			abcdef		nil		zero]===], 
	[===[a%z+f			a\0f		a\0f		zero (deprecated)]===], 
	[===[a\0+f			a\0f		a\0f		zero]===], 
	[===[a%Z+f			abcdef		abcdef		not zero (deprecated)]===], 
	[===[a[^\0]+f		abcdef		abcdef		not zero]===], 
	[===[a%Z+f			abc\0ef		nil		not zero (deprecated)]===], 
	[===[a[^\0]+f		abc\0ef		nil		not zero]===], 
	[===[a%b()f			a(bcde)f	a(bcde)f	balanced]===], 
	[===[a%b()f			a(b(de)f	nil		balanced]===], 
	[===[a%b()f			a(b(d)e)f	a(b(d)e)f	balanced]===], 
	[===[a%b''f			a'bcde'f	a'bcde'f	balanced]===], 
	[===[a%b""f			a"bcde"f	a"bcde"f	balanced]===], 
	[===[%f[b]bc			abcdef		bc		frontier]===], 
	[===[%f[b]c			abcdef		nil		frontier]===], 
	[===[%f[^ab]c		abacdef		c		frontier]===], 
	[===[%f[^ab]d		abacdef		nil		frontier]===]
}	

require 'Test.More'

plan(162)


local function split (line)
    local pattern, target, result, desc = '', '', '', ''
    local idx = 1
    local c = line:sub(idx, idx)
    while (c ~= '' and c ~= "\t") do
        if (c == '"') then
            pattern = pattern .. "\\\""
        else
            pattern = pattern .. c
        end
        idx = idx + 1
        c = line:sub(idx, idx)
    end
    if pattern == "''" then
        pattern = ''
    end
    while (c ~= '' and c == "\t") do
        idx = idx + 1
        c = line:sub(idx, idx)
    end
    while (c ~= '' and c ~= "\t") do
        if (c == '"') then
            target = target .. "\\\""
        else
            target = target .. c
        end
        idx = idx + 1
        c = line:sub(idx, idx)
    end
    if target == "''" then
        target = ''
    end
    while (c ~= '' and c == "\t") do
        idx = idx + 1
        c = line:sub(idx, idx)
    end
    while (c ~= '' and c ~= "\t") do
        if c == "\\" then
            idx = idx + 1
            c = line:sub(idx, idx)
            if     c == 'f' then
                result = result .. "\f"
            elseif c == 'n' then
                result = result .. "\n"
            elseif c == 'r' then
                result = result .. "\r"
            elseif c == 't' then
                result = result .. "\t"
            elseif c == '0' then
                idx = idx + 1
                c = line:sub(idx, idx)
                if     c == '1' then
                    result = result .. "\01"
                elseif c == '2' then
                    result = result .. "\02"
                elseif c == '3' then
                    result = result .. "\03"
                elseif c == '4' then
                    result = result .. "\04"
                else
                    result = result .. "\0" .. c
                end
            elseif c == "\t" then
                result = result .. "\\"
            else
                result = result .. "\\" .. c
            end
        else
            result = result .. c
        end
        idx = idx + 1
        c = line:sub(idx, idx)
    end
    if result == "''" then
        result = ''
    end
    while (c ~= '' and c == "\t") do
        idx = idx + 1
        c = line:sub(idx, idx)
    end
    while (c ~= '' and c ~= "\t") do
        desc = desc .. c
        idx = idx + 1
        c = line:sub(idx, idx)
    end
    return pattern, target, result, desc
end

local test_number = 0

for _, line in ipairs(test_patterns) do
    if line:len() == 0 then
        break
    end
    local pattern, target, result, desc = split(line)
    test_number = test_number + 1

    local code = [[
            local t = {string.match("]] .. target .. [[", "]] .. pattern .. [[")}
            if #t== 0 then
                return 'nil'
            else
                return table.concat(t, "\t")
            end
    ]]
    local compiled, msg = load(code)
    if not compiled then
        error("can't compile : " .. code .. "\n" .. msg)
    end
    if result:sub(1, 1) == '/' then
        local pattern = result:sub(2, result:len() - 1)
        error_like(compiled, pattern, desc)
    else
        local out
        pcall(function () out = compiled() end)
        is(out, result, desc)
    end
end

-- Local Variables:
--   mode: lua
--   lua-indent-level: 4
--   fill-column: 100
-- End:
-- vim: ft=lua expandtab shiftwidth=4:
