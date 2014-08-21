using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop
{
	// Only __eq, __tostring, __index and __newindex are supported.
	public class ReflectionMetatableBuilder
	{
		private static UserData GetUserData(CallbackArguments args)
		{
			DynValue v = args.AsType(0, "::ReflectionMetatableBuilder", DataType.UserData, false);
			return v.UserData;
		}


		public static DynValue reflection_tostring(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			UserData u = GetUserData(args);
			return DynValue.NewString(u.Object.ToString());
		}

		public static DynValue reflection_eq(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			UserData u = GetUserData(args);
			DynValue a = args[1];

			if (a.Type != DataType.UserData)
				return DynValue.False;

			return DynValue.NewBoolean(u.Object.Equals(a.UserData.Object));
		}

		public static DynValue reflection_index(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return DynValue.Nil;
		}











	}
}
