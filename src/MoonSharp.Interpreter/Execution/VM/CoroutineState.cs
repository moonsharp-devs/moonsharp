using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;

namespace MoonSharp.Interpreter.Execution.VM
{
	public enum CoroutineState
	{
		Main,
		NotStarted,
		Suspended,
		Running,
		Dead
	}
}
