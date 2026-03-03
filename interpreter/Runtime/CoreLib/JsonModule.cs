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
			try
			{
				DynValue vs = args.AsType(0, "parse", DataType.String, false);
				DynValue parseEmptyArrays = args.AsType(1, "parse", DataType.Boolean, false);
				return JsonTableConverter.ParseString(vs.String, executionContext.GetScript(), parseEmptyArrays.Boolean);
			}
			catch (SyntaxErrorException ex)
			{
				throw new ScriptRuntimeException(ex);
			}
		}

		[MoonSharpModuleMethod]
		public static DynValue serialize(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			try
			{
				string s = JsonTableConverter.ObjectToJson(args[0]);
				return DynValue.NewString(s);
			}
			catch (SyntaxErrorException ex)
			{
				throw new ScriptRuntimeException(ex);
			}
		}

		[MoonSharpModuleMethod]
		public static DynValue isnull(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vs = args[0];
			return DynValue.NewBoolean((JsonNull.IsJsonNull(vs)) || (vs.IsNil()));
		}

		[MoonSharpModuleMethod]
		public static DynValue isemptyarray(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue vs = args[0];
			return DynValue.NewBoolean(JsonEmptyArray.IsJsonEmptyArray(vs));
		}

		[MoonSharpModuleMethod]
		public static DynValue @null(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return JsonNull.Create();
		}

		[MoonSharpModuleMethod]
		public static DynValue emptyarray(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return JsonEmptyArray.Create();
		}
	}
}
