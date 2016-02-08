using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Commands;
using MoonSharp.Commands.Implementations;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Loaders;
using MoonSharp.Interpreter.REPL;
using MoonSharp.Interpreter.Serialization;
using MoonSharp.RemoteDebugger;
using MoonSharp.RemoteDebugger.Network;

namespace MoonSharp
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			CommandManager.Initialize();

			Script.DefaultOptions.ScriptLoader = new ReplInterpreterScriptLoader();

			Script script = new Script(CoreModules.Preset_Complete);

			script.Globals["makestatic"] = (Func<string, DynValue>)(MakeStatic);

			if (CheckArgs(args, new ShellContext(script)))
				return;

			Banner();

			ReplInterpreter interpreter = new ReplInterpreter(script)
			{
				HandleDynamicExprs = true,
				HandleClassicExprsSyntax = true
			};


			while (true)
			{
				InterpreterLoop(interpreter, new ShellContext(script));
			}
		}

		private static DynValue MakeStatic(string type)
		{
			Type tt = Type.GetType(type);
			if (tt == null)
				Console.WriteLine("Type '{0}' not found.", type);
			else
				return UserData.CreateStatic(tt);

			return DynValue.Nil;
		}

		private static void InterpreterLoop(ReplInterpreter interpreter, ShellContext shellContext)
		{
			Console.Write(interpreter.ClassicPrompt + " ");

			string s = Console.ReadLine();

			if (!interpreter.HasPendingCommand && s.StartsWith("!"))
			{
				ExecuteCommand(shellContext, s.Substring(1));
				return;
			}

			try
			{
				DynValue result = interpreter.Evaluate(s);

				if (result != null && result.Type != DataType.Void)
					Console.WriteLine("{0}", result);
			}
			catch (InterpreterException ex)
			{
				Console.WriteLine("{0}", ex.DecoratedMessage ?? ex.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine("{0}", ex.Message);
			}
		}

		private static void Banner()
		{
			Console.WriteLine("MoonSharp Console {0} [{1}]", Script.VERSION, Script.GlobalOptions.Platform.GetPlatformName());
			Console.WriteLine("Copyright (C) 2014-2016 Marco Mastropaolo");
			Console.WriteLine("http://www.moonsharp.org");
			Console.WriteLine();
			Console.WriteLine("Type Lua code to execute it or type !help to see help on commands.\n");
			Console.WriteLine("Welcome.\n");
		}


		private static bool CheckArgs(string[] args, ShellContext shellContext)
		{
			if (args.Length == 0)
				return false;

			if (args.Length == 1 && args[0].Length > 0 && args[0][0] != '-')
			{
				Script script = new Script();
				script.DoFile(args[0]);
			}

			if (args[0] == "-H" || args[0] == "--help" || args[0] == "/?" || args[0] == "-?")
			{
				ShowCmdLineHelpBig();
			}
			else if (args[0] == "-X")
			{
				if (args.Length == 2)
				{
					ExecuteCommand(shellContext, args[1]);
				}
				else
				{
					Console.WriteLine("Wrong syntax.");
					ShowCmdLineHelp();
				}
			}
			else if (args[0] == "-W")
			{
				bool internals = false;
				string dumpfile = null;
				string destfile = null;
				string classname = null;
				string namespacename = null;
				bool useVb = false;
				bool fail = true;

				for (int i = 1; i < args.Length; i++)
				{
					if (args[i] == "--internals")
						internals = true;
					else if (args[i] == "--vb")
						useVb = true;
					else if (args[i].StartsWith("--class:"))
						classname = args[i].Substring("--class:".Length);
					else if (args[i].StartsWith("--namespace:"))
						namespacename = args[i].Substring("--namespace:".Length);
					else if (dumpfile == null)
						dumpfile = args[i];
					else if (destfile == null)
					{
						destfile = args[i];
						fail = false;
					}
					else fail = true;
				}

				if (fail)
				{
					Console.WriteLine("Wrong syntax.");
					ShowCmdLineHelp();
				}
				else
				{
					HardWireCommand.Generate(useVb ? "vb" : "cs", dumpfile, destfile, internals, classname, namespacename);
				}
			}

			return true;
		}

		private static void ShowCmdLineHelpBig()
		{
			Console.WriteLine("usage: moonsharp [-H | --help | -X \"command\" | -W <dumpfile> <destfile> [--internals] [--vb] [--class:<name>] [--namespace:<name>] | <script>]");
			Console.WriteLine();
			Console.WriteLine("-H : shows this help");
			Console.WriteLine("-X : executes the specified command");
			Console.WriteLine("-W : creates hardwire descriptors");
			Console.WriteLine();
		}

		private static void ShowCmdLineHelp()
		{
			Console.WriteLine("usage: moonsharp [-H | --help | -X \"command\" | -W <dumpfile> <destfile> [--internals] [--vb] | <script>]");
		}

		private static void ExecuteCommand(ShellContext shellContext, string cmdline)
		{
			StringBuilder cmd = new StringBuilder();
			StringBuilder args = new StringBuilder();
			StringBuilder dest = cmd;

			for (int i = 0; i < cmdline.Length; i++)
			{
				if (dest == cmd && cmdline[i] == ' ')
				{
					dest = args;
					continue;
				}

				dest.Append(cmdline[i]);
			}

			string scmd = cmd.ToString().Trim();
			string sargs = args.ToString().Trim();

			ICommand C = CommandManager.Find(scmd);

			if (C == null)
				Console.WriteLine("Invalid command '{0}'.", scmd);
			else
				C.Execute(shellContext, sargs);
		}







	}
}
