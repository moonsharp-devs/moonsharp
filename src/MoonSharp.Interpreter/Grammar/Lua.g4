grammar Lua;

chunk
    : block (EOF)
    ;

dynamicexp
	: exp EOF
	;

singlefunc
	: anonfunctiondef EOF
	;


block
    : stat* retstat?
    ;

stat
    : ';'																			#stat_nulstatement
    | varlist '=' explist															#stat_assignment
    | varOrExp nameAndArgs+															#stat_functioncall
    | label																			#stat_label
    | BREAK																			#stat_break
    | GOTO NAME																		#stat_goto
    | DO block END																	#stat_doblock
    | WHILE exp DO block END														#stat_whiledoloop
    | REPEAT block UNTIL exp														#stat_repeatuntilloop
    | IF exp THEN block (ELSEIF exp THEN block)* (ELSE block)? END					#stat_ifblock
    | FOR NAME '=' exp ',' exp (',' exp)? DO block END								#stat_forloop
    | FOR namelist IN explist DO block END											#stat_foreachloop
    | FUNCTION funcname funcbody													#stat_funcdef
    | LOCAL FUNCTION NAME funcbody													#stat_localfuncdef
    | LOCAL namelist ('=' explist)?													#stat_localassignment
    ;


retstat
    : RETURN explist? ';'?
    ;

label
    : '::' NAME '::'
    ;

// this is an addition
funcnametableaccessor
	: ('.' NAME);

funcname
    : fnname=NAME funcnametableaccessor* (':' methodaccessor=NAME)?
    ;

varlist
    : var (',' var)*
    ;

namelist
    : NAME (',' NAME)*
    ; 

explist
    : exp (',' exp)*
    ;


exp
    : NIL												#exp_nil
	| FALSE												#exp_false
	| TRUE												#exp_true
	| number											#exp_number
	| string											#exp_string
	| vararg											#exp_varargs
	| FUNCTION funcbody									#exp_anonfunc
    | prefixexp											#exp_prefixexp
	| tableconstructor									#exp_tabctor
	| <assoc=right> exp '^' exp							#exp_power
	| operatorunary exp									#exp_unary
	| exp operatorbinary exp							#exp_binary
	;

var
    : (NAME | PAREN_OPEN exp PAREN_CLOSE varSuffix) varSuffix*
    ;

prefixexp
    : varOrExp nameAndArgs*
    ;

//
varOrExp
    : var | PAREN_OPEN exp PAREN_CLOSE
    ;


nameAndArgs
    : (':' NAME)? args
    ;

// Suffix to variable - array/table indexing
varSuffix
    : nameAndArgs* ('[' exp ']' | '.' NAME)
    ;


// Possible args to func call : list of expressions, table ctor, string literal
args
    : PAREN_OPEN explist? PAREN_CLOSE | tableconstructor | string
    ;


// Definition of func. Note: there is NO function name!
anonfunctiondef
    : 'function' funcbody
    ;

//lambdaexp
//	: '[' parlist ':' exp ']'
//	;

//lambdastat
//	: '[' parlist ':' 'do' block 'end' ']'
//	;

// A func body from the parlist to end.
funcbody
    : PAREN_OPEN parlist? PAREN_CLOSE block END
    ;

// The list of params in a function def
parlist
    : namelist (',' vararg)? | vararg
    ;


// A table ctor
tableconstructor
    : CURLY_OPEN fieldlist? CURLY_CLOSE
    ;


// The inside of a table ctor
fieldlist
    : field (fieldsep field)* fieldsep?
    ;


// field declaration in table ctor
field
    : '[' keyexp=exp ']' '=' keyedexp=exp | NAME '=' namedexp=exp | positionalexp=exp
    ;


// separators for fields in a table ctor
fieldsep
    : ',' | ';'
    ;


number
    : INT | HEX | FLOAT | HEX_FLOAT
    ;

string
    : NORMALSTRING | CHARSTRING | LONGSTRING
    ;

vararg
	: '...'
	;

// LEXER
AND : 'and';
BREAK : 'break';
DO : 'do';
ELSE : 'else';
ELSEIF : 'elseif';
END : 'end';
FALSE : 'false';
FOR : 'for';
FUNCTION : 'function';
GOTO : 'goto';
IF : 'if';
IN : 'in';
LOCAL : 'local';
NIL : 'nil';
NOT : 'not';
OR : 'or';
REPEAT : 'repeat';
RETURN : 'return';
THEN : 'then';
TRUE : 'true';
UNTIL : 'until';
WHILE : 'while';

CURLY_OPEN : '{';
CURLY_CLOSE : '}';
PAREN_OPEN : '(';
PAREN_CLOSE: ')';

operatorbinary 
	: OR | AND | '<' | '>' | '<=' | '>=' | '~=' | '==' | '..' | '+' | '-' | '*' | '/' | '%' ;

operatorunary
    : NOT | '#' | '-';


NAME
    : [a-zA-Z_][a-zA-Z_0-9]*
    ;

NORMALSTRING
    : '"' ( EscapeSequence | ~('\\'|'"') )* '"' 
    ;

CHARSTRING
    : '\'' ( EscapeSequence | ~('\''|'\\') )* '\''
    ;

LONGSTRING
    : '[' NESTED_STR ']'
    ;

fragment
NESTED_STR
    : '=' NESTED_STR '='
    | '[' .*? ']'
    ;

INT
    : Digit+
    ;

HEX
    : '0' [xX] HexDigit+
    ;

FLOAT
    : Digit+ '.' Digit* ExponentPart?
    | '.' Digit+ ExponentPart?
    | Digit+ ExponentPart
    ;

HEX_FLOAT
    : '0' [xX] HexDigit+ '.' HexDigit* HexExponentPart?
    | '0' [xX] '.' HexDigit+ HexExponentPart?
    | '0' [xX] HexDigit+ HexExponentPart
    ;

fragment
ExponentPart
    : [eE] [+-]? Digit+
    ;

fragment
HexExponentPart
    : [pP] [+-]? Digit+
    ;

fragment
EscapeSequence
    : '\\' '\r'? '\n'
    | '\\' ["']
	| '\\' .
    ;
    
fragment
Digit
    : [0-9]
    ;

fragment
HexDigit
    : [0-9a-fA-F]
    ;

COMMENT
    : '--[' NESTED_STR ']' -> channel(HIDDEN)
    ; 
    
LINE_COMMENT
    : '--'
    (                                               // --
    | '[' '='*                                      // --[==
    | '[' '='* ~('='|'['|'\r'|'\n') ~('\r'|'\n')*   // --[==AA 
    | ~('['|'\r'|'\n') ~('\r'|'\n')*                // --AAA
    ) ('\r\n'|'\r'|'\n'|EOF)
    -> channel(HIDDEN)
    ;
    
WS  
    : [ \t\u000C\r\n]+ -> skip
    ;

SHEBANG
    : '#' '!' ~('\n'|'\r')* -> channel(HIDDEN)
    ;




