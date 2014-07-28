using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule]
	public class BasicMethods
	{
		//type (v)
		//----------------------------------------------------------------------------------------------------------------
		//Returns the type of its only argument, coded as a string. The possible results of this function are "nil" 
		//(a string, not the value nil), "number", "string", "boolean", "table", "function", "thread", and "userdata". 
		[MoonSharpMethod]
		public static DynValue type(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args[0];
			return DynValue.NewString(v.Type.ToLuaTypeString());
		}



		//assert (v [, message])
		//----------------------------------------------------------------------------------------------------------------
		//Issues an error when the value of its argument v is false (i.e., nil or false); 
		//otherwise, returns all its arguments. message is an error message; when absent, it defaults to "assertion failed!" 
		[MoonSharpMethod]
		public static DynValue assert(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args[0];
			DynValue message = args[1];

			if (!v.CastToBool())
			{
				if (message.IsNil())
					throw new ScriptRuntimeException("assertion failed!");
				else
					throw new ScriptRuntimeException(message.ToPrintString());
			}

			return DynValue.Nil;
		}

		// collectgarbage  ([opt [, arg]])
		// ----------------------------------------------------------------------------------------------------------------
		// This function is mostly a stub towards the CLR GC. If mode is nil, "collect" or "restart", a GC is forced.
		[MoonSharpMethod]
		public static DynValue collectgarbage(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue opt = args[0];

			string mode = opt.CastToString();

			if (mode == null || mode == "collect" || mode == "restart")
				GC.Collect(2, GCCollectionMode.Forced);

			return DynValue.Nil;
		}

		// error (message [, level])
		// ----------------------------------------------------------------------------------------------------------------
		// Terminates the last protected function called and returns message as the error message. Function error never returns.
		// Usually, error adds some information about the error position at the beginning of the message. 
		// The level argument specifies how to get the error position. 
		// With level 1 (the default), the error position is where the error function was called. 
		// Level 2 points the error to where the function that called error was called; and so on. 
		// Passing a level 0 avoids the addition of error position information to the message. 
		[MoonSharpMethod]
		public static DynValue error(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue message = args.AsType(0, "dofile", DataType.String, false);
			throw new ScriptRuntimeException(message.String);
		}


		// tostring (v)
		// ----------------------------------------------------------------------------------------------------------------
		// Receives a value of any type and converts it to a string in a reasonable format. (For complete control of how 
		// numbers are converted, use string.format.)
		// 
		// If the metatable of v has a "__tostring" field, then tostring calls the corresponding value with v as argument, 
		// and uses the result of the call as its result. 
		[MoonSharpMethod]
		public static DynValue tostring(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args[0];
			DynValue tail = executionContext.GetMetamethodTailCall(v, "__tostring", v);
			
			if (tail == null || tail.IsNil())
				return DynValue.NewString(v.ToPrintString());

			tail.TailCallData.Continuation = new CallbackFunction(__tostring_continuation);

			return tail;
		}

		private static DynValue __tostring_continuation(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue b = args[0].ToScalar();

			if (b.IsNil())
				return b;

			if (b.Type != DataType.String)
				throw new ScriptRuntimeException("'tostring' must return a string");


			return b;
		}


		// tonumber (e [, base])
		// ----------------------------------------------------------------------------------------------------------------
		// When called with no base, tonumber tries to convert its argument to a number. If the argument is already 
		// a number or a string convertible to a number (see §3.4.2), then tonumber returns this number; otherwise, 
		// it returns nil.
		//
		// When called with base, then e should be a string to be interpreted as an integer numeral in that base. 
		// The base may be any integer between 2 and 36, inclusive. In bases above 10, the letter 'A' (in either 
		// upper or lower case) represents 10, 'B' represents 11, and so forth, with 'Z' representing 35. If the 
		// string e is not a valid numeral in the given base, the function returns nil. 
		[MoonSharpMethod]
		public static DynValue tonumber(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue e = args[0];
			DynValue b = args.AsType(1, "tonumber", DataType.Number, true);

			if (b.IsNil())
			{
				if (e.Type == DataType.Number)
					return e;

				if (e.Type != DataType.String)
					return DynValue.Nil;

				double d;
				if (double.TryParse(e.String, NumberStyles.Any, CultureInfo.InvariantCulture, out d))
				{
					return DynValue.NewNumber(d);
				}
				return DynValue.Nil;
			}
			else
			{
				//!COMPAT: tonumber supports only 2,8,10 or 16 as base
				DynValue ee = args.AsType(0, "tonumber", DataType.String, false);
				int bb = (int)b.Number;

				uint uiv = Convert.ToUInt32(ee.String, bb);

				return DynValue.NewNumber(uiv);
			}
		}

		[MoonSharpMethod]
		public static DynValue print(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < args.Count; i++)
			{
				if (i != 0)
					sb.Append('\t');

				if ((args[i].Type == DataType.Table) && (args[i].Table.MetaTable != null) &&
					(args[i].Table.MetaTable.RawGet("__tostring") != null))
				{
					var v = executionContext.GetScript().Call(args[i].Table.MetaTable.RawGet("__tostring"), args[i]);

					if (v.Type != DataType.String)
						throw new ScriptRuntimeException("'tostring' must return a string to 'print'");

					sb.Append(v.ToPrintString());
				}
				else
				{
					sb.Append(args[i].ToPrintString());
				}
			}

			executionContext.GetScript().DebugPrint(sb.ToString());

			return DynValue.Nil;
		}
	}
}
