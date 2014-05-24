using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	public static class MetaTableMethods
	{
		public static RValue setmetatable(RValue[] values) { return RValue.Nil; }
		public static RValue getmetatable(RValue[] values) { return RValue.Nil; }
		public static RValue rawget(RValue[] values) { return RValue.Nil; }
		public static RValue rawset(RValue[] values) { return RValue.Nil; }
		public static RValue rawequal(RValue[] values) { return RValue.Nil; }
		public static RValue rawlen(RValue[] values) { return RValue.Nil; }



	}
}
