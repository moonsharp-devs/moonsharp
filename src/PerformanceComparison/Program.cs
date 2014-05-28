using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Execution;
using NLua;
using System.Diagnostics;

namespace PerformanceComparison
{
	class Program
	{
		const int ITERATIONS = 10000;

		static  string scriptText = @"
			function move(n, src, dst, via)
				if n > 0 then
					move(n - 1, src, via, dst)
					--print(src, 'to', dst)
					move(n - 1, via, dst, src)
				end
			end
 
			move(4, 1, 2, 3)
			";

		static StringBuilder g_MoonSharpStr = new StringBuilder();
		static StringBuilder g_NLuaStr = new StringBuilder();

		public static RValue Print(RValue[] values)
		{
			foreach (var val in values)
			{
				g_MoonSharpStr.Append(val.AsString());
			}

			g_MoonSharpStr.AppendLine();
			return RValue.Nil;
		}

		private static void Example()
		{
			Table t = new Table();
			t[new RValue("print")] = new RValue(new CallbackFunction(Print));

			Script script = MoonSharpInterpreter.LoadFromFile(@"c:\temp\test.lua", t);

			RValue retVal = script.Execute();
		}

		public static void NPrint(params object[] values)
		{
			foreach (var val in values)
			{
				g_NLuaStr.Append(val.ToString());
			}
			g_NLuaStr.AppendLine();
		}

		static Lua lua = new Lua();
		static string testString = "world";

		static void Main(string[] args)
		{
			Stopwatch sw;

			sw = Stopwatch.StartNew();

			Table t = new Table();
			t[new RValue("print")] = new RValue(new CallbackFunction(Print));

			MoonSharpInterpreter.LoadFromString(scriptText, t);

			sw.Stop();

			Console.WriteLine("Build : {0} ms", sw.ElapsedMilliseconds);

			sw = Stopwatch.StartNew();

			t = new Table();
			t[new RValue("print")] = new RValue(new CallbackFunction(Print));

			var script = MoonSharpInterpreter.LoadFromString(scriptText, t);

			sw.Stop();

			Console.WriteLine("Build 2: {0} ms", sw.ElapsedMilliseconds);


			sw = Stopwatch.StartNew();
			for (int i = 0; i < ITERATIONS; i++)
			{
				script.Execute();
			}
			sw.Stop();

			Console.WriteLine("Moon# : {0} ms", sw.ElapsedMilliseconds);


			lua.RegisterFunction("print", typeof(Program).GetMethod("NPrint"));

			File.WriteAllText(@"c:\temp\hanoi.lua", scriptText);

			var fn = lua.LoadFile(@"c:\temp\hanoi.lua");

			sw = Stopwatch.StartNew();
			for (int i = 0; i < ITERATIONS; i++)
			{
				fn.Call();
			}
			sw.Stop();

			Console.WriteLine("NLua  : {0} ms", sw.ElapsedMilliseconds);

			Console.WriteLine("M# == NL ? {0}", g_MoonSharpStr.ToString() == g_NLuaStr.ToString());

			//Console.WriteLine("=== Moon# ===");
			//Console.WriteLine(g_MoonSharpStr.ToString());
			//Console.WriteLine("");
			//Console.WriteLine("=== NLua  ===");
			//Console.WriteLine(g_NLuaStr.ToString());

			Console.ReadKey();
		}
	}
}
