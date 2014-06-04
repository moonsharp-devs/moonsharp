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

		FastStack<RValue> m_ValueStack = new FastStack<RValue>(131072);
		FastStack<CallStackItem> m_ExecutionStack = new FastStack<CallStackItem>(131072);
		bool m_Terminate = false;
		RuntimeScope m_Scope;
		RValue[] m_TempRegs = new RValue[8];

		IDebugger m_DebuggerAttached = null;
		DebuggerAction.ActionType m_DebuggerCurrentAction = DebuggerAction.ActionType.None;
		int m_DebuggerCurrentActionTarget = -1;

		public Processor(Chunk rootChunk)
		{
			m_RootChunk = m_CurChunk = rootChunk;
			m_InstructionPtr = 0;
			m_Scope = new RuntimeScope();
		}

		public void Reset(Table global)
		{
			m_CurChunk = m_RootChunk;
			m_InstructionPtr = 0;
			m_Scope.GlobalTable = global;
		}


	}
}
