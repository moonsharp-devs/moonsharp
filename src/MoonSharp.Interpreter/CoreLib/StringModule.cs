using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib.StringLib;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace="string")]
	public class StringModule
	{
		public static void MoonSharpInit(Table globalTable, Table stringTable)
		{
			Table stringMetatable = new Table(globalTable.OwnerScript);
			stringMetatable.Set("__index", DynValue.NewTable(stringTable));
			globalTable.OwnerScript.SetTypeMetatable(DataType.String, stringMetatable);
		}

		[MoonSharpMethod]
		public static DynValue @char(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			StringBuilder sb = new StringBuilder(args.Count);

			for (int i = 0; i < args.Count; i++)
			{
				DynValue v = args[i];
				double d = 0d;

				if (v.Type == DataType.String)
				{
					double? nd = v.CastToNumber();
					if (nd == null)
						args.AsType(i, "char", DataType.Number, false);
					else
						d = nd.Value;
				}
				else
				{
					args.AsType(i, "char", DataType.Number, false);
					d = v.Number;
				}

				sb.Append((char)(d));
			}

			return DynValue.NewString(sb.ToString());
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
            StringRange range = StringRange.FromLuaRange(vi, vj, null);
            string s = range.ApplyToString(vs.String);

            int length = s.Length;
			DynValue[] rets = new DynValue[length];

            for (int i = 0; i < length; ++i)
            {
                rets[i] = DynValue.NewNumber(filter((int)s[i]));
            }

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


		[MoonSharpMethod]
		public static DynValue gmatch(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args.AsType(0, "gmatch", DataType.String, false);
			DynValue p = args.AsType(1, "gmatch", DataType.String, false);

			return PatternMatching.GMatch(executionContext.GetScript(), s.String, p.String);
		}

		[MoonSharpMethod]
		public static DynValue gsub(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue s = args.AsType(0, "gsub", DataType.String, false);
			DynValue p = args.AsType(1, "gsub", DataType.String, false);
			DynValue v_i = args.AsType(3, "gsub", DataType.Number, true);
			int? i = v_i.IsNilOrNan() ? (int?)null : (int)v_i.Number;

			return PatternMatching.Str_Gsub(executionContext, s.String, p.String, args[2], i);
		}

		[MoonSharpMethod]
		public static DynValue find(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v_s = args.AsType(0, "find", DataType.String, false);
			DynValue v_p = args.AsType(1, "find", DataType.String, false);
			DynValue v_i = args.AsType(2, "find", DataType.Number, true);
			DynValue v_plain = args.AsType(3, "find", DataType.Boolean, true);

			int i = v_i.IsNilOrNan() ? 1 : (int)v_i.Number;

			bool plain = v_plain.CastToBool();

			return PatternMatching.Str_Find(v_s.String, v_p.String, i, plain);
		}


        [MoonSharpMethod]
        public static DynValue lower(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg_s = args.AsType(0, "lower", DataType.String, false);

            return DynValue.NewString(arg_s.String.ToLower());
        }

        [MoonSharpMethod]
        public static DynValue upper(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg_s = args.AsType(0, "upper", DataType.String, false);

            return DynValue.NewString(arg_s.String.ToUpper());
        }

        [MoonSharpMethod]
        public static DynValue rep(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg_s = args.AsType(0, "rep", DataType.String, false);
            DynValue arg_n = args.AsType(1, "rep", DataType.Number, false);

            if (String.IsNullOrEmpty(arg_s.String) || (arg_n.Number < 1))
            {
                return DynValue.NewString("");
            }

            int count = (int)arg_n.Number;
            StringBuilder result = new StringBuilder(arg_s.String.Length * count);

            for (int i = 0; i < count; ++i)
            {
                result.Append(arg_s.String);
            }

            return DynValue.NewString(result.ToString());
        }

		[MoonSharpMethod]
		public static DynValue format(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			string str = PatternMatching.Str_Format(executionContext, args);
			return DynValue.NewString(str);
		}



        [MoonSharpMethod]
        public static DynValue reverse(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg_s = args.AsType(0, "reverse", DataType.String, false);

            if (String.IsNullOrEmpty(arg_s.String))
            {
                return DynValue.NewString("");
            }

            char[] elements = arg_s.String.ToCharArray();
            Array.Reverse(elements);

            return DynValue.NewString(new String(elements));
        }

        [MoonSharpMethod]
        public static DynValue sub(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue arg_s = args.AsType(0, "sub", DataType.String, false);
			DynValue arg_i = args.AsType(1, "sub", DataType.Number, true);
            DynValue arg_j = args.AsType(2, "sub", DataType.Number, true);

			StringRange range = StringRange.FromLuaRange(arg_i, arg_j, -1);
            string s = range.ApplyToString(arg_s.String);

            return DynValue.NewString(s);
        }
	}


}
