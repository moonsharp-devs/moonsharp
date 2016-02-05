using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Commands
{
	class HelpCommand : ICommand
	{
		public string Name
		{
			get { return "help"; }
		}

		public void DisplayShortHelp()
		{
			Console.WriteLine("help [command] - gets the list of possible commands or help about the specified command");
		}

		public void DisplayLongHelp()
		{
			DisplayShortHelp();
		}

		public void Execute(ShellContext context, string arguments)
		{
			if (arguments.Length > 0)
			{
				var cmd = CommandManager.Find(arguments);
				if (cmd != null)
					cmd.DisplayLongHelp();
				else
					Console.WriteLine("Command '{0}' not found.", arguments);
			}
			else
			{
				Console.WriteLine("Type Lua code to execute Lua code (multilines are accepted)");
				Console.WriteLine("or type one of the following commands to execute them.");
				Console.WriteLine("");
				Console.WriteLine("Commands:");
				Console.WriteLine("");

				foreach (var cmd in CommandManager.GetCommands())
				{
					Console.Write("  !");
					cmd.DisplayShortHelp();
				}

				Console.WriteLine("");
			}
		}
	}
}
