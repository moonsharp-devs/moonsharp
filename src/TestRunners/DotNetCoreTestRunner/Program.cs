// Disable warning 429 (Unreachable code) because of the RESTRICT_TEST condition below.
#pragma warning disable 429

using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.Tests;
using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization;
using MoonSharp.VsCodeDebugger;

namespace DotNetCoreTestRunner
{
	class Program
	{
		public const string RESTRICT_TEST = null; //"VInterop_ConstructorAndConcatMethodSemicolon_None";
		public const string LOG_ON_FILE = "moonsharp_tests.log";

		// Tests skipped on all platforms
		static List<string> SKIPLIST = new List<string>()
		{
			"TestMore_308_io",	// avoid interactions with low level system
			"TestMore_309_os",  // avoid interactions with low level system
		};

		static List<string> HARDWIRE_SKIPLIST = new List<string>()
		{
			// events
			"Interop_Event_Simple",
			"Interop_Event_TwoObjects",
			"Interop_Event_Multi",
			"Interop_Event_MultiAndDetach",
			"Interop_Event_DetachAndDeregister",
			"Interop_SEvent_DetachAndDeregister",
			"Interop_SEvent_DetachAndReregister",

			// tests dependent on type dereg
			"Interop_ListMethod_None",
			"Interop_ListMethod_Lazy",
			"Interop_ListMethod_Precomputed",
			"VInterop_ListMethod_None",
			"VInterop_ListMethod_Lazy",
			"VInterop_ListMethod_Precomputed",

			// private members
			"Interop_NestedTypes_Private_Ref",
			"Interop_NestedTypes_Private_Val",

			// value type property setters
			"VInterop_IntPropertySetter_None",
			"VInterop_IntPropertySetter_Lazy",
			"VInterop_IntPropertySetter_Precomputed",
			"VInterop_NIntPropertySetter_None",
			"VInterop_NIntPropertySetter_Lazy",
			"VInterop_NIntPropertySetter_Precomputed",
			"VInterop_WoIntPropertySetter_None",
			"VInterop_WoIntPropertySetter_Lazy",
			"VInterop_WoIntPropertySetter_Precomputed",
			"VInterop_WoIntProperty2Setter_None",
			"VInterop_WoIntProperty2Setter_Lazy",
			"VInterop_WoIntProperty2Setter_Precomputed",
			"VInterop_IntPropertySetterWithSimplifiedSyntax",
			"VInterop_IntFieldSetter_None",
			"VInterop_IntFieldSetter_Lazy",
			"VInterop_IntFieldSetter_Precomputed",
			"VInterop_NIntFieldSetter_None",
			"VInterop_NIntFieldSetter_Lazy",
			"VInterop_NIntFieldSetter_Precomputed",
			"VInterop_IntFieldSetterWithSimplifiedSyntax",
		};


		// Tests skipped on AOT platforms - known not workings :(
		static List<string> AOT_SKIPLIST = new List<string>()
		{
			//"RegCollGen_List_ExtMeth_Last", 
			//"VInterop_NIntPropertySetter_None",	
			//"VInterop_NIntPropertySetter_Lazy",	
			//"VInterop_NIntPropertySetter_Precomputed",	
			//"VInterop_Overloads_NumDowncast",	
			//"VInterop_Overloads_NilSelectsNonOptional",	
			//"VInterop_Overloads_FullDecl",
			//"VInterop_Overloads_Static2",
			//"VInterop_Overloads_Cache1",
			//"VInterop_Overloads_Cache2",
			//"VInterop_ConcatMethod_None",
			//"VInterop_ConcatMethod_Lazy",
			//"VInterop_ConcatMethod_Precomputed",
			//"VInterop_ConcatMethodSemicolon_None",
			//"VInterop_ConcatMethodSemicolon_Lazy",
			//"VInterop_ConcatMethodSemicolon_Precomputed",
			//"VInterop_ConstructorAndConcatMethodSemicolon_None",
			//"VInterop_ConstructorAndConcatMethodSemicolon_Lazy",
			//"VInterop_ConstructorAndConcatMethodSemicolon_Precomputed",
		};

		static int Main(string[] args)
		{
			Console.WriteLine("1 - Unit tests");
			Console.WriteLine("2 - Debugger");

			while (true)
			{
				Console.Write(" ? ");
				var key = Console.ReadKey();
				if (key.Key == ConsoleKey.D1)
					TestMain(args);
				else if (key.Key == ConsoleKey.D2)
					DebuggerMain(args);
			}
		}

		private static void DebuggerMain(string[] args)
		{
			MoonSharpVsCodeDebugServer server = new MoonSharpVsCodeDebugServer().Start();
			Script s = new Script();

			server.AttachToScript(s, "xxx");

			DynValue func = s.DoString("return function()\nprint 'x';\nend;");

			while (!Console.KeyAvailable)
			{
				func.Function.Call();
				System.Threading.Tasks.Task.Delay(100).Wait();
			}

			Console.ReadKey();
		}

		static int TestMain(string[] args)
		{
			Console.ForegroundColor = ConsoleColor.Magenta;

			Console.WriteLine("====================================================================================");
			Console.WriteLine("====================================================================================");
			Console.WriteLine("====================================================================================");
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();


			//MyNamespace.MyClass.Initialize();
			//SKIPLIST.AddRange(HARDWIRE_SKIPLIST);
			//UserData.RegistrationPolicy = new HardwireAndLogPolicy();


			try
			{
				TestRunner T = new TestRunner(Log);

				if (LOG_ON_FILE != null)
					File.WriteAllText(LOG_ON_FILE, "");

				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine("Running on AOT : {0}", Script.GlobalOptions.Platform.IsRunningOnAOT());


				if (Script.GlobalOptions.Platform.IsRunningOnAOT())
				{
					SKIPLIST.AddRange(AOT_SKIPLIST);
				}

				Console.WriteLine();
				Console.WriteLine();

				T.Test(RESTRICT_TEST, SKIPLIST.ToArray());

				if (Debugger.IsAttached)
				{
					Console.WriteLine("Press any key...");
					Console.ReadKey();
				}

				//OnTestEnded();
				Console.ReadKey();

				return T.Fail;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				Console.ReadKey();
				return 999;
			}

		}

		private static void OnTestEnded()
		{
			Table dump = UserData.GetDescriptionOfRegisteredTypes(true);

			string str = dump.Serialize();

			File.WriteAllText(@"c:\temp\testdump.lua", str);
		}

		private static void Log(TestResult r)
		{
			if (r.Type == TestResultType.Fail)
			{
				Console.ForegroundColor = ConsoleColor.Red;

				Console.WriteLine();
				if (r.Exception != null)
					Console_WriteLine("{0} - {1}", r.TestName, r.Exception);
				else
					Console_WriteLine("{0} - {1}", r.TestName, r.Message);
				Console.WriteLine();

				HARDWIRE_SKIPLIST.Add(r.TestName);
			}
			else if (r.Type == TestResultType.Ok)
			{
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console_WriteLine("{0} - {1}", r.TestName, r.Message);
			}
			else if (r.Type == TestResultType.Skipped)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console_WriteLine("{0} - {1}", r.TestName, r.Message);
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Gray;
				Console_WriteLine("{0}", r.Message);
			}
		}

		private static void Console_WriteLine(string format, params object[] args)
		{
			string txt = string.Format(format, args);

			Console.WriteLine(txt);

			if (LOG_ON_FILE != null)
			{
				File.AppendAllText(LOG_ON_FILE, txt);
				File.AppendAllText(LOG_ON_FILE, "\n\n");
			}
		}


	}
}
