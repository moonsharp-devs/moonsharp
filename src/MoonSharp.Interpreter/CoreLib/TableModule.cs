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
			// !INCOMPAT

			// The theory says the method calls __len to get the len in case it's not forced as a param.
			// But then it uses rawget to access. The only case where we differ if we take the shortcut
			// of using rawlen is if the first param is passed to force a non-first index and the second 
			// isn't, or if __len is used to limit the maximum length. Likely an acceptable divergence, 
			// at least for now. [Note that this behaviour is actually undefined in Lua 5.1, and __len 
			// usage is documented only for Lua 5.2]

			DynValue vlist = args.AsType(0, "concat", DataType.Table, false);
			DynValue vsep = args.AsType(1, "concat", DataType.String, true);
			DynValue vstart = args.AsType(2, "concat", DataType.Number, true);
			DynValue vend = args.AsType(3, "concat", DataType.Number, true);

			Table list = vlist.Table;
			string sep = vsep.IsNil() ? "" : vsep.String;
			int start = vstart.IsNilOrNan() ? 1 : (int)vstart.Number;
			int end = vend.IsNilOrNan() ? (int)list.Length : (int)vend.Number;

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
