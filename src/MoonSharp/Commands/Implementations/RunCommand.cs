using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Commands.Implementations
{
	class RunCommand : ICommand
	{
		public string Name
		{
			get { return "run"; }
		}

		public void DisplayShortHelp()
		{
			Console.WriteLine("run <filename> - Executes the specified Lua script");
		}

		public void DisplayLongHelp()
		{
			Console.WriteLine("run <filename> - Executes the specified Lua script.");
		}

		public void Execute(ShellContext context, string arguments)
		{
			if (arguments.Length == 0)
			{
				Console.WriteLine("Syntax : !run <file>");
			}
			else
			{
				context.Script.DoFile(arguments);
			}
		}
	}
}
