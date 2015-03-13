using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class ANTLR_LiteralExpression : Expression
	{
		DynValue m_Value;

		public DynValue Value
		{
			get { return m_Value; }
		}

		public ANTLR_LiteralExpression(IParseTree context, ScriptLoadingContext lcontext, DynValue rvalue)
			: base(context, lcontext)
		{
			m_Value = rvalue.AsReadOnly();
		}


		public ANTLR_LiteralExpression(LuaParser.NumberContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			if (m_Value == null) TryParse(context.FLOAT(), s => double.Parse(s, CultureInfo.InvariantCulture));
			if (m_Value == null) TryParse(context.HEX(), s => (double)ulong.Parse(RemoveHexHeader(s), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
			if (m_Value == null) TryParse(context.INT(), s => double.Parse(s, CultureInfo.InvariantCulture));
			if (m_Value == null) TryParse(context.HEX_FLOAT(), s => ParseHexFloat(s));

			if (m_Value == null)
				throw new SyntaxErrorException("unknown number format near '{0}'", context.GetText());
		}


		private string RemoveHexHeader(string s)
		{
			s = s.ToUpperInvariant();
			if (s.StartsWith("0X"))
				s = s.Substring(2);

			return s;
		}


		public ANTLR_LiteralExpression(LuaParser.StringContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			ITerminalNode charStr = context.CHARSTRING();
			ITerminalNode longStr = context.LONGSTRING();
			ITerminalNode normStr = context.NORMALSTRING();

			if (charStr != null)
				m_Value = DynValue.NewString(NormalizeNormStr(charStr.GetText(), true)).AsReadOnly();
			else if (longStr != null)
				m_Value = DynValue.NewString(NormalizeLongStr_ANTLR(longStr.GetText())).AsReadOnly();
			else if (normStr != null)
				m_Value = DynValue.NewString(NormalizeNormStr(normStr.GetText(), true)).AsReadOnly();
		}



		private string NormalizeNormStr(string str, bool cutPrefix)
		{
			if (cutPrefix) // ANTLR ONLY -- TO REMOVE
				str = str.Substring(1, str.Length - 2); // removes "/'

			if (!str.Contains('\\'))
				return str;

			StringBuilder sb = new StringBuilder();

			bool escape = false;
			bool hex = false;
			int unicode_state = 0;
			string hexprefix = "";
			string val = "";
			bool zmode = false;

			foreach (char c in str)
			{
			redo:
				if (escape)
				{
					if (val.Length == 0 && !hex && unicode_state == 0)
					{
						if (c == 'a') { sb.Append('\a'); escape = false; zmode = false; }
						else if (c == '\r') { }  // this makes \\r\n -> \\n
						else if (c == '\n') { sb.Append('\n'); escape = false; }  
						else if (c == 'b') { sb.Append('\b'); escape = false; }
						else if (c == 'f') { sb.Append('\f'); escape = false; }
						else if (c == 'n') { sb.Append('\n'); escape = false; }
						else if (c == 'r') { sb.Append('\r'); escape = false; }
						else if (c == 't') { sb.Append('\t'); escape = false; }
						else if (c == 'v') { sb.Append('\v'); escape = false; }
						else if (c == '\\') { sb.Append('\\'); escape = false; zmode = false; }
						else if (c == '"') { sb.Append('\"'); escape = false; zmode = false; }
						else if (c == '\'') { sb.Append('\''); escape = false; zmode = false; }
						else if (c == '[') { sb.Append('['); escape = false; zmode = false; }
						else if (c == ']') { sb.Append(']'); escape = false; zmode = false; }
						else if (c == 'x') { hex = true; }
						else if (c == 'u') { unicode_state = 1; }
						else if (c == 'z') { zmode = true; escape = false; }
						else if (char.IsDigit(c)) { val = val + c; }
						else throw new SyntaxErrorException("invalid escape sequence near '\\{0}'", c);
					}
					else
					{
						if (unicode_state == 1)
						{
							if (c != '{')
								throw new SyntaxErrorException("'{' expected near '\\u'");

							unicode_state = 2;
						}
						else if (unicode_state == 2)
						{
							if (c == '}')
							{
								int i = int.Parse(val, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
								sb.Append(char.ConvertFromUtf32(i));
								unicode_state = 0;
								val = string.Empty;
								escape = false;
							}
							else if (val.Length >= 8)
							{
								throw new SyntaxErrorException("'}' missing, or unicode code point too large after '\\u'");
							}
							else
							{
								val += c;
							}
						}
						else if (hex)
						{
							if (IsHexDigit(c))
							{
								val += c;
								if (val.Length == 2)
								{
									int i = int.Parse(val, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
									sb.Append(char.ConvertFromUtf32(i));
									zmode = false; 
									escape = false;
								}
							}
							else
							{
								throw new SyntaxErrorException("hexadecimal digit expected near '\\{0}{1}{2}'", hexprefix, val, c);
							}
						}
						else if (val.Length > 0)
						{
							if (char.IsDigit(c))
							{
								val = val + c;
							}

							if (val.Length == 3 || !char.IsDigit(c))
							{
								int i = int.Parse(val, CultureInfo.InvariantCulture);

								if (i > 255) 
									throw new SyntaxErrorException("decimal escape too large near '\\{0}'", val);

								sb.Append(char.ConvertFromUtf32(i));

								zmode = false;
								escape = false;

								if (!char.IsDigit(c))
									goto redo;
							}
						}
					}
				}
				else
				{
					if (c == '\\')
					{
						escape = true;
						hex = false;
						val = "";
					}
					else
					{
						if (!zmode || !char.IsWhiteSpace(c))
						{
							sb.Append(c);
							zmode = false;
						}
					}
				}
			}

			if (escape && !hex && val.Length > 0)
			{
				int i = int.Parse(val, CultureInfo.InvariantCulture);
				sb.Append(char.ConvertFromUtf32(i));
				escape = false;
			}

			if (escape)
			{
				throw new SyntaxErrorException("unfinished string near '\"{0}\"'", sb.ToString());
			}

			return sb.ToString();
		}

		private bool IsHexDigit(char c)
		{
			return (char.IsDigit(c)) || ("AaBbCcDdEeFf".Contains(c));
		}

		private string NormalizeLongStr(string str)
		{
			if (str.StartsWith("\r\n"))
				str = str.Substring(2);
			else if (str.StartsWith("\n"))
				str = str.Substring(1);

			return str;
		}

		private string NormalizeLongStr_ANTLR(string str)
		{
			int lenOfPrefix = 0;
			int squareBracketsFound = 0;
			str = str.Trim();

			for (int i = 0; i < str.Length; i++)
			{
				char c = str[i];
				if (c == '[')
					++squareBracketsFound;

				++lenOfPrefix;

				if (squareBracketsFound == 2)
					break;
			}

			str = str.Substring(lenOfPrefix, str.Length - lenOfPrefix * 2);

			if (str.StartsWith("\r\n"))
				str = str.Substring(2);
			else if (str.StartsWith("\n"))
				str = str.Substring(1);

			return str;
		}

		private void TryParse(string txt, Func<string, double> parser)
		{
			double val = parser(txt);
			m_Value = DynValue.NewNumber(val).AsReadOnly();
		}


		private void TryParse(ITerminalNode terminalNode, Func<string, double> parser)
		{
			if (terminalNode == null)
				return;

			string txt = terminalNode.GetText();
			TryParse(txt, parser);
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			bc.Emit_Literal(m_Value);
		}

		private double ParseHexFloat(string s)
		{
			throw new SyntaxErrorException("hex floats are not supported: '{0}'", s);
		}

		public override DynValue Eval(ScriptExecutionContext context)
		{
			return m_Value;
		}
	}
}
