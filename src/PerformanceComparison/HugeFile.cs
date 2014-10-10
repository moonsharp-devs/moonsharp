using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Grammar;

namespace PerformanceComparison
{
	public class HugeFile
	{
		static void xxxxMain(string[] args)
		{
			Console.WriteLine("Started...");
			Script.WarmUp();
			Console.WriteLine("Warmed-up...");

			Script s = new Script(CoreModules.None);
			// s.LoadFile(@"C:\gr\tsg\mod_assets\scripts\alcoves.lua");
			//s.LoadFile(@"C:\temp\test3.lua");

			Stopwatch sw = Stopwatch.StartNew();

			//for (int i = 0; i < 10; i++)
			var v = s.LoadFile(@"C:\temp\test3.lua");

			for(int i = 0; i < 1000; i++)
				s.Call(v);

			sw.Stop();

			Console.WriteLine("Ended : {0} ms", sw.ElapsedMilliseconds);

			Console.WriteLine(s.PerformanceStats.GetPerformanceLog());
			Console.ReadLine();
		}



	}
}
