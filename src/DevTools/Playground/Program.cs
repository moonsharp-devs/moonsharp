using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;

namespace Playground
{
	class MyDictionaryDescriptor : StandardUserDataDescriptor
	{
		public MyDictionaryDescriptor(Type type)
			: base(type, InteropAccessMode.Default)
		{ }

		public override DynValue Index(Script script, object obj, DynValue index, bool isDirectIndexing)
		{
			if (isDirectIndexing)
			{
				string key = index.String;

				if (key.StartsWith("_"))
					index = DynValue.NewString(key.Substring(1));
				else
					isDirectIndexing = false;
			}

			return base.Index(script, obj, index, isDirectIndexing);
		}

		public override bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing)
		{
			if (isDirectIndexing)
			{
				string key = index.String;

				if (key.StartsWith("_"))
					index = DynValue.NewString(key.Substring(1));
				else
					isDirectIndexing = false;
			}

			return base.SetIndex(script, obj, index, value, isDirectIndexing);
		}

	}


	class Program
	{
		static void Main(string[] args)
		{
			Dictionary<string, int> dic = new Dictionary<string, int>();

			dic["hp"] = 33;

			UserData.RegisterType<Dictionary<string, int>>(new MyDictionaryDescriptor(typeof(Dictionary<string, int>)));

			Script s = new Script();

			s.Globals["dic"] = dic;
			try
			{
				s.DoString("print(dic['hp'])");
				s.DoString("print(dic.hp)");
				s.DoString("print(dic._count)");
			}
			catch (ScriptRuntimeException ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine(ex.DecoratedMessage);
			}
			Console.ReadKey();


		}
	}
}
