using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace PerformanceComparison
{
	class Sample
	{

		// This prints :
		//     3
		//     hello world
		//     3
		//     hello world
		//     3
		//     hello world
		//     3
		//     hello world
		//     Done
		public static void xxMain()
		{
			string code = @"
				x = 3

				function onThis()
					print(x)
					x = 'hello'
				end

				function onThat()
					print(x .. ' world')
					x = 3
				end						
				";

			// Load the code 
			Script script = new Script();
			script.DoString(code);

			var onThis = script.Globals.Get("onThis").Function.GetDelegate();
			var onThat = script.Globals.Get("onThat").Function.GetDelegate();

			for (int i = 0; i < 4; i++)
			{
				onThis();
				onThat();
			}

			Console.WriteLine("Done");
			Console.ReadKey();
		}
	}
}
