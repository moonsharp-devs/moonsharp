using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// Exception thrown when a dynamic expression is invalid
	/// </summary>
	public class DynamicExpressionException : ScriptRuntimeException
	{
		public DynamicExpressionException(string format, params object[] args)
			: base("<dynamic>: " + format, args)
		{

		}
		public DynamicExpressionException(string message)
			: base("<dynamic>: " + message)
		{

		}
	}
}
