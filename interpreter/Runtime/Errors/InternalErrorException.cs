using System;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// Exception thrown when an inconsistent state is reached in the interpreter
	/// </summary>
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
