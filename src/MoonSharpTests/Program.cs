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
		public const string RESTRICT_TEST = null; //"ForEachLoop";

		static void Main(string[] args)
		{
			int ok = 0;
			int fail = 0;
			int total = 0;

			Console.WriteLine("Moon# Test Suite Runner");
			Console.WriteLine("Copyright (C) 2014 Marco Mastropaolo [http://www.mastropaolo.com]");
			Console.WriteLine("See : http://moonsharp.codeplex.com");
			Console.WriteLine();

			Assembly asm = Assembly.GetAssembly(typeof(SimpleTests));

			foreach (Type t in asm.GetTypes().Where(t => t.GetCustomAttributes(typeof(TestFixtureAttribute), true).Any()))
			{
				foreach (MethodInfo mi in t.GetMethods().Where(m => m.GetCustomAttributes(typeof(TestAttribute), true).Any()))
				{
					if (RESTRICT_TEST != null && mi.Name != RESTRICT_TEST)
						continue;

					if (RunTest(t, mi))
						++ok;
					else
						++fail;
					++total;
				}
			}

			Console.WriteLine("");

			Console.WriteLine("OK : {0}/{2}, Failed {1}/{2}", ok, fail, total);

			
			if (Debugger.IsAttached)
			{
				Console.WriteLine("Press any key...");
				Console.ReadKey();
			}
		}

		private static bool RunTest(Type t, MethodInfo mi)
		{
			Console.Write("{0}...", mi.Name);

			try
			{
				object o = Activator.CreateInstance(t);
				mi.Invoke(o, new object[0]);
				Console.WriteLine(" ok ");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(" failed: {0} ", ex.InnerException.Message);
				return false;
			}
		}
	}
}
