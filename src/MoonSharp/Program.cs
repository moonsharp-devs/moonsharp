using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
		static StringBuilder g_TreeDump = new StringBuilder();

		[STAThread]
		static void Main(string[] args)
		{
			Script.DefaultOptions.ScriptLoader = new ReplInterpreterScriptLoader();

			Console.WriteLine("MoonSharp Console {0} [{1}]", Script.VERSION, Script.GlobalOptions.Platform.GetPlatformName());
			Console.WriteLine("Copyright (C) 2014-2016 Marco Mastropaolo");
			Console.WriteLine("http://www.moonsharp.org");
			Console.WriteLine();


			if (args.Length == 1)
			{
				Script script = new Script();

				script.DoFile(args[0]);

				Console.WriteLine("Done.");

				if (System.Diagnostics.Debugger.IsAttached)
					Console.ReadKey();
			}
			else
			{
				Console.WriteLine("Type Lua code to execute it or type !help to see help on commands.\n");
				Console.WriteLine("Welcome.\n");

				Script script = new Script(CoreModules.Preset_Complete);

				ReplInterpreter interpreter = new ReplInterpreter(script)
				{
					HandleDynamicExprs = true,
					HandleClassicExprsSyntax = true
				};

				while (true)
				{
					Console.Write(interpreter.ClassicPrompt + " ");

					string s = Console.ReadLine();

					if (!interpreter.HasPendingCommand && s.StartsWith("!"))
					{
						ParseCommand(script, s.Substring(1));
						continue;
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
			}
		}

		static RemoteDebuggerService m_Debugger;

		private static void ParseCommand(Script S, string p)
		{
			if (p == "help")
			{
				Console.WriteLine("Type Lua code to execute Lua code, multilines are accepted, ");
				Console.WriteLine("or type one of the following commands to execute them.");
				Console.WriteLine("");
				Console.WriteLine("Commands:");
				Console.WriteLine("");
				Console.WriteLine("	!exit - Exits the interpreter");
				Console.WriteLine("	!debug - Starts the debugger");
				Console.WriteLine("	!run <filename> - Executes the specified Lua script");
				Console.WriteLine("	!compile <filename> - Compiles the file in a binary format");
				Console.WriteLine("");
			}
			else if (p == "exit")
			{
				Environment.Exit(0);
			}
			else if (p == "debug" && m_Debugger == null)
			{
				m_Debugger = new RemoteDebuggerService();
				m_Debugger.Attach(S, "MoonSharp REPL interpreter", false);
				Process.Start(m_Debugger.HttpUrlStringLocalHost);
			}
			else if (p.StartsWith("run"))
			{
				p = p.Substring(3).Trim();
				S.DoFile(p);
			}
			else if (p == "!")
			{
				ParseCommand(S, "debug");
				ParseCommand(S, @"run c:\temp\test.lua");
			}
			else if (p.StartsWith("compile"))
			{
				p = p.Substring("compile".Length).Trim();

				string targetFileName = p + "-compiled";

				DynValue chunk = S.LoadFile(p);

				using (Stream stream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write))
					S.Dump(chunk, stream);
			}
		}

		static void m_Server_DataReceivedAny(object sender, Utf8TcpPeerEventArgs e)
		{
			Console.WriteLine("RCVD: {0}", e.Message);
			e.Peer.Send(e.Message.ToUpper());
		}


	}
}
