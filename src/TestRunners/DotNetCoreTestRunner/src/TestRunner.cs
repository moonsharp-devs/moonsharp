using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.Compatibility;
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
		public int Ok = 0;
		public int Fail = 0;
		public int Total = 0;
		public int Skipped = 0;

		public static bool IsRunning { get; private set; }

		public TestRunner(Action<TestResult> loggerAction)
		{
			IsRunning = true;

			this.loggerAction = loggerAction;

			Console_WriteLine("MoonSharp Test Suite Runner - {0} [{1}]", Script.VERSION, Script.GlobalOptions.Platform.GetPlatformName());
			Console_WriteLine("http://www.moonsharp.org");
			Console_WriteLine("");
		}

		public void Test(string whichTest = null, string[] testsToSkip = null)
		{
			foreach (TestResult tr in IterateOnTests(whichTest, testsToSkip))
				loggerAction(tr);
		}


		public IEnumerable<TestResult> IterateOnTests(string whichTest = null, string[] testsToSkip = null, Type[] types = null)
		{
			HashSet<string> skipList = new HashSet<string>();

			if (testsToSkip != null)
				skipList.UnionWith(testsToSkip);

			Assembly asm = Framework.Do.GetAssembly(typeof(TestRunner));

			types = types ?? Framework.Do.GetAssemblyTypes(asm).Where(
				t => Framework.Do.GetCustomAttributes(t, typeof(TestFixtureAttribute), true).Any()).ToArray();

#if UNITY_EDITOR_OSX
            System.IO.File.WriteAllLines("/temp/types.cs", types.Select(t => t.FullName).ToArray());
#endif

			Console_WriteLine("Found {0} test types.", types.Length);

			foreach (Type t in types)
			{
				MethodInfo[] tests = Framework.Do.GetMethods(t).Where(m => m.GetCustomAttributes(typeof(TestAttribute), true).Any()).ToArray();
				//Console_WriteLine("Testing {0} - {1} tests found.", t.Name, tests.Length);

				foreach (MethodInfo mi in tests)
				{
					if (whichTest != null && mi.Name != whichTest)
						continue;

					if (skipList.Contains(mi.Name))
					{
						++Skipped;
						TestResult trs = new TestResult()
						{
							TestName = mi.Name,
							Message = "skipped (skip-list)",
							Type = TestResultType.Skipped
						};
						yield return trs;
						continue;
					}

					TestResult tr = RunTest(t, mi);

					if (tr.Type != TestResultType.Message)
					{
						if (tr.Type == TestResultType.Fail)
							++Fail;
						else if (tr.Type == TestResultType.Ok)
							++Ok;
						else
							++Skipped;

						++Total;
					}

					yield return tr;
				}
			}

			Console_WriteLine("");
			Console_WriteLine("OK : {0}/{2}, Failed {1}/{2}, Skipped {3}/{2}", Ok, Fail, Total, Skipped);
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

				if (expectedEx != null && Framework.Do.IsInstanceOfType(expectedEx.ExpectedException, ex))
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
						Message = BuildExceptionMessage(ex),
						Type = TestResultType.Fail,
						Exception = ex
					};
				}
			}
		}

		private static string BuildExceptionMessage(Exception ex)
		{
			StringBuilder sb = new StringBuilder();

			for (Exception e = ex; e != null; e = e.InnerException)
			{
				sb.Append(">>> ");
				sb.Append(e.Message);
			}

			return sb.ToString();
		}

		internal static void Skip()
		{
			if (TestRunner.IsRunning)
				throw new SkipThisTestException();
		}
	}
}
