using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Tree
{
	internal static class LexerUtils
	{
		public static bool CharIsHexDigit(char c)
		{
			return char.IsDigit(c) ||
				c == 'a' || c == 'b' || c == 'c' || c == 'd' || c == 'e' || c == 'f' ||
				c == 'A' || c == 'B' || c == 'C' || c == 'D' || c == 'E' || c == 'F';
		}

		public static string AdjustLuaLongString(string str)
		{
			if (str.StartsWith("\r\n"))
				str = str.Substring(2);
			else if (str.StartsWith("\n"))
				str = str.Substring(1);

			return str;
		}

		public static string UnescapeLuaString(string str)
		{
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
							if (CharIsHexDigit(c))
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
	}
}
