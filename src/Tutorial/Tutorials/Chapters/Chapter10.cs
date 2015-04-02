using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using MoonSharp.Interpreter.Platforms;

namespace Tutorials.Chapters
{
	[Tutorial]
	static class Chapter10
	{
		[Tutorial]
		static void OverriddenPrint()
		{
			// redefine print to print in lowercase, for all new scripts
			Script.DefaultOptions.DebugPrint = s => Console.WriteLine(s.ToLower());
				
			Script script = new Script();

			DynValue fn = script.LoadString("print 'Hello, World!'");

			fn.Function.Call(); // this prints "hello, world!"

			// redefine print to print in UPPERCASE, for this script only
			script.Options.DebugPrint = s => Console.WriteLine(s.ToUpper());
			
			fn.Function.Call(); // this prints "HELLO, WORLD!"
		}


	}
}
