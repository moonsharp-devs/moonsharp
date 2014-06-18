using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
	sealed partial class Processor
	{
		ByteCode m_RootChunk;

		FastStack<DynValue> m_ValueStack = new FastStack<DynValue>(131072);
		FastStack<CallStackItem> m_ExecutionStack = new FastStack<CallStackItem>(131072);
		Table m_GlobalTable;

		IDebugger m_DebuggerAttached = null;
		DebuggerAction.ActionType m_DebuggerCurrentAction = DebuggerAction.ActionType.None;
		int m_DebuggerCurrentActionTarget = -1;
		Script m_Script;

		public Processor(Script script, Table globalContext, ByteCode byteCode)
		{
			m_RootChunk = byteCode;
			m_GlobalTable = globalContext;
			m_Script = script;
		}


		public DynValue Call(DynValue function, DynValue[] args)
		{
			m_ValueStack.Push(function);  // func val

			args = Internal_AdjustTuple(args);

			for (int i = 0; i < args.Length; i++)
				m_ValueStack.Push(args[i]);
			
			m_ValueStack.Push(DynValue.NewNumber(args.Length));  // func args count

			m_ExecutionStack.Push(new CallStackItem()
			{
				BasePointer = m_ValueStack.Count,
				Debug_EntryPoint = function.Function.ByteCodeLocation,
				ReturnAddress = -1,
			});

			return Processing_Loop(function.Function.ByteCodeLocation);
		}
	}
}
