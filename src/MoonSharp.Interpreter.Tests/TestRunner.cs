using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.RuntimeAbstraction;
using NUnit.Framework;

namespace MoonSharp.Interpreter.Tests
{
	public enum TestResultType
	{
		Message,
		Ok,
		Fail,
		Skipped
	}

	public class TestResult
	{
		public string TestName;
		public string Message;
		public Exception Exception;
		public TestResultType Type;
	}

	internal class SkipThisTestException : Exception { }

	public class TestRunner
	{
		Action<TestResult> loggerAction;

		public TestRunner(Action<TestResult> loggerAction)
		{
			this.loggerAction = loggerAction;

			Console_WriteLine("MoonSharp Test Suite Runner - {0} [{1}]", Script.VERSION, Platform.Current.Name);
			Console_WriteLine("http://www.moonsharp.org");
			Console_WriteLine("");
		}

		public void Test(string whichTest = null)
		{
			int ok = 0;
			int fail = 0;
			int total = 0;
			int skipped = 0;

			Assembly asm = Assembly.GetAssembly(typeof(SimpleTests));

			foreach (Type t in asm.GetTypes().Where(t => t.GetCustomAttributes(typeof(TestFixtureAttribute), true).Any()))
			{
				foreach (MethodInfo mi in t.GetMethods().Where(m => m.GetCustomAttributes(typeof(TestAttribute), true).Any()))
				{
					if (whichTest != null && mi.Name != whichTest)
						continue;

					TestResult tr = RunTest(t, mi);

					if (tr.Type != TestResultType.Message)
					{
						if (tr.Type == TestResultType.Fail)
							++fail;
						else if (tr.Type == TestResultType.Ok)
							++ok;
						else
							++skipped;

						++total;
					}

					loggerAction(tr);
				}
			}

			Console_WriteLine("");
			Console_WriteLine("OK : {0}/{2}, Failed {1}/{2}, Skipped {3}/{2}", ok, fail, total, skipped);
		}

		private void Console_WriteLine(string message, params object[] args)
		{
			loggerAction(new TestResult()
			{
				Type = TestResultType.Message,
				Message = string.Format(message, args)
			});
		}

		private static TestResult RunTest(Type t, MethodInfo mi)
		{
			if (mi.GetCustomAttributes(typeof(IgnoreAttribute), true).Any())
			{
				return new TestResult()
				{
					TestName = mi.Name,
					Message = "skipped",
					Type = TestResultType.Skipped
				};
			}

			ExpectedExceptionAttribute expectedEx = mi.GetCustomAttributes(typeof(ExpectedExceptionAttribute), true)
				.OfType<ExpectedExceptionAttribute>()
				.FirstOrDefault();


			try
			{
				object o = Activator.CreateInstance(t);
				mi.Invoke(o, new object[0]);

				if (expectedEx != null)
				{
					return new TestResult()
					{
						TestName = mi.Name,
						Message = string.Format("Exception {0} expected", expectedEx.ExpectedException),
						Type = TestResultType.Fail
					};
				}
				else
				{
					return new TestResult()
					{
						TestName = mi.Name,
						Message = "ok",
						Type = TestResultType.Ok
					};
				}
			}
			catch (TargetInvocationException tiex)
			{
				Exception ex = tiex.InnerException;

				if (ex is SkipThisTestException)
				{
					return new TestResult()
					{
						TestName = mi.Name,
						Message = "skipped",
						Type = TestResultType.Skipped
					};
				}

				if (expectedEx != null && expectedEx.ExpectedException.IsInstanceOfType(ex))
				{
					return new TestResult()
					{
						TestName = mi.Name,
						Message = "ok",
						Type = TestResultType.Ok
					};
				}
				else
				{
					return new TestResult()
					{
						TestName = mi.Name,
						Message = ex.Message,
						Type = TestResultType.Fail,
						Exception = ex
					};
				}
			}
		}
	}
}
