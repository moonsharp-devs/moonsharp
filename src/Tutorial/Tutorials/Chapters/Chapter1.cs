using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace Tutorials.Chapters
{
	[Tutorial]
	static class Chapter01
	{
		[Tutorial]
		public static double MoonSharpFactorial()
		{
			string script = @"    
				-- defines a factorial function
				function fact (n)
					if (n == 0) then
						return 1
					else
						return n*fact(n - 1)
					end
				end

				return fact(5)";

			DynValue res = Script.RunString(script);
			return res.Number;
		}



	}
}
