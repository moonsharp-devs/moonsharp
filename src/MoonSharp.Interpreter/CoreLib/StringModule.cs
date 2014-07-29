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
		public static void MoonSharpInit(Table globalTable, Table stringTable)
		{
			Table stringMetatable = new Table(globalTable.OwnerScript);
			stringMetatable["__index"] = DynValue.NewTable(stringTable);
			globalTable.OwnerScript.SetTypeMetatable(DataType.String, stringMetatable);
		}

		[MoonSharpMethod]
		public static DynValue @byte(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vs = args.AsType(0, "byte", DataType.String, false);
			DynValue vi = args.AsType(1, "byte", DataType.Number, true);
			DynValue vj = args.AsType(2, "byte", DataType.Number, true);

			return PerformByteLike(vs, vi, vj,
				i => Unicode2Ascii(i));
		}

		[MoonSharpMethod]
		public static DynValue unicode(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vs = args.AsType(0, "unicode", DataType.String, false);
			DynValue vi = args.AsType(1, "unicode", DataType.Number, true);
			DynValue vj = args.AsType(2, "unicode", DataType.Number, true);

			return PerformByteLike(vs, vi, vj, i => i);
		}

		private static int Unicode2Ascii(int i)
		{
			if (i >= 0 && i < 255)
				return i;

			return (int)'?';
		}

		private static DynValue PerformByteLike(DynValue vs, DynValue vi, DynValue vj, Func<int, int> filter)
		{
			string s = vs.String;
			int? i = AdjustIndex(s, vi, 0);
			int? j = AdjustIndex(s, vj, i ?? 0);

			if (i == null || j == null || i > j)
				return DynValue.Nil;

			DynValue[] rets = new DynValue[j.Value - i.Value + 1];

			for (int ii = i.Value; ii <= j.Value; ii++)
				rets[ii] = DynValue.NewNumber(filter((int)s[ii]));

			return DynValue.NewTuple(rets);
		}


		private static int? AdjustIndex(string s, DynValue vi, int defval)
		{
			if (vi.IsNil())
				return defval;

			int i = (int)Math.Round(vi.Number, 0);

			if (i == 0)
				return null;

			if (i > 0)
				return i - 1;

			return s.Length - i;
		}

		[MoonSharpMethod]
		public static DynValue len(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vs = args.AsType(0, "len", DataType.String, false);
			return DynValue.NewNumber(vs.String.Length);
		}



		[MoonSharpMethod]
		public static DynValue match(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args.AsType(0, "match", DataType.String, false);
			DynValue p = args.AsType(1, "match", DataType.String, false);
			DynValue i = args.AsType(2, "match", DataType.Number, true);

			return PatternMatching.Match(s.String, p.String, i.IsNilOrNan() ? 1 : (int)i.Number);
		}


		[MoonSharpMethod()]
		public static DynValue gmatch(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args.AsType(0, "gmatch", DataType.String, false);
			DynValue p = args.AsType(1, "gmatch", DataType.String, false);

			return PatternMatching.GMatch(executionContext.GetScript(), s.String, p.String);
		}

		[MoonSharpMethod()]
		public static DynValue gsub(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args.AsType(0, "gsub", DataType.String, false);
			DynValue p = args.AsType(1, "gsub", DataType.String, false);
			DynValue v_i = args.AsType(3, "gsub", DataType.Number, true);
			int? i = v_i.IsNilOrNan() ? (int?)null : (int)v_i.Number;

			return PatternMatching.Str_Gsub(s.String, p.String, args[2], i);
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
