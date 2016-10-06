using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Serialization.Json;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace = "json")]
	public class JsonModule
	{
		[MoonSharpModuleMethod]
		public static DynValue parse(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vs = args.AsType(0, "parse", DataType.String, false);
			Table t = JsonTableConverter.JsonToTable(vs.String, executionContext.GetScript());
			return DynValue.NewTable(t);
		}

		[MoonSharpModuleMethod]
		public static DynValue serialize(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vt = args.AsType(0, "serialize", DataType.Table, false);
			string s = JsonTableConverter.TableToJson(vt.Table);
			return DynValue.NewString(s);
		}

		[MoonSharpModuleMethod]
		public static DynValue isnull(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vs = args[0];
			return DynValue.NewBoolean((JsonNull.IsJsonNull(vs)) || (vs.IsNil()));
		}

		[MoonSharpModuleMethod]
		public static DynValue @null(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return JsonNull.Create();
		}
	}
}
