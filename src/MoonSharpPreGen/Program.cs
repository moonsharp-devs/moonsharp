using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace MoonSharpPreGen
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("MoonSharp PreGen Tool {0} [{1}]", Script.VERSION, Script.GlobalOptions.Platform.GetPlatformName());
			Console.WriteLine("Copyright (C) 2014-2015 Marco Mastropaolo");
			Console.WriteLine("http://www.moonsharp.org");
			Console.WriteLine();

			Console.WriteLine("Usage:");
			Console.WriteLine("MoonSharpPreGen <dllfiles> [-t:<typelistfiles>] [-outtype:<outputtype>] [-out:<outputfile>] ");
			Console.WriteLine();
			Console.WriteLine("Will generate optimized interop type descriptors from existing types.");
			Console.WriteLine("As an alternative to use runtime type descriptors, these offer better performance.");
			Console.WriteLine("and the best compatibility with AOT platforms.");
			Console.WriteLine();
			Console.WriteLine("Unless a list of types is specified, the types described will be those marked as .");
			Console.WriteLine("[MoonSharpUserData] in code.");
			Console.WriteLine("Output type can be 'cs' to generate C# source files, 'vb' to generate VB.NET source files");
			Console.WriteLine("or 'dll' to generate a class library (.NET 4.x).");
			Console.WriteLine();
			Console.WriteLine("Options:");
			Console.WriteLine("    dllfiles : class libraries containing the types to describe");
			Console.WriteLine("    typelistfiles : text files containing the list of types to describe.");
			Console.WriteLine("    -out : output file containing the descriptors");
			Console.WriteLine("    -outtype : which language to generate the sources: either cs, vb or dll (default:cs)");



			Console.ReadKey();
		}
	}
}
