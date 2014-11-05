using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class PowerOperatorExpression : Expression
	{
		Expression m_Exp1, m_Exp2;

		public PowerOperatorExpression(IParseTree tree, ScriptLoadingContext lcontext)
			: base(tree, lcontext)
		{
			m_Exp1 = NodeFactory.CreateExpression(tree.GetChild(0), lcontext);
			m_Exp2 = NodeFactory.CreateExpression(tree.GetChild(2), lcontext);
		}

		public override void Compile(ByteCode bc)
		{
			m_Exp1.Compile(bc);
			m_Exp2.Compile(bc);
			bc.Emit_Operator(OpCode.Power);
		}

		public override DynValue Eval(ScriptExecutionContext context)
		{
			DynValue v1 = m_Exp1.Eval(context).ToScalar();
			DynValue v2 = m_Exp1.Eval(context).ToScalar();

			double? d1 = v1.CastToNumber();
			double? d2 = v1.CastToNumber();

			if (d1.HasValue && d2.HasValue)
				return DynValue.NewNumber(Math.Pow(d1.Value, d2.Value));

			throw new DynamicExpressionException("Attempt to perform arithmetic on non-numbers.");
		}
	}
}
