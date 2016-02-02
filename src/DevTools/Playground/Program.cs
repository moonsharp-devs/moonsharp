using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Serialization;
using MoonSharp.Interpreter.Interop;
using MoonSharp.RemoteDebugger;
using System.IO;
using System.CodeDom.Compiler;
using MoonSharp.Hardwire;
using MoonSharp.Hardwire.Languages;

namespace MoonSharp.Playground
{
	class ConsoleLogger : ICodeGenerationLogger
	{
		public void LogError(string message)
		{
			Console.WriteLine("[EE] - " + message);
		}

		public void LogWarning(string message)
		{
			Console.WriteLine("[ww] - " + message);
		}

		public void LogMinor(string message)
		{
			Console.WriteLine("[ii] - " + message);
		}
	}


	class Program
	{
		static void Main(string[] args)
		{
			UserData.RegisterType<TimeSpan>();

			Table t = UserData.GetDescriptionOfRegisteredTypes();

			Script s = new Script();
			var eee = s.CreateDynamicExpression(File.ReadAllText(@"c:\temp\testdump.lua"));

			//Table t = eee.Evaluate(null).Table;

			string str = t.Serialize();
			//File.WriteAllText(@"c:\temp\luadump.lua", str);

			HardwireGeneratorRegistry.RegisterPredefined();

			HardwireGenerator hcg = new HardwireGenerator("MyNamespace", "MyClass", new ConsoleLogger(), HardwireCodeGenerationLanguage.CSharp);

			hcg.BuildCodeModel(t);

			string code = hcg.GenerateSourceCode();

			File.WriteAllText(@"c:\temp\gen.cs", code);

			//Console.WriteLine(str);
			Console.WriteLine("--done");

			//Console.ReadKey();

		}



	}
}
