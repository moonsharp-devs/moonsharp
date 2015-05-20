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


		static int Main(string[] args)
		{
			try
			{
				TestRunner T = new TestRunner(Log);

				if (LOG_ON_FILE != null)
					File.WriteAllText(LOG_ON_FILE, "");

				Console_WriteLine("Running on AOT : {0}", Script.GlobalOptions.Platform.IsRunningOnAOT());

				T.Test(RESTRICT_TEST);

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

				if (r.Exception!= null)
					Console_WriteLine("{0} - {1}", r.TestName, r.Exception);
				else
					Console_WriteLine("{0} - {1}", r.TestName, r.Message);
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
