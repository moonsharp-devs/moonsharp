using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{
	public class CallStackItem
	{
		public int Debug_EntryPoint;
		public LRef[] Debug_Symbols;

		public int BasePointer;
		public int ReturnAddress;
		public RValue[] LocalScope;
		public ClosureContext ClosureScope;
	}

}
