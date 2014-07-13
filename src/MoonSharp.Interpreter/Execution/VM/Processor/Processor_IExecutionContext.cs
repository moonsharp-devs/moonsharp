using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{
	sealed partial class Processor
	{
		internal DynValue GetVar(SymbolRef symref)
		{
			return this.GetGenericSymbol(symref);
		}

		internal void SetVar(SymbolRef symref, DynValue value)
		{
			AssignGenericSymbol(symref, value);
		}

		internal SymbolRef FindVar(string name)
		{
			return FindRefByName(name);
		}

		internal DynValue GetMetamethod(DynValue value, string metamethod)
		{
			if (value.MetaTable == null || value.Type == DataType.Nil)
				return null;

			var metameth = value.MetaTable.RawGet(metamethod);
			
			if (metameth == null || metameth.Type == DataType.Nil)
				return null;

			return metameth;
		}

		internal Script GetOwnerScript()
		{
			return m_Script;
		}
	}
}
