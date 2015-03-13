#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;


namespace MoonSharp.Interpreter.Tree.Expressions
{
	class AdjustmentExpression : Expression 
	{
		private Expression expression;

		public AdjustmentExpression(IParseTree tree, ScriptLoadingContext lcontext, IParseTree subtree)
			: base(tree, lcontext)
		{
			expression = NodeFactory.CreateExpression(subtree, lcontext);
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			expression.Compile(bc);
			bc.Emit_Scalar();
		}

		public override DynValue Eval(ScriptExecutionContext context)
		{
			return expression.Eval(context).ToScalar();
		}
	}
}

#endif