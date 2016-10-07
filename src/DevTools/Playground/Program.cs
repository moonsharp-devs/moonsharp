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

			try
			{
				Script S = new Script();
				S.Options.ColonOperatorClrCallbackBehaviour = ColonOperatorBehaviour.TreatAsDotOnUserData;

				S.DoString(@"

require 'test'
require 'test2'

");


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