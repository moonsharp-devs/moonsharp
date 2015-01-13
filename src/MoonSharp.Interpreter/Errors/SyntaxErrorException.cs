using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr4.Runtime.Tree;

namespace MoonSharp.Interpreter
{
	[Serializable]
	public class SyntaxErrorException : InterpreterException
	{
		internal SyntaxErrorException(string format, params object[] args)
			: base(format, args)
		{

		}

		internal SyntaxErrorException(IParseTree tree, string format, params object[] args)
			: base(tree, format, args)
		{

		}

		internal SyntaxErrorException(string message)
			: base(message)
		{

		}

		internal SyntaxErrorException(IParseTree tree, string message)
			: base(tree, message)
		{

		}
	}
}
