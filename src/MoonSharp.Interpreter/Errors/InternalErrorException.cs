using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	[Serializable]
	public class InternalErrorException : InterpreterException
	{
		internal InternalErrorException(string message)
			: base(message)
		{

		}

		internal InternalErrorException(string format, params object[] args)
			: base(format, args)
		{

		}
	}

}
