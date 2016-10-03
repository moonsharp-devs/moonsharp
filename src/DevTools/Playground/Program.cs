using MoonSharp.Interpreter;
using System;
using System.Diagnostics;
using System.IO;
using MoonSharp.Interpreter.Loaders;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				DynValue v = Script.RunString("return 3 + .5");
				Console.WriteLine(v.Number);
			}
			catch (InterpreterException ex)
			{
				Console.WriteLine(ex.DecoratedMessage);
			}

			Console.WriteLine(">> DONE");

			Console.ReadKey();
		}
	}
}