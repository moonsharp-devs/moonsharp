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


		internal Table GetMetatable(DynValue value)
		{
			if (value.Type == DataType.Table)
			{
				return value.Table.MetaTable;
			}
			else
			{
				return null;
			}
		}

		internal DynValue GetMetamethod(DynValue value, string metamethod)
		{
			var metatable = GetMetatable(value);

			if (metatable == null)
				return null;

			var metameth = metatable.RawGet(metamethod);
			
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
