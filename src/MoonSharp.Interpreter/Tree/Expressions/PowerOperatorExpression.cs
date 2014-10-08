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
	}
}
