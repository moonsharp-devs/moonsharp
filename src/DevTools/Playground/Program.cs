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

namespace Playground
{
	
	class Program
	{
		static void Main(string[] args)
		{
			UserData.RegisterType<Script>();

			Table t = UserData.GetDescriptionOfRegisteredTypes();

			string str = t.Serialize();

			Script s = new Script();

			var exp = s.CreateDynamicExpression(str);

			DynValue D = exp.Evaluate(null);

			//File.WriteAllText(@"c:\temp\luadump.lua", str);
			//File.WriteAllText(@"c:\temp\luadump2.lua", D.Table.Serialize());

			HardwireGeneratorRegistry.AutoRegister();

			HardwireCodeGenerator hcg = new HardwireCodeGenerator(t);

			hcg.GenerateCode();

			//Console.WriteLine(str);
			Console.WriteLine("--done");

			//Console.ReadKey();

		}



	}
}
