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
			try
			{
				Script script = new Script();
				MoonSharpVsCodeDebugServer server = new MoonSharpVsCodeDebugServer(script, DEFAULT_PORT);

				script.AttachDebugger(server.GetDebugger());

				script.DoFile(@"C:\Users\mmastropaolo\Desktop\PRJ\script1.lua");

				Closure func = script.Globals.Get("fact").Function;

				server.Start();

				Console.WriteLine("READY.");

				while (Console.ReadKey().Key != ConsoleKey.Escape)
				{
					var val = func.Call(5);
					Console.ForegroundColor = ConsoleColor.Magenta;
					Console.WriteLine(val.Number);
				}
			}
			catch (InterpreterException ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Write(ex.DecoratedMessage);
				Console.ReadLine();
			}
		}



	}
}
