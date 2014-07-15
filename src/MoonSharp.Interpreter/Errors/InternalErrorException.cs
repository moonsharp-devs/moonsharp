using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;

namespace MoonSharp.Interpreter
{
	[Serializable]
	public class InternalErrorException : InterpreterException
	{
		internal InternalErrorException(string format, params object[] args)
			: base(format, args)
		{

		}
	}

}
