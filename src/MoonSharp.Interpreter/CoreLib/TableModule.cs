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

		//table.concat (list [, sep [, i [, j]]])
		//Given a list where all elements are strings or numbers, returns the string list[i]..sep..list[i+1] (...) sep..list[j]. 
		//The default value for sep is the empty string, the default for i is 1, and the default for j is #list. If i is greater 
		//than j, returns the empty string. 
		[MoonSharpMethod()]
		public static DynValue concat(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vlist = args.AsType(0, "concat", DataType.Table, false);
			DynValue vsep = args.AsType(1, "concat", DataType.String, true);
			DynValue vstart = args.AsType(2, "concat", DataType.Number, true);
			DynValue vend = args.AsType(3, "concat", DataType.Number, true);



			Table list = vlist.Table;
			string sep = vsep.IsNil() ? "" : vsep.String;
			int start = vstart.IsNilOrNan() ? 1 : (int)vstart.Number;
			int end; 

			if (vend.IsNilOrNan())
			{
				DynValue __len = executionContext.GetMetamethod(vlist, "__len");

				if (__len != null)
				{
					DynValue lenv = executionContext.GetOwnerScript().Call(__len, vlist);

					double? len = lenv.CastToNumber();

					if (len == null)
						throw new ScriptRuntimeException("object length is not a number");

					end = (int)len;
				}
				else
				{
					end = (int)vlist.Table.Length;
				}
			}
			else 
			{
				end = (int)vend.Number;
			}

			if (end < start)
				return DynValue.NewString(string.Empty);

			StringBuilder sb = new StringBuilder();

			for (int i = start; i <= end; i++)
			{
				DynValue v = list[i];

				if (v.Type != DataType.Number && v.Type != DataType.String)
					throw new ScriptRuntimeException("invalid value (boolean) at index {0} in table for 'concat'", i);

				string s = v.ToPrintString();

				if (i != start)
					sb.Append(sep);
				
				sb.Append(s);

			}

			return DynValue.NewString(sb.ToString());
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
