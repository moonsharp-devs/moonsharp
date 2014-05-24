using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;

namespace MoonSharp.Interpreter.Diagnostics
{
	public class AstDump
	{
		StringBuilder m_TreeDump = new StringBuilder();

		public void DumpTree(IParseTree tree, string filename)
		{
			DumpTree(tree, 0);
			File.WriteAllText(filename, m_TreeDump.ToString());
		}

		private void DumpTree(IParseTree tree, int depth = 0)
		{
			string tabs = new string(' ', depth * 4);

			m_TreeDump.AppendFormat("{0}{1} : {2}\n", tabs, Purify(tree.GetType()), tree.GetText());

			for (int i = 0; i < tree.ChildCount; i++)
			{
				DumpTree(tree.GetChild(i), depth + 1);
			}
		}

		private string Purify(Type type)
		{
			string t = type.ToString();

			if (t.StartsWith("MoonSharp.Interpreter.Grammar.LuaParser+"))
			{
				return t.Replace("MoonSharp.Interpreter.Grammar.LuaParser+", "").Replace("Context", "").ToUpper();
			}
			else if (t == "Antlr4.Runtime.Tree.TerminalNodeImpl") return "/TERM";

			return t;

		}
	}
}
