using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace Tutorials.Chapters
{
	[Tutorial]
	static class Chapter03
	{
		[Tutorial]
		public static double MoonSharpFactorial()
		{
			string scriptCode = @"    
		-- defines a factorial function
		function fact (n)
			if (n == 0) then
				return 1
			else
				return n*fact(n - 1)
			end
		end";

			Script script = new Script();

			script.DoString(scriptCode);

			DynValue luaFactFunction = script.Globals.Get("fact");

			DynValue res = script.Call(luaFactFunction, 4);

			return res.Number;
		}

		[Tutorial]
		public static double MoonSharpFactorial2()
		{
			string scriptCode = @"    
		-- defines a factorial function
		function fact (n)
			if (n == 0) then
				return 1
			else
				return n*fact(n - 1)
			end
		end";

			Script script = new Script();

			script.DoString(scriptCode);

			DynValue luaFactFunction = script.Globals.Get("fact");

			DynValue res = script.Call(luaFactFunction, DynValue.NewNumber(4));

			return res.Number;
		}

	}
}
