using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Execution
{
	public interface IExecutionContext
	{
		DynValue GetVar(SymbolRef symref);
		void SetVar(SymbolRef symref, DynValue value);
		SymbolRef FindVar(string name);
		DynValue GetMetamethod(DynValue value, string metamethod);
		DynValue GetMetamethodTailCall(DynValue value, string metamethod, params DynValue[] args);
		Script GetOwnerScript();
	}
}
