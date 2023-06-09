using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;

namespace MoonSharp.Commands.Implementations
{
	class CompileCommand : ICommand
	{
		public string Name
		{
			get { return "compile"; }
		}

		public void DisplayShortHelp()
		{
			Console.WriteLine("compile <filename> - Compiles the file in a binary format");
		}

		public void DisplayLongHelp()
		{
			Console.WriteLine("compile <filename> - Compiles the file in a binary format.\nThe destination filename will be appended with '-compiled'.");
		}

		public void Execute(ShellContext context, string p)
		{
			string targetFileName = p + "-compiled";

			Script S = new Script(CoreModules.None);

			DynValue chunk = S.LoadFile(p);

			using (Stream stream = new FileStream(targetFileName, FileMode.Create, FileAccess.Write))
				S.Dump(chunk, stream);
		}
	}
}
