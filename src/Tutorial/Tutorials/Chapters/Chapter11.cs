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
	static class Chapter11
	{
		static RemoteDebuggerService remoteDebugger;

		static void ActivateRemoteDebugger(Script script)
		{
			if (remoteDebugger == null)
			{
				remoteDebugger = new RemoteDebuggerService();

				// the last boolean is to specify if the script is free to run 
				// after attachment, defaults to false
				remoteDebugger.Attach(script, "Description of the script", false);
			}

			// start the web-browser at the correct url. Replace this or just
			// pass the url to the user in some way.
			Process.Start(remoteDebugger.HttpUrlStringLocalHost);
		}


		[Tutorial]
		static void DebuggerDemo()
		{
			Script script = new Script();

			ActivateRemoteDebugger(script);

			script.DoString(@"

				function accum(n, f)
					if (n == 0) then
						return 1;
					else
						return n * f(n);
					end
				end


				local sum = 0;

				for i = 1, 5 do
					-- let's use a lambda to spice things up
					sum = sum + accum(i, | x | x - 1);
				end
				");

			Console.WriteLine("The script has ended..");

		}


	}
}
