using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree
{
	abstract class Expression : NodeBase
	{
		protected Expression(IParseTree node, ScriptLoadingContext lcontext)
			: base(node, lcontext)
		{ }


		public abstract RValue Eval(RuntimeScope scope); 

	}
}
