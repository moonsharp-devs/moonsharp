using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp
{
	class Program
	{
		static RValue Print(RValue[] values)
		{
			string prn = string.Join(" ", values.Select(v => v.AsString()).ToArray());
			Console.WriteLine("{0}", prn);
			return RValue.Nil;
		}

		static RValue Read(RValue[] values)
		{
			double d = double.Parse(Console.ReadLine());
			return new RValue(d);
		}

		static StringBuilder g_TreeDump = new StringBuilder();

		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine("Moon# {0}\nCopyright (C) 2014 Marco Mastropaolo\nhttp://www.moonsharp.org",
				Assembly.GetExecutingAssembly().GetName().Version);

			Console.WriteLine("Based on Lua 5.2, Copyright (C) 1994-2013 Lua.org");

			Console.WriteLine();

			if (args.Length == 1)
			{
				Table globalTable = new Table();
				globalTable[new RValue("print")] = new RValue(new CallbackFunction(Print));

				var script = MoonSharpInterpreter.LoadFromFile(args[0]);

				script.Execute(globalTable);

				Console.WriteLine("Done.");
				
				if (System.Diagnostics.Debugger.IsAttached)
					Console.ReadKey();
			}
			else
			{
				Console.WriteLine("Sorry, at the moment, only file operations are supported:");
				Console.WriteLine("\tUsage : MoonSharp [filename]");
			}
		}


	}
}
