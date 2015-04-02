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
	static class Chapter09
	{
		[Tutorial]
		static void ChangePlatform()
		{
			// This prints "function"
			Console.WriteLine(Script.RunString("return type(os.exit);").ToPrintString());

			// Save the old platform
			var oldplatform = Script.GlobalOptions.Platform;

			// Changing platform after a script has been created is not recommended.. do not do it.
			// We are doing it for the purpose of the walkthrough..
			Script.GlobalOptions.Platform = new LimitedPlatformAccessor();

			// This time, this prints "nil"
			Console.WriteLine(Script.RunString("return type(os.exit);").ToPrintString());

			// Restore the old platform
			Script.GlobalOptions.Platform = oldplatform;
		}


	}
}
