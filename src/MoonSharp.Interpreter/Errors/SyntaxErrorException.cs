using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Tree;

namespace MoonSharp.Interpreter
{
	[Serializable]
	public class SyntaxErrorException : InterpreterException
	{
		internal Token Token { get; private set; }

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
