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

namespace MoonSharpTests
{
	class Program
	{
		public const string RESTRICT_TEST = null;

		static void Main(string[] args)
		{
			TestRunner T = new TestRunner(Log);

			T.Test(RESTRICT_TEST);

			if (Debugger.IsAttached)
			{
				Console.WriteLine("Press any key...");
				Console.ReadKey();
			}
		}

		private static void Log(TestResult r)
		{
			if (r.Type == TestResultType.Fail)
			{
				Console.ForegroundColor = ConsoleColor.Red;

				if (r.Exception!= null)
					Console.WriteLine("{0} - {1}", r.TestName, r.Exception);
				else
					Console.WriteLine("{0} - {1}", r.TestName, r.Message);
			}
			else if (r.Type == TestResultType.Ok)
			{
				Console.ForegroundColor = ConsoleColor.DarkGreen;
				Console.WriteLine("{0} - {1}", r.TestName, r.Message);
			}
			else if (r.Type == TestResultType.Skipped)
			{
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine("{0} - {1}", r.TestName, r.Message);
			}
			else
			{
				Console.ForegroundColor = ConsoleColor.Gray;
				Console.WriteLine("{0}", r.Message);
			}
		}


	}
}
