using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{

	public class CallStackItem
	{
		public int BasePointer;
		public int ReturnAddress;
		public int Debug_EntryPoint;
	}

}
