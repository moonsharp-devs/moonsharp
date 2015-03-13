using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Tree
{
	class Lexer
	{
		Token m_Current = null;
		string m_Code;
		int m_Cursor = 0;
		int m_Line = 0;
		int m_Col = 0;
		bool m_AutoSkipComments = false;

		public Lexer(string scriptContent, bool autoSkipComments)
		{
			m_Code = scriptContent;

			// remove unicode BOM if any
			if (m_Code.Length > 0 && m_Code[0] == 0xFEFF)
				m_Code = m_Code.Substring(1);

			m_AutoSkipComments = autoSkipComments;

			Next();
		}

		public Token Current
		{
			get
			{
				return m_Current;
			}
		}

		private Token FetchNewToken()
		{
			while (true)
			{
				Token T = ReadToken();
	
				//System.Diagnostics.Debug.WriteLine("LEXER : " + T.ToString());

				if ((T.Type != TokenType.Comment && T.Type != TokenType.HashBang) || (!m_AutoSkipComments))
					return T;
			}
		}

		public void Next()
		{
			m_Current = FetchNewToken();
		}

		public Token PeekNext()
		{
			int snapshot = m_Cursor;
			Token current = m_Current;
			int line = m_Line;
			int col = m_Col;

			Next();
			Token t = Current;

			m_Cursor = snapshot;
			m_Current = current;
			m_Line = line;
			m_Col = col;

			return t;
		}


		private void CursorNext()
		{
			if (CursorNotEof())
			{
				if (CursorChar() == '\n')
				{
					m_Col = 0;
					m_Line += 1;
				}
				else
				{
					m_Col += 1;
				}

				m_Cursor += 1;
			}
		}

		private char CursorChar()
		{
			if (m_Cursor < m_Code.Length)
				return m_Code[m_Cursor];
			else
				return '\0'; //  sentinel
		}

		private char CursorCharNext()
		{
			m_Cursor += 1;
			return CursorChar();	
		}

		private bool CursorMatches(string pattern)
		{
			for (int i = 0; i < pattern.Length; i++)
			{
				int j = m_Cursor + i;

				if (j >= m_Code.Length)
					return false;
				if (m_Code[j] != pattern[i])
					return false;
			}
			return true;
		}

		private bool CursorNotEof()
		{
			return m_Cursor < m_Code.Length;
		}

		private bool IsWhiteSpace(char c)
		{
			return char.IsWhiteSpace(c);
		}

		private void SkipWhiteSpace()
		{
			for (; CursorNotEof() && IsWhiteSpace(CursorChar()); CursorNext())
			{
			}
		}


		private Token ReadToken()
		{
			SkipWhiteSpace();

			int fromLine = m_Line;
			int fromCol = m_Col;

			if (!CursorNotEof())
				return new Token(TokenType.Eof) { Text = "<eof>" };

			char c = CursorChar();

			switch (c)
			{
				case ';':
					CursorCharNext();
					return CreateToken(TokenType.SemiColon, fromLine, fromCol, ";");
				case '=':
					return PotentiallyDoubleCharOperator('=', TokenType.Op_Assignment, TokenType.Op_Equal, fromLine, fromCol);
				case '<':
					return PotentiallyDoubleCharOperator('=', TokenType.Op_LessThan, TokenType.Op_LessThanEqual, fromLine, fromCol);
				case '>':
					return PotentiallyDoubleCharOperator('=', TokenType.Op_GreaterThan, TokenType.Op_GreaterThanEqual, fromLine, fromCol);
				case '~':
				case '!':
					if (CursorCharNext() != '=')
						throw new SyntaxErrorException("Expected '=', {0} was found", CursorChar());
					CursorCharNext();
					return CreateToken(TokenType.Op_NotEqual, fromLine, fromCol, "~=");
				case '.':
					if (CursorCharNext() == '.')
						return PotentiallyDoubleCharOperator('.', TokenType.Op_Concat, TokenType.VarArgs, fromLine, fromCol);
					else
						return CreateToken(TokenType.Dot, fromLine, fromCol, ".");
				case '+':
					return CreateSingleCharToken(TokenType.Op_Add, fromLine, fromCol);
				case '-':
					{
						char next = CursorCharNext();
						if (next == '-')
						{
							return ReadComment(fromLine, fromCol);
						}
						else
						{
							return CreateToken(TokenType.Op_MinusOrSub, fromLine, fromCol, "-");
						}
					}
				case '*':
					return CreateSingleCharToken(TokenType.Op_Mul, fromLine, fromCol);
				case '/':
					return CreateSingleCharToken(TokenType.Op_Div, fromLine, fromCol);
				case '%':
					return CreateSingleCharToken(TokenType.Op_Mod, fromLine, fromCol);
				case '^':
					return CreateSingleCharToken(TokenType.Op_Pwr, fromLine, fromCol);
				case '#':
					if (m_Cursor == 0 && m_Code.Length > 1 && m_Code[1] == '!')
						return ReadHashBang(fromLine, fromCol);

					return CreateSingleCharToken(TokenType.Op_Len, fromLine, fromCol);
				case '[':
					{
						char next = CursorCharNext();
						if (next == '=' || next == '[')
						{
							string str = ReadLongString(null);
							return CreateToken(TokenType.String_Long, fromLine, fromCol, str);
						}
						return CreateToken(TokenType.Brk_Open_Square, fromLine, fromCol, "[");
					}
				case ']':
					return CreateSingleCharToken(TokenType.Brk_Close_Square, fromLine, fromCol);
				case '(':
					return CreateSingleCharToken(TokenType.Brk_Open_Round, fromLine, fromCol);
				case ')':
					return CreateSingleCharToken(TokenType.Brk_Close_Round, fromLine, fromCol);
				case '{':
					return CreateSingleCharToken(TokenType.Brk_Open_Curly, fromLine, fromCol);
				case '}':
					return CreateSingleCharToken(TokenType.Brk_Close_Curly, fromLine, fromCol);
				case ',':
					return CreateSingleCharToken(TokenType.Comma, fromLine, fromCol);
				case ':':
					return PotentiallyDoubleCharOperator(':', TokenType.Colon, TokenType.DoubleColon, fromLine, fromCol);
				case '"':
				case '\'':
					return ReadSimpleStringToken(fromLine, fromCol);
				default:
					{
						if (char.IsLetter(c) || c == '_')
						{
							string name = ReadNameToken();
							return CreateNameToken(name, fromLine, fromCol);
						}
						else if (char.IsDigit(c))
						{
							return ReadNumberToken(fromLine, fromCol);
						}
					}
					throw new SyntaxErrorException("Fallback to default ?!", CursorChar());
			}



		}

		private string ReadLongString(string startpattern)
		{
			// here we are at the first '=' or second '['
			StringBuilder text = new StringBuilder(1024);
			string end_pattern = "]";

			if (startpattern == null)
			{
				for (char c = CursorChar(); ; c = CursorCharNext())
				{
					if (c == '\0' || !CursorNotEof())
					{
						throw new SyntaxErrorException("Unterminated long string");
					}
					else if (c == '=')
					{
						end_pattern += "=";
					}
					else if (c == '[')
					{
						end_pattern += "]";
						break;
					}
					else
					{
						throw new SyntaxErrorException("Unexpected token in long string prefix: {0}", c);
					}
				}
			}
			else
			{
				end_pattern = startpattern.Replace('[', ']');
			}


			for (char c = CursorCharNext(); ; c = CursorCharNext())
			{
				if (c == '\0' || !CursorNotEof())
				{
					throw new SyntaxErrorException("Unterminated long string or comment");
				}
				else if (c == ']' && CursorMatches(end_pattern))
				{
					for (int i = 0; i < end_pattern.Length; i++)
						CursorCharNext();

					return LexerUtils.AdjustLuaLongString(text.ToString());
				}
				else
				{
					text.Append(c);
				}
			}
		}

		private Token ReadNumberToken(int fromLine, int fromCol)
		{
			StringBuilder text = new StringBuilder(32);

			//INT : Digit+
			//HEX : '0' [xX] HexDigit+
			//FLOAT : Digit+ '.' Digit* ExponentPart?
			//		| '.' Digit+ ExponentPart?
			//		| Digit+ ExponentPart
			//HEX_FLOAT : '0' [xX] HexDigit+ '.' HexDigit* HexExponentPart?
			//			| '0' [xX] '.' HexDigit+ HexExponentPart?
			//			| '0' [xX] HexDigit+ HexExponentPart
			//
			// ExponentPart : [eE] [+-]? Digit+
			// HexExponentPart : [pP] [+-]? Digit+

			bool isHex = false;
			bool dotAdded = false;
			bool exponentPart = false;
			bool exponentSignAllowed = false;

			text.Append(CursorChar());

			char secondChar = CursorCharNext();

			if (secondChar == 'x' || secondChar == 'X')
			{
				isHex = true;
				text.Append(CursorChar());
				CursorCharNext();
			}

			for (char c = CursorChar(); CursorNotEof(); c = CursorCharNext())
			{
				if (exponentSignAllowed && (c == '+' || c == '-'))
				{
					exponentSignAllowed = false;
					text.Append(c);
				}
				else if (char.IsDigit(c))
				{
					text.Append(c);
				}
				else if (c == '.' && !dotAdded)
				{
					dotAdded = true;
					text.Append(c);
				}
				else if (LexerUtils.CharIsHexDigit(c) && isHex && !exponentPart)
				{
					text.Append(c);
				}
				else if (c == 'e' || c == 'E' || (isHex && (c == 'p' || c == 'P')))
				{
					text.Append(c);
					exponentPart = true;
					exponentSignAllowed = true;
					dotAdded = true;
				}
				else
				{
					break;
				}
			}

			TokenType numberType = TokenType.Number;

			if (isHex && (dotAdded || exponentPart))
				numberType = TokenType.Number_HexFloat;
			else if (isHex)
				numberType = TokenType.Number_Hex;

			return CreateToken(numberType, fromLine, fromCol, text.ToString());
		}

		private Token CreateSingleCharToken(TokenType tokenType, int fromLine, int fromCol)
		{
			char c = CursorChar();
			CursorCharNext();
			return CreateToken(tokenType, fromLine, fromCol, c.ToString());
		}

		private Token ReadHashBang(int fromLine, int fromCol)
		{
			StringBuilder text = new StringBuilder(32);

			for (char c = CursorChar(); CursorNotEof(); c = CursorCharNext())
			{
				if (c == '\n')
				{
					CursorCharNext();
					return CreateToken(TokenType.HashBang, fromLine, fromCol, text.ToString());
				}
				else if (c != '\r')
				{
					text.Append(c);
				}
			}

			return CreateToken(TokenType.HashBang, fromLine, fromCol, text.ToString());
		}


		private Token ReadComment(int fromLine, int fromCol)
		{
			StringBuilder text = new StringBuilder(32);

			bool extraneousFound = false;

			for (char c = CursorCharNext(); CursorNotEof(); c = CursorCharNext())
			{
				if (c == '[' && !extraneousFound && text.Length > 0)
				{
					text.Append('[');
					CursorCharNext();
					string comment = ReadLongString(text.ToString());
					return CreateToken(TokenType.Comment, fromLine, fromCol, comment);
				}
				else if (c == '\n')
				{
					extraneousFound = true;
					CursorCharNext();
					return CreateToken(TokenType.Comment, fromLine, fromCol, text.ToString());
				}
				else if (c != '\r')
				{
					if (c != '[' && c != '=')
						extraneousFound = true;

					text.Append(c);
				}
			}

			return CreateToken(TokenType.Comment, fromLine, fromCol, text.ToString());
		}

		private Token ReadSimpleStringToken(int fromLine, int fromCol)
		{
			StringBuilder text = new StringBuilder(32);
			char separator = CursorChar();

			for (char c = CursorCharNext(); CursorNotEof(); c = CursorCharNext())
			{
				if (c == '\\')
				{
					text.Append(c);
					text.Append(CursorCharNext());
				}
				else if (c == separator)
				{
					CursorCharNext();
					return CreateToken(TokenType.String, fromLine, fromCol, LexerUtils.UnescapeLuaString(text.ToString()));
				}
				else
				{
					text.Append(c);
				}
			}

			throw new SyntaxErrorException("Unterminated string");
		}

		private Token PotentiallyDoubleCharOperator(char expectedSecondChar, TokenType singleCharToken, TokenType doubleCharToken, int fromLine, int fromCol)
		{
			string op = CursorChar().ToString();
			
			CursorCharNext();

			if (CursorChar() == expectedSecondChar)
			{
				CursorCharNext();
				return CreateToken(doubleCharToken, fromLine, fromCol, op + expectedSecondChar);
			}
			else
				return CreateToken(singleCharToken, fromLine, fromCol, op);
		}



		private Token CreateNameToken(string name, int fromLine, int fromCol)
		{
			TokenType? reservedType = Token.GetReservedTokenType(name);

			if (reservedType.HasValue)
			{
				return CreateToken(reservedType.Value, fromLine, fromCol, name);
			}
			else
			{
				return CreateToken(TokenType.Name, fromLine, fromCol, name);
			}
		}


		private Token CreateToken(TokenType tokenType, int fromLine, int fromCol, string text = null)
		{
			return new Token(tokenType, fromLine, fromCol, m_Line, m_Col)
			{
				Text = text
			};
		}

		private string ReadNameToken()
		{
			StringBuilder name = new StringBuilder(32);

			for (char c = CursorChar(); CursorNotEof(); c = CursorCharNext())
			{
				if (char.IsLetterOrDigit(c) || c == '_')
					name.Append(c);
				else
					break;
			}

			return name.ToString();
		}




	}
}
