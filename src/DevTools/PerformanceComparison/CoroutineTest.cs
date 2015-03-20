using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace PerformanceComparison
{
	class CoroutineTest
	{
		public static void xMain()
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


		public static void xxMain()
		{
			string code = @"
				function a()
					callback(b)
				end

				function b()
					coroutine.yield();
				end						

				c = coroutine.create(a);

				return coroutine.resume(c);		
				";

			// Load the code and get the returned function
			Script script = new Script();

			script.Globals["callback"] = DynValue.NewCallback(
				(ctx, args) => args[0].Function.Call()
				);

			DynValue ret = script.DoString(code);

			// false, "attempt to yield from outside a coroutine"
			Console.WriteLine(ret);

			

			Console.ReadKey();
		}






	}
}
