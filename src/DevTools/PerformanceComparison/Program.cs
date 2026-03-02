// This code is a workbench code - it gets commented on the fly, changed, etc.
// Disable warnings for "assigned but value never used" and "unreachable code".
#pragma warning disable 414
#pragma warning disable 429

//#define PROFILER

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
#if PROFILER
		const int ITERATIONS = 10;
#else
		const int ITERATIONS = 1;
#endif

		static  string scriptText = @"
			function move(n, src, dst, via)
				if n > 0 then
					move(n - 1, src, via, dst)
					--check(src, 'to', dst)
					move(n - 1, via, dst, src)
				end
			end

			for i = 1, 10000 do
				move(4, 1, 2, 3)
			end
			";
		static  string scriptText22 = @"
N = 8

board = {}
for i = 1, N do
	board[i] = {}
	for j = 1, N do
		board[i][j] = false
	end
end

function Allowed( x, y )
	for i = 1, x-1 do
	if ( board[i][y] ) or ( i <= y and board[x-i][y-i] ) or ( y+i <= N and board[x-i][y+i] ) then
		return false
	end
	end
	return true
end

function Find_Solution( x )
	for y = 1, N do
	if Allowed( x, y ) then
		board[x][y] = true
		if x == N or Find_Solution( x+1 ) then
		return true
		end
		board[x][y] = false
	end
	end
	return false
end

if Find_Solution( 1 ) then
	for i = 1, N do
 	for j = 1, N do
		if board[i][j] then
		--print( 'Q' )
		else
		--print( 'x' )
		end
	end
	--print( '|' )
	end
else
	--print( 'NO!' )
end

			";
		static StringBuilder g_MoonSharpStr = new StringBuilder();
		static StringBuilder g_NLuaStr = new StringBuilder();

		public static DynValue Check(ScriptExecutionContext executionContext, CallbackArguments values)
		{
			//foreach (var val in values.GetArray())
			//{
			//	g_MoonSharpStr.Append(val.ToPrintString());
			//}

			//g_MoonSharpStr.AppendLine();
			return DynValue.Nil;
		}


		public static void NCheck(params object[] values)
		{
			//foreach (var val in values)
			//{
			//	g_NLuaStr.Append(val.ToString());
			//}
			//g_NLuaStr.AppendLine();
		}

		public static void XCheck(int from, string mid, int to)
		{
			g_MoonSharpStr.Append(from);
			g_MoonSharpStr.Append(mid);
			g_MoonSharpStr.Append(to);
			g_MoonSharpStr.AppendLine();
		}

		static Lua lua = new Lua();
		static string testString = "world";

		static void Main(string[] args)
		{
			Script.WarmUp();

			Stopwatch sw;

			sw = Stopwatch.StartNew();

			var _s = new Script();
			_s.LoadString(scriptText);

			sw.Stop();

			Console.WriteLine("Build : {0} ms", sw.ElapsedMilliseconds);

			sw = Stopwatch.StartNew();

			var script = new Script();
			script.Globals.Set("check", DynValue.NewCallback(new CallbackFunction(Check)));
			CallbackFunction.DefaultAccessMode = InteropAccessMode.Preoptimized;

			//script.Globals["print"] = (Action<int, string, int>)PrintX;


			DynValue func = script.LoadString(scriptText);

			sw.Stop();

			Console.WriteLine("Build 2: {0} ms", sw.ElapsedMilliseconds);


			sw = Stopwatch.StartNew();
			for (int i = 0; i < ITERATIONS; i++)
			{
				script.Call(func);
			}
			sw.Stop();

			Console.WriteLine("MoonSharp : {0} ms", sw.ElapsedMilliseconds);


			lua.RegisterFunction("check", typeof(Program).GetMethod("NCheck"));

			var hanoiPath = Path.GetTempPath() + Path.DirectorySeparatorChar + "hanoi.lua";
			File.WriteAllText(hanoiPath, scriptText);

#if !PROFILER
			var fn = lua.LoadFile(hanoiPath);

			sw = Stopwatch.StartNew();
			for (int i = 0; i < ITERATIONS; i++)
			{
				fn.Call();
			}
			sw.Stop();
#endif

			Console.WriteLine("NLua  : {0} ms", sw.ElapsedMilliseconds);

			Console.WriteLine("M# == NL ? {0}", g_MoonSharpStr.ToString() == g_NLuaStr.ToString());

			Console.WriteLine("=== MoonSharp ===");
			//Console.WriteLine(g_MoonSharpStr.ToString());
			Console.WriteLine("");
			Console.WriteLine("=== NLua  ===");
			//Console.WriteLine(g_NLuaStr.ToString());
		}
	}
}
