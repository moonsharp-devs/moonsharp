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

		public Table Closure { get { return m_Callback.Closure; } }


		public DynValue GetVar(SymbolRef symref)
		{
			if (CheckUpValue(symref))
				return m_Callback.Closure[symref.Name];

			return m_Processor.GetVar(symref);
		}

		public void SetVar(SymbolRef symref, DynValue value)
		{
			if (CheckUpValue(symref))
				m_Callback.Closure[symref.Name] = value;

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

		public Script GetOwnerScript()
		{
			return m_Processor.GetOwnerScript();
		}

		private bool CheckUpValue(SymbolRef symref)
		{
			if (symref.Type != SymbolRefType.Upvalue)
				return false;

			if (m_Callback.Closure == null)
				throw new ArgumentException("symref.Type is Upvalue on null CLR Closure");

			return true;
		}

	}
}
