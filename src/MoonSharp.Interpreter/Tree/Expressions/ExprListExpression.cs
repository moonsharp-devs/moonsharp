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
		List<Expression> expressions;

		public ExprListExpression(List<Expression> exps, ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			expressions = exps;
		}

		public ExprListExpression(LuaParser.ExplistContext tree, ScriptLoadingContext lcontext)
			: base(tree, lcontext)
		{
			expressions = NodeFactory.CreateExpessionArray(tree.children, lcontext).ToList();
		}


		public Expression[] GetExpressions()
		{
			return expressions.ToArray();
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			foreach (var exp in expressions)
				exp.Compile(bc);

			if (expressions.Count > 1)
				bc.Emit_MkTuple(expressions.Count);
		}

		public override DynValue Eval(ScriptExecutionContext context)
		{
			if (expressions.Count >= 1)
				return expressions[0].Eval(context);

			return DynValue.Void;
		}
	}
}
