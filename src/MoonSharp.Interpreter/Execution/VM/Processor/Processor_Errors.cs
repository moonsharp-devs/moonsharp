using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
	sealed partial class Processor
	{
		private SourceRef GetCurrentSourceRef(int instructionPtr)
		{
			if (instructionPtr >= 0 && instructionPtr < m_RootChunk.Code.Count)
			{
				return m_RootChunk.Code[instructionPtr].SourceCodeRef;
			}
			return null;
		}


		private void FillDebugData(InterpreterException ex, int ip)
		{
			// adjust IP
			if (ip == YIELD_SPECIAL_TRAP)
				ip = m_SavedInstructionPtr;
			else
				ip -= 1;

			ex.InstructionPtr = ip;

			SourceRef sref = GetCurrentSourceRef(ip);

			if (sref != null)
			{
				ex.DecoratedMessage = string.Format("{0}: {1}", sref.FormatLocation(m_Script), ex.Message);
			}
			else
			{
				ex.DecoratedMessage = string.Format("bytecode:{0}: {1}", ip, ex.Message);
			}

			ex.CallStack = Debugger_GetCallStack(sref);
		}


	}
}
