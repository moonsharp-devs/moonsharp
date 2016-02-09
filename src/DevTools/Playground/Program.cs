using MoonSharp.Interpreter;
using System;

namespace Test
{
	[MoonSharpUserData]
	class MyClass
	{
		public double calcHypotenuse(double a, double b)
		{
			return Math.Sqrt(a * a + b * b);
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			string scriptCode = @"return obj.calcHypotenuse(3, 4);";

			// Automatically register all MoonSharpUserData types
			UserData.RegisterAssembly();

			Script script = new Script();

			// Pass an instance of MyClass to the script in a global
			script.Globals["obj"] = new MyClass();

			DynValue res = script.DoString(scriptCode);

			return;
		}
	}
}