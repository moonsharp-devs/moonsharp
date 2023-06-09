using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;
using MoonSharp.VsCodeDebugger.SDK;

namespace VsCodeDebugger_Testbed
{
	class Program
	{
		public static void Main(string[] argv)
		{
			MoonSharpVsCodeDebugServer server = new MoonSharpVsCodeDebugServer();
			server.Logger = s => Console.WriteLine(s);
			server.Start();

			Script script1 = new Script();
			script1.DoFile(@"Z:/HDD/temp/lua/fact.lua");
			server.AttachToScript(script1, "Script #1");
			Closure func1 = script1.Globals.Get("run").Function;

			Script script2 = new Script();
			script2.DoFile(@"Z:/HDD/temp/lua/fact2.lua");
			server.AttachToScript(script2, "Script #2");
			Closure func2 = script2.Globals.Get("run").Function;

			Console.WriteLine("READY.");
			int i = 0;

			server.Current = null;

			while (true)//(Console.ReadKey().Key != ConsoleKey.Escape)
			{
				if (Console.KeyAvailable)
				{
					Console.ReadKey();
					server.Detach(script2);
					Console.WriteLine("Detached");
				}

				Closure func = ((++i) % 2) == 0 ? func1 : func2;

				try
				{
					var val = func.Call(5);
					Console.ForegroundColor = ConsoleColor.Magenta;
					Console.WriteLine(val.Number);
					System.Threading.Thread.Sleep(1000);
				}
				catch (InterpreterException ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write(ex.DecoratedMessage);
				}
			}
		}
	}
}
