using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	public static class BasicMethods
	{
		// ALSO TO SUPPORT:
		// _G, _VERSION, _MOONSHARP

		public static RValue assert(RValue[] values) { return RValue.Nil; }
		public static RValue collectgarbage(RValue[] values) { return RValue.Nil; }
		public static RValue error(RValue[] values) { return RValue.Nil; }
		public static RValue ipairs(RValue[] values) { return RValue.Nil; }
		public static RValue pairs(RValue[] values) { return RValue.Nil; }
		public static RValue next(RValue[] values) { return RValue.Nil; }
		public static RValue pcall(RValue[] values) { return RValue.Nil; }
		public static RValue xpcall(RValue[] values) { return RValue.Nil; }
		public static RValue print(RValue[] values) { return RValue.Nil; }
		public static RValue select(RValue[] values) { return RValue.Nil; }
		public static RValue tonumber(RValue[] values) { return RValue.Nil; }
		public static RValue tostring(RValue[] values) { return RValue.Nil; }
		public static RValue type(RValue[] values) { return RValue.Nil; }




		// Unsupported (?) - will raise exceptions:
		public static RValue load(RValue[] values) { return RValue.Nil; }
		public static RValue loadfile(RValue[] values) { return RValue.Nil; }
		public static RValue dofile(RValue[] values) { return RValue.Nil; }

	}
}
