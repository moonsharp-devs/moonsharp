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
	class UnaryOperatorExpression : Expression
	{
		Expression m_Exp;
		string m_OpText;

		public UnaryOperatorExpression(IParseTree tree, ScriptLoadingContext lcontext)
			: base(tree, lcontext)
		{
			var child0 = tree.GetChild(0);

			m_OpText = child0.GetText();

			m_Exp = NodeFactory.CreateExpression(tree.GetChild(1), lcontext);
		}


		public override void Compile(ByteCode bc)
		{
			m_Exp.Compile(bc);

			switch (m_OpText)
			{
				case "not":
					bc.Emit_Operator(OpCode.Not);
					break;
				case "#":
					bc.Emit_Operator(OpCode.Len);
					break;
				case "-":
					bc.Emit_Operator(OpCode.Neg);
					break;
				default:
					throw new InternalErrorException("Unexpected unary operator '{0}'", m_OpText);
			}


		}

		public override DynValue Eval(ScriptExecutionContext context)
		{
			DynValue v = m_Exp.Eval(context).ToScalar();

			switch (m_OpText)
			{
				case "not":
					return DynValue.NewBoolean(!v.CastToBool());
				case "#":
					return v.GetLength();
				case "-":
					{
						double? d = v.CastToNumber();

						if (d.HasValue)
							return DynValue.NewNumber(-d.Value);

						throw new DynamicExpressionException("Attempt to perform arithmetic on non-numbers.");
					}
				default:
					throw new DynamicExpressionException("Unexpected unary operator '{0}'", m_OpText);
			}
		}
	}
}
