using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	public class DynamicExpressionException : ScriptRuntimeException
	{
		public DynamicExpressionException(string format, params object[] args)
			: base("<dynamic>: " + format, args)
		{

		}
	}
}
