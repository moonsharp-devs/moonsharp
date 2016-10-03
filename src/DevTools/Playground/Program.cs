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
			((ScriptLoaderBase)Script.DefaultOptions.ScriptLoader).ModulePaths = new string[] { "./?", "./?.lua" };
			string code = @"
			
require 'samplescript'

";

			try
			{
				Script.RunString(code);
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