using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter
{
	public class TailCallData
	{
		public DynValue Function { get; set; }
		public DynValue[] Args { get; set; }
		public CallMode Mode { get; set; }
	}
}
