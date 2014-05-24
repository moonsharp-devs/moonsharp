using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Diagnostics
{
	class Debug
	{
		[System.Diagnostics.Conditional("DEBUG")]
		public static void WriteLine(string source, string msg, params object[] args)
		{
			System.Diagnostics.Debug.WriteLine(string.Format(msg, args));
		}

	}
}
