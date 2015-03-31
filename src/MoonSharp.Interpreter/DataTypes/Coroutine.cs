using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// A class representing a script coroutine
	/// </summary>
	public class Coroutine : RefIdObject
	{
		/// <summary>
		/// Possible types of coroutine
		/// </summary>
		public enum CoroutineType
		{
			/// <summary>
			/// A valid coroutine
			/// </summary>
			Coroutine,
			/// <summary>
			/// A CLR callback assigned to a coroutine. 
			/// </summary>
			ClrCallback,
			/// <summary>
			/// A CLR callback assigned to a coroutine and already executed.
			/// </summary>
			ClrCallbackDead
		}

		/// <summary>
		/// Gets the type of coroutine
		/// </summary>
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


		/// <summary>
		/// Gets this coroutine as a typed enumerable which can be looped over for resuming.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<DynValue> AsTypedEnumerable()
		{
			while (this.State == CoroutineState.NotStarted || this.State == CoroutineState.Suspended)
				yield return Resume();
		}

		/// <summary>
		/// Gets this coroutine as a System.Collections.IEnumerator. This should bridge with Unity3D coroutines.
		/// </summary>
		/// <returns></returns>
		public System.Collections.IEnumerator AsEnumerator()
		{
			return AsTypedEnumerable().GetEnumerator();
		}


		/// <summary>
		/// Resumes the coroutine
		/// </summary>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		/// <exception cref="System.InvalidOperationException">Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead</exception>
		public DynValue Resume(params DynValue[] args)
		{
			if (Type == CoroutineType.Coroutine)
				return m_Processor.Coroutine_Resume(args);
			else 
				throw new InvalidOperationException("Only non-CLR coroutines can be resumed with this overload of the Resume method. Use the overload accepting a ScriptExecutionContext instead");
		}


		/// <summary>
		/// Resumes the coroutine
		/// </summary>
		/// <param name="context">The ScriptExecutionContext.</param>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Gets the coroutine state.
		/// </summary>
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

		/// <summary>
		/// Gets the coroutine stack trace for debug purposes
		/// </summary>
		/// <param name="skip">The skip.</param>
		/// <param name="entrySourceRef">The entry source reference.</param>
		/// <returns></returns>
		public WatchItem[] GetStackTrace(int skip, SourceRef entrySourceRef = null)
		{
			if (this.State != CoroutineState.Running)
			{
				entrySourceRef = m_Processor.GetCoroutineSuspendedLocation();
			}

			List<WatchItem> stack = m_Processor.Debugger_GetCallStack(entrySourceRef);
			return stack.Skip(skip).ToArray();
		}


	}
}
