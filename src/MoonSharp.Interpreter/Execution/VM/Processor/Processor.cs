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


		Script m_Script;
		Processor m_Parent = null;
		List<Processor> m_Coroutines = null;
		CoroutineState m_State;
		bool m_CanYield = true;
		int m_SavedInstructionPtr = -1;
		DebugContext m_Debug;
		int m_CoroutineIndex = 0;

		public Processor(Script script, Table globalContext, ByteCode byteCode)
		{
			m_Debug = new DebugContext();
			m_RootChunk = byteCode;
			m_GlobalTable = globalContext;
			m_Script = script;
			m_Coroutines = new List<Processor>();
			m_State = CoroutineState.Main;
			m_CoroutineIndex = 0;
			m_Coroutines.Add(this);
		}

		private Processor(Processor parentProcessor, int corIndex)
		{
			m_Debug = parentProcessor.m_Debug;
			m_RootChunk = parentProcessor.m_RootChunk;
			m_GlobalTable = parentProcessor.m_GlobalTable;
			m_Script = parentProcessor.m_Script;
			m_Coroutines = parentProcessor.m_Coroutines;
			m_Parent = parentProcessor;
			m_State = CoroutineState.NotStarted;
			m_CoroutineIndex = corIndex;
		}

		


		public DynValue Call(DynValue function, DynValue[] args)
		{
			m_CanYield = false;

			try
			{
				int entrypoint = PushClrToScriptStackFrame(function, args);
				return Processing_Loop(entrypoint);
			}
			finally
			{
				m_CanYield = true;
			}
		}

		// pushes all what's required to perform a clr-to-script function call. function can be null if it's already
		// at vstack top.
		private int PushClrToScriptStackFrame(DynValue function, DynValue[] args)
		{
			if (function == null) 
				function = m_ValueStack.Peek();
			else
				m_ValueStack.Push(function);  // func val

			args = Internal_AdjustTuple(args);

			for (int i = 0; i < args.Length; i++)
				m_ValueStack.Push(args[i]);

			m_ValueStack.Push(DynValue.NewNumber(args.Length));  // func args count

			m_ExecutionStack.Push(new CallStackItem()
			{
				BasePointer = m_ValueStack.Count,
				Debug_EntryPoint = function.Function.EntryPointByteCodeLocation,
				ReturnAddress = -1,
				ClosureScope = function.Function.ClosureContext,
			});

			return function.Function.EntryPointByteCodeLocation;
		}













	}
}
