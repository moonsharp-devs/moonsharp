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
		static DynValue Print(IExecutionContext executionContext, CallbackArguments values)
		{
			string prn = string.Join(" ", values.List.Select(v => v.ToPrintString()).ToArray());
			Console.WriteLine("{0}", prn);
			return DynValue.Nil;
		}

		static DynValue Read(IExecutionContext executionContext, CallbackArguments values)
		{
			double d = double.Parse(Console.ReadLine());
			return DynValue.NewNumber(d);
		}

		static StringBuilder g_TreeDump = new StringBuilder();

		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine("Moon# {0}\nCopyright (C) 2014 Marco Mastropaolo\nhttp://www.moonsharp.org",
				Assembly.GetExecutingAssembly().GetName().Version);

			Console.WriteLine("Based on Lua 5.1 - 5.3, Copyright (C) 1994-2014 Lua.org");

			Console.WriteLine();

			if (args.Length == 1)
			{
				Script script = new Script();

				script.Globals["print"] = DynValue.NewCallback(new CallbackFunction(Print));

				script.DoFile(args[0]);

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
