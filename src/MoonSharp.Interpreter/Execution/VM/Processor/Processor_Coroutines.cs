using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.Interpreter.Execution.VM
{
	// This part is practically written procedural style - it looks more like C than C#.
	// This is intentional so to avoid this-calls and virtual-calls as much as possible.
	// Same reason for the "sealed" declaration.
	sealed partial class Processor
	{
		public DynValue Coroutine_Create(Closure closure)
		{
			// create a processor instance
			Processor P = new Processor(this);
			int coroutineHandle = this.m_Coroutines.Count;
			this.m_Coroutines.Add(P);

			// Put the closure as first value on the stack, for future reference
			P.m_ValueStack.Push(DynValue.NewClosure(closure));

			// Return the coroutine handle
			return DynValue.NewCoroutine(coroutineHandle);
		}

		public Processor Coroutine_Get(DynValue handle)
		{
			return m_Coroutines[handle.CoroutineHandle];
		}

		public DynValue Coroutine_GetRunning()
		{
			for (int coroutineHandle = 0; coroutineHandle < m_Coroutines.Count; coroutineHandle++)
			{
				if (m_Coroutines[coroutineHandle] == this)
					return DynValue.NewCoroutine(coroutineHandle);
			}

			throw new InternalErrorException("coroutine list exception in Coroutine_GetRunning - coroutine not found");
		}

		public CoroutineState State { get { return m_State; } }

		public DynValue Coroutine_Resume(DynValue[] args)
		{
			int entrypoint = 0;

			if (m_State != CoroutineState.NotStarted && m_State != CoroutineState.Suspended)
				throw ScriptRuntimeException.CannotResumeNotSuspended(m_State);

			if (m_State == CoroutineState.NotStarted)
				entrypoint = PushClrToScriptStackFrame(null, args);
			else
				entrypoint = m_SavedInstructionPtr;

			m_State = CoroutineState.Running;
			DynValue retVal = Processing_Loop(entrypoint);

			if (retVal.Type == DataType.YieldRequest)
			{
				m_State = CoroutineState.Suspended;
				m_SavedInstructionPtr = retVal.YieldRequest.InstructionPtr;
				return DynValue.NewTuple(retVal.YieldRequest.ReturnValues);
			}
			else
			{
				m_State = CoroutineState.Dead;
				return retVal;
			}
		}



	}

}
