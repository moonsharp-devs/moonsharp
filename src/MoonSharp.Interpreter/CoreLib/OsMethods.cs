using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
		[MoonSharpModule(Namespace = "os")]
	public  class OsMethods
	{
		static DateTime Time0 = DateTime.UtcNow;
		static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		[MoonSharpMethod]
		public static DynValue clock(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return DynValue.NewNumber((DateTime.UtcNow - Time0).TotalSeconds);
		}

		[MoonSharpMethod]
		public static DynValue time(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DateTime date = DateTime.UtcNow;

			if (args.Count > 0)
			{


			}


			return DynValue.NewNumber(Math.Floor(( date - Epoch).TotalSeconds));


		}
		
	}
}
