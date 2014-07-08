using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib.Patterns;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace = "table")]
	public class TableModule
	{
		[MoonSharpMethod()]
		public static DynValue unpack(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args.AsType(0, "unpack", DataType.Table, false);
			Table t = s.Table;

			DynValue[] v = new DynValue[(int)t.Length];

			for (int i = 1; i <= v.Length; i++)
				v[i - 1] = t[i];

			return DynValue.NewTuple(v);
		}

		[MoonSharpMethod()]
		public static DynValue pack(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args[0];
			Table t = new Table(executionContext.GetOwnerScript());
			DynValue v = DynValue.NewTable(t);

			if (s.IsNil())
				return v;

			if (s.Type == DataType.Tuple)
			{
				for (int i = 0; i < s.Tuple.Length; i++)
					t[i + 1] = s.Tuple[i];
			}
			else
			{
				t[1] = s;
			}

			return v;
		}

	}


	[MoonSharpModule]
	public class TableModule_Globals
	{
		[MoonSharpMethod()]
		public static DynValue unpack(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return TableModule.unpack(executionContext, args);
		}

		[MoonSharpMethod()]
		public static DynValue pack(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return TableModule.pack(executionContext, args);
		}
	}


}
