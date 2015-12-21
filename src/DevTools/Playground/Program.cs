using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.RemoteDebugger;

namespace Playground
{
	
	class Program
	{
		static void Main(string[] args)
		{
			Script S = new Script(CoreModules.Basic);

			RemoteDebuggerService remoteDebugger;

			remoteDebugger = new RemoteDebuggerService();
		
			remoteDebugger.Attach(S, "MyScript", false);

			Process.Start(remoteDebugger.HttpUrlStringLocalHost);
	
			S.DoString(@"

local hi = 'hello'

local function test()
    print(hi)
end

test();

hi = 'X'

test();

local hi = '!';

test();




");

			Console.WriteLine("DONE");

			Console.ReadKey();
		}



	}
}
