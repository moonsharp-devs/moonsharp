using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Commands
{
	static class CommandManager
	{
		static Dictionary<string, ICommand> m_Registry = new Dictionary<string, ICommand>();

		public static void Initialize()
		{
			foreach (Type t in typeof(CommandManager).Assembly.GetTypes()
				.Where(tt => typeof(ICommand).IsAssignableFrom(tt))
				.Where(tt => tt.IsClass && (!tt.IsAbstract))
			)
			{
				object o = Activator.CreateInstance(t);
				ICommand cmd = (ICommand)o;
				m_Registry.Add(cmd.Name, cmd);
			}
		}

		public static void Execute(ShellContext context, string commandLine)
		{

		}

		public static IEnumerable<ICommand> GetCommands()
		{
			yield return m_Registry["help"];

			foreach (ICommand cmd in m_Registry.Values.Where(c => !(c is HelpCommand)).OrderBy(c => c.Name))
			{
				yield return cmd;
			}
		}


		public static ICommand Find(string cmd)
		{
			if (m_Registry.ContainsKey(cmd))
				return m_Registry[cmd];

			return null;
		}
	}
}
