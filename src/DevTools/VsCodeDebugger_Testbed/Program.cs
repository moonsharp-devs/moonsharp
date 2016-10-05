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
		const int DEFAULT_PORT = 41912;

		public static void Main(string[] argv)
		{
			Script script = new Script();
			MoonSharpVsCodeDebugServer server = new MoonSharpVsCodeDebugServer(script, DEFAULT_PORT);

			script.AttachDebugger(server.GetDebugger());

			script.DoFile(@"R:/temp/lua/fact.lua");

			Closure func = script.Globals.Get("run").Function;

			server.Start();

			Console.WriteLine("READY.");

			while (true)//(Console.ReadKey().Key != ConsoleKey.Escape)
			{
				try
				{
					var val = func.Call(5);
					Console.ForegroundColor = ConsoleColor.Magenta;
					Console.WriteLine(val.Number);
					System.Threading.Thread.Sleep(5000);
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
