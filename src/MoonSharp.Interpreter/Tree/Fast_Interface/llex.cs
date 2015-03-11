#if false
/*
** $Id: llex.c,v 2.20.1.1 2007/12/27 13:02:25 roberto Exp $
** Lexical Analyzer
** See Copyright Notice in lua.h
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace KopiLua
{
	using TValue = MoonSharp.Interpreter.DataType;
	using LuaNumberType = System.Double;

	public partial class Lua
	{
		public const int FIRSTRESERVED	= 257;

		/* maximum length of a reserved word */
		public const int TOKENLEN	= 9; // "function"


		/*
		* WARNING: if you change the order of this enumeration,
		* grep "ORDER RESERVED"
		*/
		public enum RESERVED {
		  /* terminal symbols denoted by reserved words */
		  TK_AND = FIRSTRESERVED, TK_BREAK,
		  TK_DO, TK_ELSE, TK_ELSEIF, TK_END, TK_FALSE, TK_FOR, TK_FUNCTION,
		  TK_IF, TK_IN, TK_LOCAL, TK_NIL, TK_NOT, TK_OR, TK_REPEAT,
		  TK_RETURN, TK_THEN, TK_TRUE, TK_UNTIL, TK_WHILE,
		  /* other terminal symbols */
		  TK_CONCAT, TK_DOTS, TK_EQ, TK_GE, TK_LE, TK_NE, TK_NUMBER,
		  TK_NAME, TK_STRING, TK_EOS
		};

		/* number of reserved words */
		public const int NUMRESERVED = (int)RESERVED.TK_WHILE - FIRSTRESERVED + 1;

		public class SemInfo {
			public SemInfo() { }
			public SemInfo(SemInfo copy)
			{
				this.r = copy.r;
				this.ts = copy.ts;
			}
			public LuaNumberType r;
			public string ts;
		} ;  /* semantics information */

		public class Token {
			public Token() { }
			public Token(Token copy)
			{
				this.token = copy.token;
				this.seminfo = new SemInfo(copy.seminfo);
			}
			public int token;
			public SemInfo seminfo = new SemInfo();
		};


		public class LexState {
			public int current;  /* current character (charint) */
			public int linenumber;  /* input line counter */
			public int lastline;  /* line of last token `consumed' */
			public Token t = new Token();  /* current token */
			public Token lookahead = new Token();  /* look ahead token */
			public FuncState fs;  /* `FuncState' is private to the parser */
			public LuaState L;
			public ZIO z;  /* input stream */
			public Mbuffer buff;  /* buffer for tokens */
			public string source;  /* current source name */
			public char decpoint;  /* locale decimal point */
		};


		public static void Next(LexState ls) { ls.current = zgetc(ls.z); }


		public static bool CurrIsNewline(LexState ls) { return (ls.current == '\n' || ls.current == '\r'); }


		/* ORDER RESERVED */
		public static readonly string[] LuaXTokens = {
			"and", "break", "do", "else", "elseif",
			"end", "false", "for", "function", "if",
			"in", "local", "nil", "not", "or", "repeat",
			"return", "then", "true", "until", "while",
			"..", "...", "==", ">=", "<=", "~=",
			"<number>", "<name>", "<string>", "<eof>"
		};


		public static void SaveAndNext(LexState ls) {Save(ls, ls.current); Next(ls);}

		private static void Save (LexState ls, int c) {
		  Mbuffer b = ls.buff;
		  if (b.n + 1 > b.buffsize) {
			uint newsize;
			if (b.buffsize >= MAXSIZET/2)
			  LuaXLexError(ls, "lexical element too long", 0);
			newsize = b.buffsize * 2;
			luaZ_resizebuffer(ls.L, b, (int)newsize);
		  }
		  b.buffer[b.n++] = (char)c;
		}

		
		public static void LuaXInit (LuaState L) {
		  int i;
		  for (i=0; i<NUMRESERVED; i++) {
			TString ts = luaS_new(L, LuaXTokens[i]);
			luaS_fix(ts);  /* reserved words are never collected */
			LuaAssert(LuaXTokens[i].Length+1 <= TOKENLEN);
			ts.tsv.reserved = CastByte(i+1);  /* reserved word */
		  }
		}


		public const int MAXSRC          = 80;


		public static CharPtr LuaXTokenToString (LexState ls, int token) {
		  if (token < FIRSTRESERVED) {
			LuaAssert(token == (byte)token);
			return (iscntrl(token)) ? LuaOPushFString(ls.L, "char(%d)", token) :
									  LuaOPushFString(ls.L, "%c", token);
		  }
		  else
			return LuaXTokens[(int)token-FIRSTRESERVED];
		}


		public static CharPtr TextToken (LexState ls, int token) {
		  switch (token) {
			case (int)RESERVED.TK_NAME:
			case (int)RESERVED.TK_STRING:
			case (int)RESERVED.TK_NUMBER:
			  Save(ls, '\0');
			  return luaZ_buffer(ls.buff);
			default:
			  return LuaXTokenToString(ls, token);
		  }
		}

		public static void LuaXLexError (LexState ls, CharPtr msg, int token) {
		  CharPtr buff = new char[MAXSRC];
		  LuaOChunkID(buff, GetStr(ls.source), MAXSRC);
		  msg = LuaOPushFString(ls.L, "%s:%d: %s", buff, ls.linenumber, msg);
		  if (token != 0)
			LuaOPushFString(ls.L, "%s near " + LUA_QS, msg, TextToken(ls, token));
		  LuaDThrow(ls.L, LUA_ERRSYNTAX);
		}

		public static void LuaXSyntaxError (LexState ls, CharPtr msg) {
		  LuaXLexError(ls, msg, ls.t.token);
		}

		[CLSCompliantAttribute(false)]
		public static TString LuaXNewString(LexState ls, CharPtr str, uint l)
		{
		  LuaState L = ls.L;
		  TString ts = luaS_newlstr(L, str, l);
		  TValue o = luaH_setstr(L, ls.fs.h, ts);  /* entry for `str' */
		  if (TTIsNil (o)) {
				SetBValue (o, 1);  /* make sure `str' will not be collected */
				LuaCCheckGC(L);
		  }
		  return ts;
		}


		private static void IncLineNumber (LexState ls) {
		  int old = ls.current;
		  LuaAssert(CurrIsNewline(ls));
		  Next(ls);  /* skip `\n' or `\r' */
		  if (CurrIsNewline(ls) && ls.current != old)
			Next(ls);  /* skip `\n\r' or `\r\n' */
		  if (++ls.linenumber >= MAXINT)
			LuaXSyntaxError(ls, "chunk has too many lines");
		}


		public static void LuaXSetInput (LuaState L, LexState ls, ZIO z, TString source) {
		  ls.decpoint = '.';
		  ls.L = L;
		  ls.lookahead.token = (int)RESERVED.TK_EOS;  /* no look-ahead token */
		  ls.z = z;
		  ls.fs = null;
		  ls.linenumber = 1;
		  ls.lastline = 1;
		  ls.source = source;
		  luaZ_resizebuffer(ls.L, ls.buff, LUAMINBUFFER);  /* initialize buffer */
		  Next(ls);  /* read first char */
		}



		/*
		** =======================================================
		** LEXICAL ANALYZER
		** =======================================================
		*/


		private static int CheckNext (LexState ls, CharPtr set) {
		  if (strchr(set, (char)ls.current) == null)
			return 0;
		  SaveAndNext(ls);
		  return 1;
		}


		private static void BufferReplace (LexState ls, char from, char to) {
		  uint n = luaZ_bufflen(ls.buff);
		  CharPtr p = luaZ_buffer(ls.buff);
		  while ((n--) != 0)
			  if (p[n] == from) p[n] = to;
		}


		private static void TryDecPoint (LexState ls, SemInfo seminfo) {
		  /* format error: try to update decimal point separator */
			// todo: add proper support for localeconv - mjf
			//lconv cv = localeconv();
			char old = ls.decpoint;
			ls.decpoint = '.'; // (cv ? cv.decimal_point[0] : '.');
			BufferReplace(ls, old, ls.decpoint);  /* try updated decimal separator */
			if (LuaOStr2d(luaZ_buffer(ls.buff), out seminfo.r) == 0)
			{
				/* format error with correct decimal point: no more options */
				BufferReplace(ls, ls.decpoint, '.');  /* undo change (for error message) */
				LuaXLexError(ls, "malformed number", (int)RESERVED.TK_NUMBER);
			}
		}


		/* LUA_NUMBER */
		private static void ReadNumeral (LexState ls, SemInfo seminfo) {
		  LuaAssert(isdigit(ls.current));
		  do {
			SaveAndNext(ls);
		  } while (isdigit(ls.current) || ls.current == '.');
		  if (CheckNext(ls, "Ee") != 0)  /* `E'? */
			CheckNext(ls, "+-");  /* optional exponent sign */
		  while (isalnum(ls.current) || ls.current == '_')
			SaveAndNext(ls);
		  Save(ls, '\0');
		  BufferReplace(ls, '.', ls.decpoint);  /* follow locale for decimal point */
		  if (LuaOStr2d(luaZ_buffer(ls.buff), out seminfo.r) == 0)  /* format error? */
			TryDecPoint(ls, seminfo); /* try to update decimal point separator */
		}


		private static int SkipSep (LexState ls) {
		  int count = 0;
		  int s = ls.current;
		  LuaAssert(s == '[' || s == ']');
		  SaveAndNext(ls);
		  while (ls.current == '=') {
			SaveAndNext(ls);
			count++;
		  }
		  return (ls.current == s) ? count : (-count) - 1;
		}


		private static void ReadLongString (LexState ls, SemInfo seminfo, int sep) {
		  //int cont = 0;
		  //(void)(cont);  /* avoid warnings when `cont' is not used */
		  SaveAndNext(ls);  /* skip 2nd `[' */
		  if (CurrIsNewline(ls))  /* string starts with a newline? */
			IncLineNumber(ls);  /* skip it */
		  for (;;) {
			switch (ls.current) {
			  case EOZ:
				LuaXLexError(ls, (seminfo != null) ? "unfinished long string" :
										   "unfinished long comment", (int)RESERVED.TK_EOS);
				break;  /* to avoid warnings */
		#if LUA_COMPAT_LSTR
			  case '[': {
				if (skip_sep(ls) == sep) {
				  save_and_next(ls);  /* skip 2nd `[' */
				  cont++;
		#if LUA_COMPAT_LSTR
				  if (sep == 0)
					luaX_lexerror(ls, "nesting of [[...]] is deprecated", '[');
		#endif
				}
				break;
			  }
		#endif
			  case ']':
				if (SkipSep(ls) == sep)
				{
				  SaveAndNext(ls);  /* skip 2nd `]' */
		//#if defined(LUA_COMPAT_LSTR) && LUA_COMPAT_LSTR == 2
		//          cont--;
		//          if (sep == 0 && cont >= 0) break;
		//#endif
				  goto endloop;
				}
			  break;
			  case '\n':
			  case '\r':
				Save(ls, '\n');
				IncLineNumber(ls);
				if (seminfo == null) luaZ_resetbuffer(ls.buff);  /* avoid wasting space */
				break;
			  default: {
				if (seminfo != null) SaveAndNext(ls);
				else Next(ls);
			  }
			  break;
			}
		  } endloop:
		  if (seminfo != null)
		  {
			  seminfo.ts = LuaXNewString(ls, luaZ_buffer(ls.buff) + (2 + sep), 
											(uint)(luaZ_bufflen(ls.buff) - 2*(2 + sep)));
		  }
		}


		static void ReadString (LexState ls, int del, SemInfo seminfo) {
		  SaveAndNext(ls);
		  while (ls.current != del) {
			switch (ls.current) {
			  case EOZ:
				LuaXLexError(ls, "unfinished string", (int)RESERVED.TK_EOS);
				continue;  /* to avoid warnings */
			  case '\n':
			  case '\r':
				LuaXLexError(ls, "unfinished string", (int)RESERVED.TK_STRING);
				continue;  /* to avoid warnings */
			  case '\\': {
				int c;
				Next(ls);  /* do not save the `\' */
				switch (ls.current) {
				  case 'a': c = '\a'; break;
				  case 'b': c = '\b'; break;
				  case 'f': c = '\f'; break;
				  case 'n': c = '\n'; break;
				  case 'r': c = '\r'; break;
				  case 't': c = '\t'; break;
				  case 'v': c = '\v'; break;
				  case '\n':  /* go through */
				  case '\r': Save(ls, '\n'); IncLineNumber(ls); continue;
				  case EOZ: continue;  /* will raise an error next loop */
				  default: {
					if (!isdigit(ls.current))
					  SaveAndNext(ls);  /* handles \\, \", \', and \? */
					else {  /* \xxx */
					  int i = 0;
					  c = 0;
					  do {
						c = 10*c + (ls.current-'0');
						Next(ls);
					  } while (++i<3 && isdigit(ls.current));
					  if (c > System.Byte.MaxValue)
						LuaXLexError(ls, "escape sequence too large", (int)RESERVED.TK_STRING);
					  Save(ls, c);
					}
					continue;
				  }
				}
				Save(ls, c);
				Next(ls);
				continue;
			  }
			  default:
				SaveAndNext(ls);
				break;
			}
		  }
		  SaveAndNext(ls);  /* skip delimiter */
		  seminfo.ts = LuaXNewString(ls, luaZ_buffer(ls.buff) + 1,
		                                  luaZ_bufflen(ls.buff) - 2);
		}


		private static int LLex (LexState ls, SemInfo seminfo) {
		  luaZ_resetbuffer(ls.buff);
		  for (;;) {
			switch (ls.current) {
			  case '\n':
			  case '\r': {
				IncLineNumber(ls);
				continue;
			  }
			  case '-': {
				Next(ls);
				if (ls.current != '-') return '-';
				/* else is a comment */
				Next(ls);
				if (ls.current == '[') {
				  int sep = SkipSep(ls);
				  luaZ_resetbuffer(ls.buff);  /* `skip_sep' may dirty the buffer */
				  if (sep >= 0) {
					ReadLongString(ls, null, sep);  /* long comment */
					luaZ_resetbuffer(ls.buff);
					continue;
				  }
				}
				/* else short comment */
				while (!CurrIsNewline(ls) && ls.current != EOZ)
				  Next(ls);
				continue;
			  }
			  case '[': {
				int sep = SkipSep(ls);
				if (sep >= 0) {
				  ReadLongString(ls, seminfo, sep);
				  return (int)RESERVED.TK_STRING;
				}
				else if (sep == -1) return '[';
				else LuaXLexError(ls, "invalid long string delimiter", (int)RESERVED.TK_STRING);
			  }
			  break;
			  case '=': {
				Next(ls);
				if (ls.current != '=') return '=';
				else { Next(ls); return (int)RESERVED.TK_EQ; }
			  }
			  case '<': {
				Next(ls);
				if (ls.current != '=') return '<';
				else { Next(ls); return (int)RESERVED.TK_LE; }
			  }
			  case '>': {
				Next(ls);
				if (ls.current != '=') return '>';
				else { Next(ls); return (int)RESERVED.TK_GE; }
			  }
			  case '~': {
				Next(ls);
				if (ls.current != '=') return '~';
				else { Next(ls); return (int)RESERVED.TK_NE; }
			  }
			  case '"':
			  case '\'': {
				ReadString(ls, ls.current, seminfo);
				return (int)RESERVED.TK_STRING;
			  }
			  case '.': {
				SaveAndNext(ls);
				if (CheckNext(ls, ".") != 0) {
				  if (CheckNext(ls, ".") != 0)
					  return (int)RESERVED.TK_DOTS;   /* ... */
				  else return (int)RESERVED.TK_CONCAT;   /* .. */
				}
				else if (!isdigit(ls.current)) return '.';
				else {
				  ReadNumeral(ls, seminfo);
				  return (int)RESERVED.TK_NUMBER;
				}
			  }
			  case EOZ: {
				  return (int)RESERVED.TK_EOS;
			  }
			  default: {
				if (isspace(ls.current)) {
				  LuaAssert(!CurrIsNewline(ls));
				  Next(ls);
				  continue;
				}
				else if (isdigit(ls.current)) {
				  ReadNumeral(ls, seminfo);
				  return (int)RESERVED.TK_NUMBER;
				}
				else if (isalpha(ls.current) || ls.current == '_') {
				  /* identifier or reserved word */
				  TString ts;
				  do {
					SaveAndNext(ls);
				  } while (isalnum(ls.current) || ls.current == '_');
				  ts = LuaXNewString(ls, luaZ_buffer(ls.buff),
										  luaZ_bufflen(ls.buff));
				  if (ts.tsv.reserved > 0)  /* reserved word? */
					return ts.tsv.reserved - 1 + FIRSTRESERVED;
				  else {
					seminfo.ts = ts;
					return (int)RESERVED.TK_NAME;
				  }
				}
				else {
				  int c = ls.current;
				  Next(ls);
				  return c;  /* single-char tokens (+ - / ...) */
				}
			  }
			}
		  }
		}


		public static void LuaXNext (LexState ls) {
		  ls.lastline = ls.linenumber;
		  if (ls.lookahead.token != (int)RESERVED.TK_EOS)
		  {  /* is there a look-ahead token? */
			ls.t = new Token(ls.lookahead);  /* use this one */
			ls.lookahead.token = (int)RESERVED.TK_EOS;  /* and discharge it */
		  }
		  else
			ls.t.token = LLex(ls, ls.t.seminfo);  /* read next token */
		}


		public static void LuaXLookAhead (LexState ls) {
			LuaAssert(ls.lookahead.token == (int)RESERVED.TK_EOS);
		  ls.lookahead.token = LLex(ls, ls.lookahead.seminfo);
		}

	}
}

#endif