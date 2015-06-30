using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Playground
{
	public class Foo
	{
		public static void Log1(string msg) { }

		public static void Test1(string msg, out string obj, string val)
		{
			Console.WriteLine("{0} - {1}", msg ?? "(NULL)", val ?? "(NULL)");
			obj = msg;
		}

	}

	public static class FooExtension
	{
		public static void Log2(this Foo self, string msg) { }
	}

	class Program
	{
		static void Main(string[] args)
		{
			UserData.RegisterType<Foo>();
			UserData.RegisterType<Dictionary<int, int>>();
			UserData.RegisterExtensionType(typeof(FooExtension));

			var lua = new Script();
			lua.Globals["DictionaryIntInt"] = typeof(Dictionary<int, int>);

			var script = @"local dict = DictionaryIntInt.__new(); local res, v = dict.TryGetValue(0)";
			lua.DoString(script);
			lua.DoString(script);


			//var lua = new Script();
			//lua.Globals["Foo"] = typeof(Foo);

			//var script = @"local _, obj = Foo.Test1('ciao', 'hello'); print(obj);";
			//lua.DoString(script);





			Console.WriteLine("Done");
			Console.ReadKey();


		}
	}
}
