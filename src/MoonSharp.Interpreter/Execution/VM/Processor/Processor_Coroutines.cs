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

			// Put the closure as first value on the stack, for future reference
			P.m_ValueStack.Push(DynValue.NewClosure(closure));

			// Return the coroutine handle
			return DynValue.NewCoroutine(new Coroutine(P));
		}

		public CoroutineState State { get { return m_State; } }
		public Coroutine AssociatedCoroutine { get; set; }

		public DynValue Coroutine_Resume(DynValue[] args)
		{
			int entrypoint = 0;

			if (m_State != CoroutineState.NotStarted && m_State != CoroutineState.Suspended)
				throw ScriptRuntimeException.CannotResumeNotSuspended(m_State);

			if (m_State == CoroutineState.NotStarted)
				entrypoint = PushClrToScriptStackFrame(null, args);
			else
			{
				m_ValueStack.Push(DynValue.NewTuple(args));
				entrypoint = m_SavedInstructionPtr;
			}

			m_State = CoroutineState.Running;
			DynValue retVal = Processing_Loop(entrypoint);

			if (retVal.Type == DataType.YieldRequest)
			{
				m_State = CoroutineState.Suspended;
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
