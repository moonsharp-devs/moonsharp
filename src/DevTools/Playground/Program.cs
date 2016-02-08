using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
			{
				string test = @"
function print_env()
  print(_ENV)
end

function sandbox()
  print(_ENV) -- prints: 'table: 0x100100610'
  -- need to keep access to a few globals:
  _ENV = { print = print, print_env = print_env, debug = debug, load = load }
  print(_ENV) -- prints: 'table: 0x100105140'
  print_env() -- prints: 'table: 0x100105140'
  local code1 = load('print(_ENV)')
  code1()     -- prints: 'table: 0x100100610'
  debug.setupvalue(code1, 0, _ENV) -- set our modified env
  debug.setupvalue(code1, 1, _ENV) -- set our modified env
  code1()     -- prints: 'table: 0x100105140'
  local code2 = load('print(_ENV)', nil, nil, _ENV) -- pass 'env' arg
  code2()     -- prints: 'table: 0x100105140'
end

sandbox()";

				Script S = new Script(CoreModules.Preset_Complete);

				S.DoString(test);

				Console.ReadKey();
				return;
			}




			UserData.RegisterType<TimeSpan>();

			//Table t = UserData.GetDescriptionOfRegisteredTypes();

			Script s = new Script();
			var eee = s.CreateDynamicExpression(File.ReadAllText(@"c:\temp\testdump.lua"));

			Table t = eee.Evaluate(null).Table;

			string str = t.Serialize();
			File.WriteAllText(@"c:\temp\luadump.lua", str);

			HardwireGeneratorRegistry.RegisterPredefined();

			HardwireGenerator hcg = new HardwireGenerator("MyNamespace", "MyClass", new ConsoleLogger(), HardwireCodeGenerationLanguage.CSharp)
			{
				AllowInternals = true
			};

			hcg.BuildCodeModel(t);

			string code = hcg.GenerateSourceCode();

			File.WriteAllText(@"c:\temp\gen.cs", code);

			//Console.WriteLine(str);
			Console.WriteLine("--done");

			//Console.ReadKey();

		}



	}
}
