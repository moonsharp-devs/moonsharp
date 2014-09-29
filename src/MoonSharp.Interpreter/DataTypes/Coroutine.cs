using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter
{
	public class Coroutine : RefIdObject
	{
		public enum CoroutineType
		{
			Coroutine,
			ClrCallback,
			ClrCallbackDead
		}

		public  CoroutineType Type { get; private set; }

		private CallbackFunction m_ClrCallback;
		private Processor m_Processor;


		internal Coroutine(CallbackFunction function)
		{
			Type = CoroutineType.ClrCallback;
			m_ClrCallback = function;
		}

		internal Coroutine(Processor proc)
		{
			Type = CoroutineType.Coroutine;
			m_Processor = proc;
			m_Processor.AssociatedCoroutine = this;
		}

		internal void MarkClrCallbackAsDead()
		{
			if (Type != CoroutineType.ClrCallback)
				throw new InvalidOperationException("State must be CoroutineType.ClrCallback");

			Type = CoroutineType.ClrCallbackDead;
		}

		
		public DynValue Resume(params DynValue[] args)
		{
			if (Type == CoroutineType.Coroutine)
				return m_Processor.Coroutine_Resume(args);
			else 
				throw new InvalidOperationException("Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead");
		}


		public DynValue Resume(ScriptExecutionContext context, params DynValue[] args)
		{
			if (Type == CoroutineType.Coroutine)
				return m_Processor.Coroutine_Resume(args);
			else if (Type == CoroutineType.ClrCallback)
			{
				DynValue ret = m_ClrCallback.Invoke(context, args);
				MarkClrCallbackAsDead();
				return ret;
			}
			else
				throw ScriptRuntimeException.CannotResumeNotSuspended(CoroutineState.Dead);
		}

		public CoroutineState State
		{
			get
			{
				if (Type == CoroutineType.ClrCallback)
					return CoroutineState.NotStarted;
				else if (Type == CoroutineType.ClrCallbackDead)
					return CoroutineState.Dead;
				else
					return m_Processor.State;
			}
		}



	}
}
