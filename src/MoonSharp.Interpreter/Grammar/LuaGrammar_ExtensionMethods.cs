using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Antlr4.Runtime.Tree;

namespace MoonSharp.Interpreter
{
	internal static class LuaGrammar_ExtensionMethods
	{
		public static IEnumerable<IParseTree> EnumChilds(this IParseTree tree)
		{
			for (int i = 0; i < tree.ChildCount; i++)
				yield return tree.GetChild(i);
		}
	}
}
