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
			Table T = new Table(null);
			T.Append(DynValue.NewNumber(12));

			Benchmark(10);
			Benchmark(100);
			Benchmark(1000);
			Benchmark(10000);
			Benchmark(100000);


			Console.WriteLine(">> DONE");

			Console.ReadKey();
		}

		private static void Benchmark(int count)
		{
			Table T = new Table(null);

			Stopwatch sw = Stopwatch.StartNew();

			for (int i = 0; i < count; i++)
			{
				T.Append(DynValue.NewNumber(i));
				//T.Set(i, DynValue.NewNumber(i));
			}

			sw.Stop();

			Console.WriteLine($"{count} elements -> ${sw.ElapsedMilliseconds} ms");


		}
	}
}