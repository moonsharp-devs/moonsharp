using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using MoonSharp.Interpreter.Platforms;
using MoonSharp.RemoteDebugger;

namespace Tutorials.Chapters
{
	[Tutorial]
	static class Chapter12
	{
		[Tutorial]
		static void CoroutinesFromCSharp()
		{
			string code = @"
				return function()
					local x = 0
					while true do
						x = x + 1
						coroutine.yield(x)
					end
				end
				";

			// Load the code and get the returned function
			Script script = new Script();
			DynValue function = script.DoString(code);

			// Create the coroutine in C#
			DynValue coroutine = script.CreateCoroutine(function);

			// Resume the coroutine forever and ever.. 
			while (true)
			{
				DynValue x = coroutine.Coroutine.Resume();
				Console.WriteLine("{0}", x);
			}
		}

		[Tutorial]
		static void CoroutinesAsCSharpIterator()
		{
			string code = @"
				return function()
					local x = 0
					while true do
						x = x + 1
						coroutine.yield(x)
						if (x > 5) then
							return 7
						end
					end
				end
				";

			// Load the code and get the returned function
			Script script = new Script();
			DynValue function = script.DoString(code);

			// Create the coroutine in C#
			DynValue coroutine = script.CreateCoroutine(function);

			// Loop the coroutine 
			string ret = "";

			foreach (DynValue x in coroutine.Coroutine.AsTypedEnumerable())
			{
				ret = ret + x.ToString();
			}

			Console.WriteLine(ret);
		}

	}
}
