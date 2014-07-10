using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule]
	public class RawTableAccess
	{
		[MoonSharpMethod]
		static DynValue rawget(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue table = args.AsType(0, "rawget", DataType.Table);
			DynValue index = args[1];

			return table.Table[index];
		}

		[MoonSharpMethod]
		static DynValue rawset(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue table = args.AsType(0, "rawset", DataType.Table);
			DynValue index = args[1];
			DynValue val = args[2];

			table.Table[index] = val;

			return table;
		}
	}
}
