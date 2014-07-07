using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class ExprListExpression : Expression 
	{
		Expression[] expressions;

		public ExprListExpression(LuaParser.ExplistContext tree, ScriptLoadingContext lcontext)
			: base(tree, lcontext)
		{
			expressions = tree.children
				.Select(t => NodeFactory.CreateExpression(t, lcontext))
				.Where(e => e != null)
				.ToArray();
		}


		public Expression[] GetExpressions()
		{
			return expressions;
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			foreach (var exp in expressions)
				exp.Compile(bc);

			if (expressions.Length > 1)
				bc.Emit_MkTuple(expressions.Length);
		}
	}
}
