using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{
	sealed partial class Processor : IExecutionContext
	{
		DynValue IExecutionContext.GetVar(SymbolRef symref)
		{
			return this.GetGenericSymbol(symref);
		}

		void IExecutionContext.SetVar(SymbolRef symref, DynValue value)
		{
			AssignGenericSymbol(symref, value);
		}

		SymbolRef IExecutionContext.FindVar(string name)
		{
			return FindRefByName(name);
		}

		DynValue IExecutionContext.GetMetamethod(DynValue value, string metamethod)
		{
			if (value.Meta == null || value.Type == DataType.Nil)
				return null;

			if (value.Meta.Type != DataType.Table)
				throw new InternalErrorException("Metatable is not a table!");

			var metameth = value.Meta.Table.RawGet(metamethod);
			
			if (metameth == null || metameth.Type == DataType.Nil)
				return null;

			return metameth;
		}

		DynValue IExecutionContext.GetMetamethodTailCall(DynValue value, string metamethod, params DynValue[] args)
		{
			DynValue meta = ((IExecutionContext)this).GetMetamethod(value, metamethod);

			if (meta == null) return null;

			return DynValue.NewTailCallReq(meta, args);
		}


		Script IExecutionContext.GetOwnerScript()
		{
			return m_Script;
		}
	}
}
