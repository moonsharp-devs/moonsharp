using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	[Serializable]
	public class SyntaxErrorException : InterpreterException
	{
		internal SyntaxErrorException(string format, params object[] args)
			: base(format, args)
		{

		}

		internal SyntaxErrorException(string message)
			: base(message)
		{

		}

	}
}
