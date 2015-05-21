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

namespace MoonSharpTests
{
	class Program
	{
		public const string RESTRICT_TEST = null; //"Interop_StaticInstanceAccessRaisesError";
		public const string LOG_ON_FILE = "moonsharp_tests.log";

		// Tests skipped on all platforms
		static List<string> SKIPLIST = new List<string>()
		{
			"TestMore_308_io",	// avoid interactions with low level system
			"TestMore_309_os",  // avoid interactions with low level system
		};

		// Tests skipped on AOT platforms - known not workings :(
		static List<string> AOT_SKIPLIST = new List<string>()
		{
			"RegCollGen_List_ExtMeth_Last", 
			"VInterop_NIntPropertySetter_None",	
			"VInterop_NIntPropertySetter_Lazy",	
			"VInterop_NIntPropertySetter_Precomputed",	
			"VInterop_Overloads_NumDowncast",	
			"VInterop_Overloads_NilSelectsNonOptional",	
			"VInterop_Overloads_FullDecl",
			"VInterop_Overloads_Static2",
			"VInterop_Overloads_Cache1",
			"VInterop_Overloads_Cache2",
			"VInterop_ConcatMethod_None",
			"VInterop_ConcatMethod_Lazy",
			"VInterop_ConcatMethod_Precomputed",
			"VInterop_ConcatMethodSemicolon_None",
			"VInterop_ConcatMethodSemicolon_Lazy",
			"VInterop_ConcatMethodSemicolon_Precomputed",
			"VInterop_ConstructorAndConcatMethodSemicolon_None",
			"VInterop_ConstructorAndConcatMethodSemicolon_Lazy",
			"VInterop_ConstructorAndConcatMethodSemicolon_Precomputed",
		};



		static int Main(string[] args)
		{
			Console.ForegroundColor = ConsoleColor.Magenta;

			Console.WriteLine("====================================================================================");
			Console.WriteLine("====================================================================================");
			Console.WriteLine("====================================================================================");
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			
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

				return T.Fail;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return 999;
			}
		}

		private static void Log(TestResult r)
		{
			if (r.Type == TestResultType.Fail)
			{
				Console.ForegroundColor = ConsoleColor.Red;

				Console.WriteLine();
				if (r.Exception!= null)
					Console_WriteLine("{0} - {1}", r.TestName, r.Exception);
				else
					Console_WriteLine("{0} - {1}", r.TestName, r.Message);
				Console.WriteLine();
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
