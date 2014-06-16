using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{
	sealed partial class Processor : IExecutionContext
	{
		RValue IExecutionContext.GetVar(LRef symref)
		{
			return this.GetGenericSymbol(symref);
		}

		void IExecutionContext.SetVar(LRef symref, RValue value)
		{
			AssignGenericSymbol(symref, value);
		}

		LRef IExecutionContext.FindVar(string name)
		{
			return FindRefByName(name);
		}

		RValue IExecutionContext.GetMetamethod(RValue value, string metamethod)
		{
			if (value.Meta == null)
				return null;

			if (value.Meta.Type != DataType.Table)
				throw new InternalErrorException("Metatable is not a table!");

			return value.Meta.Table.RawGet(metamethod);
		}

		RValue IExecutionContext.GetMetamethodTailCall(RValue value, string metamethod, params RValue[] args)
		{
			RValue meta = ((IExecutionContext)this).GetMetamethod(value, metamethod);

			if (meta == null) return null;

			return new RValue(meta, args);
		}
	}
}
