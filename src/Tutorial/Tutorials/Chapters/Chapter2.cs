using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace Tutorials.Chapters
{
	[Tutorial]
	static class Chapter02
	{
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
		end

		return fact(mynumber)";

			Script script = new Script();

			script.Globals["mynumber"] = 7;

			DynValue res = script.DoString(scriptCode);
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

			DynValue res = script.Call(script.Globals["fact"], 4);

			return res.Number;
		}
	}
}
