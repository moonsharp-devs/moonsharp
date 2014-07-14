using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{
	public class CallStackItem
	{
		public int Debug_EntryPoint;
		public SymbolRef[] Debug_Symbols;

		public CallbackFunction Continuation;
		public CallbackFunction ErrorHandler;

		public int BasePointer;
		public int ReturnAddress;
		public DynValue[] LocalScope;
		public ClosureContext ClosureScope;
	}

}
