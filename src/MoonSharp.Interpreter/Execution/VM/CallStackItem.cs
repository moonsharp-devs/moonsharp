using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
	public class CallStackItem
	{
		public int Debug_EntryPoint;
		public SymbolRef[] Debug_Symbols;

		public SourceRef CallingSourceRef;

		public CallbackFunction Continuation;
		public CallbackFunction ErrorHandler;

		public int BasePointer;
		public int ReturnAddress;
		public DynValue[] LocalScope;
		public ClosureContext ClosureScope;
	}

}
