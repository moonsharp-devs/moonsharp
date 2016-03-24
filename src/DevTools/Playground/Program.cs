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
			string scriptCode = @"

local aClass = {}
setmetatable(aClass, {__newindex = function() end, __index = function() end })

local p = {a = 1, b = 2}
 
for x , v in pairs(p) do
	print (x, v)
	aClass[x] = v
end

";

			Script script = new Script(CoreModules.Basic | CoreModules.Table | CoreModules.TableIterators | CoreModules.Metatables);

			DynValue res = script.DoString(scriptCode);

			Console.WriteLine(">> done");
			Console.ReadKey();
			return;
		}
	}
}