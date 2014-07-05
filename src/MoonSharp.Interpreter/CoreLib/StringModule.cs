using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib.Patterns;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace="string")]
	public class StringModule
	{
		[MoonSharpMethod()]
		public static DynValue gmatch(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args.AsType(0, "gmatch", DataType.String, false);
			DynValue p = args.AsType(1, "gmatch", DataType.String, false);

			return PatternMatching.GMatch(s.String, p.String);
		}

		[MoonSharpMethod()]
		public static DynValue find(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v_s = args.AsType(0, "find", DataType.String, false);
			DynValue v_p = args.AsType(1, "find", DataType.String, false);
			DynValue v_i = args.AsType(2, "find", DataType.Number, true);
			DynValue v_plain = args.AsType(3, "find", DataType.Boolean, true);

			int i = v_i.IsNilOrNan() ? int.MinValue : (int)v_i.Number;

			bool plain = v_plain.CastToBool();

			return PatternMatching.Str_Find(v_s.String, v_p.String, i, plain);
		}

	}
}
