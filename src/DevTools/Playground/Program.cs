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
				Script S = new Script();
				S.Options.ColonOperatorClrCallbackBehaviour = ColonOperatorBehaviour.TreatAsDotOnUserData;

				Table my_table = S.DoString("my_table = { }; return my_table").Table;
				my_table["Foo"] = (Action<Table, string>)((self, str) => { Console.WriteLine("!!!" + str); });

				S.DoString("my_table:Foo('Ciao');");

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