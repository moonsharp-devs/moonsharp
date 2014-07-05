using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule]
	public static class BasicMethods
	{
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
					throw new ScriptRuntimeException(null, "assertion failed!");
				else
					throw new ScriptRuntimeException(null, message.ToPrintString());
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
				GC.Collect();

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
			throw new ScriptRuntimeException(null, message.String);
		}


	}
}
