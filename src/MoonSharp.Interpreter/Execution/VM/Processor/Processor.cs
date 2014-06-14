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
		Chunk m_RootChunk;
		Chunk m_CurChunk;
		int m_InstructionPtr;
		bool m_Terminate = false;

		FastStack<RValue> m_ValueStack = new FastStack<RValue>(131072);
		FastStack<CallStackItem> m_ExecutionStack = new FastStack<CallStackItem>(131072);

		IDebugger m_DebuggerAttached = null;
		DebuggerAction.ActionType m_DebuggerCurrentAction = DebuggerAction.ActionType.None;
		int m_DebuggerCurrentActionTarget = -1;

		Table m_GlobalTable = new Table();

		public Processor(Chunk rootChunk)
		{
			m_RootChunk = m_CurChunk = rootChunk;
			m_InstructionPtr = 0;
		}

		public void Reset(Table global)
		{
			m_CurChunk = m_RootChunk;
			m_InstructionPtr = 0;
			m_GlobalTable = global;
		}

		public RValue InvokeRoot()
		{
			m_ValueStack.Push(new RValue(0));  // func val
			m_ValueStack.Push(new RValue(0));  // func args count
			m_ExecutionStack.Push(new CallStackItem()
			{
				BasePointer = m_ValueStack.Count,
				Debug_EntryPoint = 0,
				ReturnAddress = -1,
			});

			return Processing_Loop();
		}
	}
}
