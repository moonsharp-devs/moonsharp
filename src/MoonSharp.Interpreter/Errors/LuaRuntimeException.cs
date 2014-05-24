using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr4.Runtime.Tree;

namespace MoonSharp.Interpreter
{
	[Serializable]
	public class LuaRuntimeException : Exception
	{
		internal LuaRuntimeException(IParseTree tree, string format, params object[] args)
			: base(string.Format(format, args) + FormatTree(tree))
		{

		}

		private static string FormatTree(IParseTree tree)
		{
			if (tree == null)
				return "";

			return "@ " + tree.GetText();

		}
	}
}
