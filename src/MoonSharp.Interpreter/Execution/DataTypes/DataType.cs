
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public enum DataType
	{
		Nil,
		Boolean,
		Number,
		String,
		Function,
		Table,
		Tuple,

		Symbol,
		ClrFunction,

		UNSUPPORTED_UserData,
		UNSUPPORTED_Thread,
	}

	public static class LuaTypeExtensions
	{
		public static string ToLuaTypeString(this DataType type)
		{
			switch (type)
			{
				case DataType.Nil:
					return "nil";
				case DataType.Boolean:
					return "boolean";
				case DataType.Number:
					return "number";
				case DataType.String:
					return "string";
				case DataType.Function:
					return "function";
				case DataType.ClrFunction:
					return "function";
				case DataType.Table:
					return "table";
				case DataType.UNSUPPORTED_UserData:
					return "userdata";
				case DataType.UNSUPPORTED_Thread:
					return "thread";
				case DataType.Tuple:
				default:
					throw new LuaRuntimeException(null, "Unexpected LuaType {0}", type);
			}
		}
	}
}
