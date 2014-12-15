using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{
	[Flags]
	public enum CallStackItemFlags
	{
		None = 0,

		EntryPoint = 1,
		ResumeEntryPoint = 3,
		CallEntryPoint = 5,
	}
}
