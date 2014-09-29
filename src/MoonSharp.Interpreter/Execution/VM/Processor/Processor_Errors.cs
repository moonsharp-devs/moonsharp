using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{
	sealed partial class Processor
	{
		private IList<StackTraceItem> GetCallStack(int instructionPtr)
		{
			List<StackTraceItem> callStack = new List<StackTraceItem>();

			for (int i = 0; i < m_ExecutionStack.Count; i++)
			{
				var c = m_ExecutionStack.Peek(i);

				var I = m_RootChunk.Code[c.Debug_EntryPoint];

				string callname = I.OpCode == OpCode.BeginFn ? I.Name : null;


				callStack.Add(new StackTraceItem()
				{
					EntryPoint = c.Debug_EntryPoint,
					CurrentInstruction = instructionPtr,
					SourceRef = m_RootChunk.Code[instructionPtr].SourceCodeRef,
					BasePtr = c.BasePointer,
					RetAddress = c.ReturnAddress,
					Name = callname,
				});

				instructionPtr = c.ReturnAddress;
			}

			return callStack;
		}

		private void FillDebugData(InterpreterException ex, int ip)
		{
			ex.DecoratedMessage = "chunk:0: " + ex.Message;

			// adjust IP
			if (ip == YIELD_SPECIAL_TRAP)
				ip = m_SavedInstructionPtr;
			else
				ip -= 1;

			ex.InstructionPtr = ip;
			ex.CallStack = GetCallStack(ip);
		}


	}
}
