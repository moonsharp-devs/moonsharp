using System;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Tree;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// Exception for all parsing/lexing errors. 
	/// </summary>
	[Serializable]
	public class SyntaxErrorException : InterpreterException
	{
		internal Token Token { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether this exception was caused by premature stream termination (that is, unexpected EOF).
		/// This can be used in REPL interfaces to tell between unrecoverable errors and those which can be recovered by extra input.
		/// </summary>
		public bool IsPrematureStreamTermination { get; set; }

		internal SyntaxErrorException(Token t, string format, params object[] args)
			: base(format, args)
		{
			Token = t;
		}

		internal SyntaxErrorException(Token t, string message)
			: base(message)
		{
			Token = t;
		}

		internal SyntaxErrorException(Script script, SourceRef sref, string format, params object[] args)
			: base(format, args)
		{
			DecorateMessage(script, sref);
		}

		internal SyntaxErrorException(Script script, SourceRef sref, string message)
			: base(message)
		{
			DecorateMessage(script, sref);
		}

		internal void DecorateMessage(Script script)
		{
			if (Token != null)
			{
				DecorateMessage(script, Token.GetSourceRef(false));
			}
		}
	}
}
