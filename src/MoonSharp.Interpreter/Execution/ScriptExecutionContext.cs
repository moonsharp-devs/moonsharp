using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Execution
{
	public class ScriptExecutionContext
	{
		Processor m_Processor;
		CallbackFunction m_Callback;

		internal ScriptExecutionContext(Processor p, CallbackFunction callBackFunction)
		{
			m_Processor = p;
			m_Callback = callBackFunction;
		}

		public Table Closure 
		{ 
			get { return m_Callback.Closure; } 
			set { m_Callback.Closure = value; } 
		}


		public DynValue GetVar(SymbolRef symref)
		{
			if (CheckUpValue(symref))
				return m_Callback.Closure.Get(symref.Name);

			return m_Processor.GetVar(symref);
		}

		public void SetVar(SymbolRef symref, DynValue value)
		{
			if (CheckUpValue(symref))
				m_Callback.Closure.Set(symref.Name, value);

			m_Processor.SetVar(symref, value);
		}


		public Table GetMetatable(DynValue value)
		{
			return m_Processor.GetMetatable(value);
		}


		public DynValue GetMetamethod(DynValue value, string metamethod)
		{
			return m_Processor.GetMetamethod(value, metamethod);
		}

		public DynValue GetMetamethodTailCall(DynValue value, string metamethod, params DynValue[] args)
		{
			DynValue meta = this.GetMetamethod(value, metamethod);
			if (meta == null) return null;
			return DynValue.NewTailCallReq(meta, args);
		}

		public DynValue GetBinaryMetamethod(DynValue op1, DynValue op2, string eventName)
		{
			return m_Processor.GetBinaryMetamethod(op1, op2, eventName);
		}

		public Script GetScript()
		{
			return m_Processor.GetScript();
		}

		private bool CheckUpValue(SymbolRef symref)
		{
			if (symref.Type != SymbolRefType.Upvalue)
				return false;

			if (m_Callback.Closure == null)
				throw new ArgumentException("symref.Type is Upvalue on null CLR Closure");

			return true;
		}

		public DynValue CoroutineCreate(Closure closure)
		{
			return m_Processor.Coroutine_Create(closure);
		}

		public DynValue CoroutineResume(DynValue handle, DynValue[] args)
		{
			Processor P = m_Processor.Coroutine_Get(handle);
			return P.Coroutine_Resume(args);
		}

		public DynValue CoroutineRunning()
		{
			return m_Processor.Coroutine_GetRunning();
		}

		public CoroutineState CoroutineGetState(DynValue handle)
		{
			return m_Processor.Coroutine_Get(handle).State;
		}

		public bool CoroutineIsRunning(DynValue handle)
		{
			DynValue running = CoroutineRunning();
			return (running.CoroutineHandle == handle.CoroutineHandle) ;
		}



	}
}
