using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr4.Runtime.Tree;

namespace MoonSharp.Interpreter
{
	[Serializable]
	public class SyntaxErrorException : Exception
	{
		internal SyntaxErrorException(IParseTree tree, string format, params object[] args)
			: base(string.Format(format, args))
		{
		}
	}
}
