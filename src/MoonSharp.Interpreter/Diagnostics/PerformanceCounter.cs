using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Diagnostics
{
	public enum PerformanceCounter
	{
		Parsing,
		AstCreation,
		Compilation,
		Execution,
		AdaptersCompilation,

		LastValue
	}
}
