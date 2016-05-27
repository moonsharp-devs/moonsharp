using MoonSharp.Interpreter;
using System;

namespace Test
{
	class Program
	{
		private static void CaptureNewIndex(Table table, DynValue index, DynValue value)
		{
			if (index.String == "math")
			{
				return; // could do a throw new ScriptRuntimeException($"{index} is read-only");
			}

			table.Set(index, value);
		}

		static void Main(string[] args)
		{
			string scriptCode = @"
				math = { sin = function(x) return 3*x; end }
				print(math.sin(1.57));
			";

			Script script = new Script(CoreModules.None);
			Table protectedTable = new Table(script);

			protectedTable.RegisterCoreModules(CoreModules.Preset_HardSandbox);

			script.Globals.MetaTable = new Table(script);

			script.Globals.MetaTable["__index"] = protectedTable;
			script.Globals.MetaTable["__newindex"] = (Action<Table, DynValue, DynValue>)CaptureNewIndex;

			script.DoString(scriptCode);

			Console.WriteLine(">> done");
			Console.ReadKey();
		}
	}
}