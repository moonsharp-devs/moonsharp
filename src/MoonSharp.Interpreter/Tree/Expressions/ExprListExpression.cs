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

		public override RValue Eval(RuntimeScope scope)
		{
			RValue[] values = expressions.Select(e => e.Eval(scope)).ToArray();

			if (values.Length > 1)
			{
				return RValue.FromPotentiallyNestedTuple(values);
			}
			else if (values.Length == 0)
				return RValue.Nil;
			else
				return values[0].AsReadOnly();
		}

		public Expression[] Unpack()
		{
			return expressions;
		}

		public override void Compile(Execution.VM.Chunk bc)
		{
			foreach (var exp in expressions)
				exp.Compile(bc);

			if (expressions.Length > 1)
				bc.MkTuple(expressions.Length);
		}
	}
}
