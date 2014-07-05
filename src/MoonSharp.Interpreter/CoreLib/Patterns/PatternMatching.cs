// This portion of code is taken from UniLua: https://github.com/xebecnan/UniLua
//
// Copyright (C) 2013 Sheng Lunan
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files 
// (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do 
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib.Patterns
{
	static class PatternMatching
	{
		private const int CAP_UNFINISHED = -1;
		private const int CAP_POSITION = -2;
		public const int LUA_MAXCAPTURES = 32;
		private const char L_ESC = '%';
		private const string FLAGS = "-+ #0";
		private static readonly char[] SPECIALS;

		static PatternMatching()
		{
			SPECIALS = "^$*+?.([%-".ToCharArray();
		}

		private static int PosRelative(int pos, int len)
		{
			if (pos == int.MinValue) return 1;
			else if (pos >= 0) return pos;
			else if (0 - pos > len) return 0;
			else return len - (-pos) + 1;
		}

		private static int ClassEnd(MatchState ms, int p)
		{
			switch (ms.Pattern[p++])
			{
				case L_ESC:
					{
						if (p == ms.PatternEnd)
							throw new ScriptRuntimeException("malformed pattern (ends with '%')");
						return p + 1;
					}
				case '[':
					{
						if (ms.Pattern[p] == '^') p++;
						do
						{
							if (p == ms.PatternEnd)
								throw new ScriptRuntimeException("malformed pattern (missing ']')");
							if (ms.Pattern[p++] == L_ESC && p < ms.PatternEnd)
								p++; // skip escapes (e.g. `%]')
						} while (ms.Pattern[p] != ']');
						return p + 1;
					}
				default: return p;
			}
		}

		private static bool IsXDigit(char c)
		{
			switch (c)
			{
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				case 'a':
				case 'b':
				case 'c':
				case 'd':
				case 'e':
				case 'f':
				case 'A':
				case 'B':
				case 'C':
				case 'D':
				case 'E':
				case 'F':
					return true;
				default:
					return false;
			}
		}

		private static bool MatchClass(char c, char cl)
		{
			bool res;
			switch (cl)
			{
				case 'a': res = Char.IsLetter(c); break;
				case 'c': res = Char.IsControl(c); break;
				case 'd': res = Char.IsDigit(c); break;
				case 'g': throw new System.NotImplementedException();
				case 'l': res = Char.IsLower(c); break;
				case 'p': res = Char.IsPunctuation(c); break;
				case 's': res = Char.IsWhiteSpace(c); break;
				case 'u': res = Char.IsUpper(c); break;
				case 'w': res = Char.IsLetterOrDigit(c); break;
				case 'x': res = IsXDigit(c); break;
				case 'z': res = (c == '\0'); break;  /* deprecated option */
				default: return (cl == c);
			}
			return res;
		}

		private static bool MatchBreaketClass(MatchState ms, char c, int p, int ec)
		{
			bool sig = true;
			if (ms.Pattern[p + 1] == '^')
			{
				sig = false;
				p++; // skip the `^'
			}
			while (++p < ec)
			{
				if (ms.Pattern[p] == L_ESC)
				{
					p++;
					if (MatchClass(c, ms.Pattern[p]))
						return sig;
				}
				else if (ms.Pattern[p + 1] == '-' && (p + 2 < ec))
				{
					p += 2;
					if (ms.Pattern[p - 2] <= c && c <= ms.Pattern[p])
						return sig;
				}
				else if (ms.Pattern[p] == c) return sig;
			}
			return !sig;
		}

		private static bool SingleMatch(MatchState ms, char c, int p, int ep)
		{
			switch (ms.Pattern[p])
			{
				case '.': return true; // matches any char
				case L_ESC: return MatchClass(c, ms.Pattern[p + 1]);
				case '[': return MatchBreaketClass(ms, c, p, ep - 1);
				default: return ms.Pattern[p] == c;
			}
		}

		private static int MatchBalance(MatchState ms, int s, int p)
		{
			if (p >= ms.PatternEnd - 1)
				throw new ScriptRuntimeException("malformed pattern (missing arguments to '%b')");
			if (ms.Src[s] != ms.Pattern[p]) return -1;
			else
			{
				char b = ms.Pattern[p];
				char e = ms.Pattern[p + 1];
				int count = 1;
				while (++s < ms.SrcEnd)
				{
					if (ms.Src[s] == e)
					{
						if (--count == 0) return s + 1;
					}
					else if (ms.Src[s] == b) count++;
				}
			}
			return -1; //string ends out of balance
		}

		private static int MaxExpand(MatchState ms, int s, int p, int ep)
		{
			int i = 0; // counts maximum expand for item
			while ((s + i) < ms.SrcEnd && SingleMatch(ms, ms.Src[s + i], p, ep))
				i++;
			// keeps trying to match with the maximum repetitions
			while (i >= 0)
			{
				int res = Match(ms, (s + i), (ep + 1));
				if (res >= 0) return res;
				i--; // else didn't match; reduce 1 repetition to try again
			}
			return -1;
		}

		private static int MinExpand(MatchState ms, int s, int p, int ep)
		{
			for (; ; )
			{
				int res = Match(ms, s, ep + 1);
				if (res >= 0)
					return res;
				else if (s < ms.SrcEnd && SingleMatch(ms, ms.Src[s], p, ep))
					s++; // try with one more repetition
				else return -1;
			}
		}

		private static int CaptureToClose(MatchState ms)
		{
			int level = ms.Level;
			for (level--; level >= 0; level--)
			{
				if (ms.Capture[level].Len == CAP_UNFINISHED)
					return level;
			}

			throw new ScriptRuntimeException("invalid pattern capture");
		}

		private static int StartCapture(MatchState ms, int s, int p, int what)
		{
			int level = ms.Level;
			if (level >= LUA_MAXCAPTURES)
				throw new ScriptRuntimeException("too many captures");

			ms.Capture[level].Init = s;
			ms.Capture[level].Len = what;
			ms.Level = level + 1;
			int res = Match(ms, s, p);
			if (res == -1) // match failed?
				ms.Level--;
			return res;
		}

		private static int EndCapture(MatchState ms, int s, int p)
		{
			int l = CaptureToClose(ms);
			ms.Capture[l].Len = s - ms.Capture[l].Init; // close capture
			int res = Match(ms, s, p);
			if (res == -1) // match failed?
				ms.Capture[l].Len = CAP_UNFINISHED; // undo capture
			return res;
		}

		private static int CheckCapture(MatchState ms, char l)
		{
			int i = (int)(l - '1');
			if (i < 0 || i >= ms.Level || ms.Capture[i].Len == CAP_UNFINISHED)
				throw new ScriptRuntimeException("invalid capture index {0}", i + 1);

			return i;
		}

		private static int MatchCapture(MatchState ms, int s, char l)
		{
			int i = CheckCapture(ms, l);
			int len = ms.Capture[i].Len;
			if (ms.SrcEnd - s >= len &&
				string.Compare(ms.Src, ms.Capture[i].Init, ms.Src, s, len) == 0)
				return s + len;
			else
				return -1;
		}

		private static int Match(MatchState ms, int s, int p)
		{
		init: // using goto's to optimize tail recursion
			if (p == ms.PatternEnd)
				return s;
			switch (ms.Pattern[p])
			{
				case '(': // start capture
					{
						if (ms.Pattern[p + 1] == ')') // position capture?
							return StartCapture(ms, s, p + 2, CAP_POSITION);
						else
							return StartCapture(ms, s, p + 1, CAP_UNFINISHED);
					}
				case ')': // end capture
					{
						return EndCapture(ms, s, p + 1);
					}
				case '$':
					{
						if (p + 1 == ms.PatternEnd) // is the `$' the last char in pattern?
							return (s == ms.SrcEnd) ? s : -1; // check end of string
						else goto dflt;
					}
				case L_ESC: // escaped sequences not in the format class[*+?-]?
					{
						switch (ms.Pattern[p + 1])
						{
							case 'b': // balanced string?
								{
									s = MatchBalance(ms, s, p + 2);
									if (s == -1) return -1;
									p += 4; goto init; // else return match(ms, s, p+4);
								}
							case 'f': // frontier?
								{
									p += 2;
									if (ms.Pattern[p] != '[')
										throw new ScriptRuntimeException("missing '[' after '%f' in pattern");
									int ep = ClassEnd(ms, p); //points to what is next
									char previous = (s == ms.SrcInit) ? '\0' : ms.Src[s - 1];
									if (MatchBreaketClass(ms, previous, p, ep - 1) ||
										!MatchBreaketClass(ms, ms.Src[s], p, ep - 1)) return -1;
									p = ep; goto init; // else return match( ms, s, ep );
								}
							case '0':
							case '1':
							case '2':
							case '3':
							case '4':
							case '5':
							case '6':
							case '7':
							case '8':
							case '9': // capture results (%0-%9)?
								{
									s = MatchCapture(ms, s, ms.Pattern[p + 1]);
									if (s == -1) return -1;
									p += 2; goto init; // else return match(ms, s, p+2);
								}
							default: goto dflt;
						}
					}
				default:
				dflt: // pattern class plus optional suffix
					{
						int ep = ClassEnd(ms, p);
						bool m = s < ms.SrcEnd && SingleMatch(ms, ms.Src[s], p, ep);
						if (ep < ms.PatternEnd)
						{
							switch (ms.Pattern[ep]) //fix gmatch bug patten is [^a]
							{
								case '?': // optional
									{
										if (m)
										{
											int res = Match(ms, s + 1, ep + 1);
											if (res != -1)
												return res;
										}
										p = ep + 1; goto init; // else return match(ms, s, ep+1);
									}
								case '*': // 0 or more repetitions
									{
										return MaxExpand(ms, s, p, ep);
									}
								case '+': // 1 or more repetitions
									{
										return (m ? MaxExpand(ms, s + 1, p, ep) : -1);
									}
								case '-': // 0 or more repetitions (minimum)
									{
										return MinExpand(ms, s, p, ep);
									}
							}
						}
						if (!m) return -1;
						s++; p = ep; goto init; // else return match(ms, s+1, ep);
					}
			}
		}





		private static DynValue PushOneCapture(MatchState ms, int i, int start, int end)
		{
			if (i >= ms.Level)
			{
				if (i == 0) // ms.Level == 0, too
					return DynValue.NewString(ms.Src.Substring(start, end - start));
				else
					throw new ScriptRuntimeException("invalid capture index");
			}
			else
			{
				int l = ms.Capture[i].Len;
				if (l == CAP_UNFINISHED)
					throw new ScriptRuntimeException("unfinished capture");
				if (l == CAP_POSITION)
					return DynValue.NewNumber(ms.Capture[i].Init - ms.SrcInit + 1);
				else
					return DynValue.NewString(ms.Src.Substring(ms.Capture[i].Init, l));
			}
		}

		private static DynValue PushCaptures(MatchState ms, int spos, int epos)
		{
			int nLevels = (ms.Level == 0 && spos >= 0) ? 1 : ms.Level;

			DynValue[] captures = new DynValue[nLevels];

			for (int i = 0; i < nLevels; ++i)
				captures[i] = PushOneCapture(ms, i, spos, epos);

			return DynValue.NewTuple(captures);
		}

		private static bool NoSpecials(string pattern)
		{
			return pattern.IndexOfAny(SPECIALS) == -1;
		}

		private static DynValue StrFindAux(string s, string p, int init, bool plain, bool find)
		{
			init = PosRelative(init, s.Length);

			if (init < 1)
			{
				init = 1;
			}
			else if (init > s.Length + 1) // start after string's end?
			{
				return DynValue.Nil;
			}
			
			// explicit request or no special characters?
			if (find && (plain || NoSpecials(p)))
			{
				// do a plain search
				int pos = s.IndexOf(p, init - 1);
				if (pos >= 0)
				{
					return DynValue.NewTuple(
						DynValue.NewNumber(pos + 1),
						DynValue.NewNumber(pos + p.Length));
				}
			}
			else
			{
				int s1 = init - 1;
				int ppos = 0;
				bool anchor = p[ppos] == '^';
				if (anchor)
					ppos++; // skip anchor character

				MatchState ms = new MatchState();
				ms.Src = s;
				ms.SrcInit = s1;
				ms.SrcEnd = s.Length;
				ms.Pattern = p;
				ms.PatternEnd = p.Length;

				do
				{
					ms.Level = 0;
					int res = Match(ms, s1, ppos);
					if (res != -1)
					{
						if (find)
						{
							return DynValue.NewTupleNested(
								DynValue.NewNumber(s1 + 1),
								DynValue.NewNumber(res),
								PushCaptures(ms, -1, 0));
						}
						else return PushCaptures(ms, s1, res);
					}
				} while (s1++ < ms.SrcEnd && !anchor);
			}

			return DynValue.Nil;
		}

		public static DynValue Str_Find(string s, string p, int init, bool plain)
		{
			return StrFindAux(s, p, init, plain, true);
		}


		private static DynValue GmatchAux(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v_idx = executionContext.Closure["idx"];
			DynValue v_src = executionContext.Closure["src"];
			DynValue v_pattern = executionContext.Closure["pattern"];

			MatchState ms = new MatchState();
			string src = v_src.String;
			string pattern = v_pattern.String;
			ms.Src = src;
			ms.SrcInit = 0;
			ms.SrcEnd = src.Length;
			ms.Pattern = pattern;
			ms.PatternEnd = pattern.Length;

			for (int s = (int)v_idx.Number; s <= ms.SrcEnd; s++)
			{
				ms.Level = 0;
				int e = Match(ms, s, 0);
				if (e != -1)
				{
					int newStart = (e == 0) ? e + 1 : e;
					executionContext.Closure["idx"] = DynValue.NewNumber(newStart);
					return PushCaptures(ms, s, e);
				}
			}

			return DynValue.Nil;
		}

		public static DynValue GMatch(string src, string pattern)
		{
			DynValue aux = DynValue.NewCallback(GmatchAux);

			aux.Callback.Closure = new Table();
			aux.Callback.Closure["idx"] = DynValue.NewNumber(0);
			aux.Callback.Closure["src"] = DynValue.NewString(src);
			aux.Callback.Closure["pattern"] = DynValue.NewString(pattern);

			return aux;
		}



	}

}
