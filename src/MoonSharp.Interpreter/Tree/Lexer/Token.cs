using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Tree
{
	class Token
	{
		public readonly int FromCol, ToCol, FromLine, ToLine;
		public readonly TokenType Type;

		public string Text { get; set; }

		public Token(TokenType type, int fromLine, int fromCol, int toLine, int toCol)
		{
			Type = type;

			FromLine = fromLine;
			FromCol = fromCol;
			ToCol = toCol;
			ToLine = toLine;
		}

		public Token(TokenType type, int line, int col)
			: this(type, line, col, line, col)
		{ }

		public Token(TokenType type)
			: this(type, -1, -1, -1, -1)
		{ }

		public override string ToString()
		{
			string tokenTypeString = (Type.ToString() + "                                                      ").Substring(0, 16);
			return string.Format("{0}  -  {1}", tokenTypeString, this.Text ?? "");
		}



		public static TokenType? GetReservedTokenType(string reservedWord)
		{
			switch (reservedWord)
			{
				case "and":
					return TokenType.And;
				case "break":
					return TokenType.Break;
				case "do":
					return TokenType.Do;
				case "else":
					return TokenType.Else;
				case "elseif":
					return TokenType.ElseIf;
				case "end":
					return TokenType.End;
				case "false":
					return TokenType.False;
				case "for":
					return TokenType.For;
				case "function":
					return TokenType.Function;
				case "goto":
					return TokenType.Goto;
				case "if":
					return TokenType.If;
				case "in":
					return TokenType.In;
				case "local":
					return TokenType.Local;
				case "nil":
					return TokenType.Nil;
				case "not":
					return TokenType.Not;
				case "or":
					return TokenType.Or;
				case "repeat":
					return TokenType.Repeat;
				case "return":
					return TokenType.Return;
				case "then":
					return TokenType.Then;
				case "true":
					return TokenType.True;
				case "until":
					return TokenType.Until;
				case "while":
					return TokenType.While;
				default:
					return null;
			}
		}




		public bool IsEndOfBlock()
		{
			switch (Type)
			{
				case TokenType.Else:
				case TokenType.ElseIf:
				case TokenType.End:
				case TokenType.Until:
				case TokenType.Eof:
					return true;
				default:
					return false;
			}
		}

	}
}
