using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;

namespace MoonSharp.Commands.Implementations
{
	class RegisterCommand : ICommand
	{
		public string Name
		{
			get { return "register"; }
		}

		public void DisplayShortHelp()
		{
			Console.WriteLine("register [type] - register a CLR type or prints a list of registered types");
		}

		public void DisplayLongHelp()
		{
			Console.WriteLine("register [type] - register a CLR type or prints a list of registered types. Use makestatic('type') to make a static instance.");
		}

		public void Execute(ShellContext context, string argument)
		{
			if (argument.Length > 0)
			{
				Type t = Type.GetType(argument);
				if (t == null)
					Console.WriteLine("Type {0} not found.", argument);
				else
					UserData.RegisterType(t);
			}
			else
			{
				foreach (var type in UserData.GetRegisteredTypes())
				{
					Console.WriteLine(type.FullName);
				}
			}
		}
	}
}
