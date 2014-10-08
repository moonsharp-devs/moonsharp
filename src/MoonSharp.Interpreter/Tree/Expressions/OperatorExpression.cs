#if false

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
	class OperatorExpression : Expression 
	{
		private enum Operator
		{
			Or,
			And,
			Less, Greater, LessOrEqual, GreaterOrEqual, NotEqual, Equal,
			StrConcat,
			Add, Sub,
			Mul, Div, Mod,
			Not, Size, Neg,
			Power
		}

		static HashSet<Type> s_OperatorTypes = new HashSet<Type>();
		static HashSet<Type> s_LeftAssocOperatorTypes = new HashSet<Type>();

		static OperatorExpression()
		{
			s_OperatorTypes.Add(typeof(LuaParser.OperatorOrContext));
			s_OperatorTypes.Add(typeof(LuaParser.OperatorAndContext));
			s_OperatorTypes.Add(typeof(LuaParser.OperatorComparisonContext));
			s_OperatorTypes.Add(typeof(LuaParser.OperatorStrcatContext));
			s_OperatorTypes.Add(typeof(LuaParser.OperatorAddSubContext));
			s_OperatorTypes.Add(typeof(LuaParser.OperatorMulDivModContext));
			s_OperatorTypes.Add(typeof(LuaParser.OperatorUnaryContext));
			s_OperatorTypes.Add(typeof(LuaParser.OperatorPowerContext));

			s_LeftAssocOperatorTypes.Add(typeof(LuaParser.Exp_logicOrContext));
			s_LeftAssocOperatorTypes.Add(typeof(LuaParser.Exp_logicAndContext));
			s_LeftAssocOperatorTypes.Add(typeof(LuaParser.Exp_compareContext));
			s_LeftAssocOperatorTypes.Add(typeof(LuaParser.Exp_addsubContext));
			s_LeftAssocOperatorTypes.Add(typeof(LuaParser.Exp_muldivContext));
		}

		bool m_IsUnary = false;
		Operator m_Operator;
		Expression m_Exp1;
		Expression m_Exp2;
		List<Expression> m_Exps;
		List<Operator> m_Ops;

		public OperatorExpression(IParseTree tree, ScriptLoadingContext lcontext)
			: base(tree, lcontext)
		{
			var child0 = tree.GetChild(0);

			if (s_OperatorTypes.Contains(child0.GetType()))
			{
				// unary op
				SyntaxAssert(tree.ChildCount == 2, "Unexpected node found");
				m_Operator = ParseUnaryOperator(child0);
				m_Exp1 = NodeFactory.CreateExpression(tree.GetChild(1), lcontext);
				m_IsUnary = true;
			}
			else if (s_LeftAssocOperatorTypes.Contains(tree.GetType()))
			{
				// binary right associative op or simple left associative
				IParseTree child2 = tree.GetChild(2);

				if(child2.GetType() == tree.GetType())
				{
					m_Exps = new List<Expression>();
					m_Ops = new List<Operator>();

					m_Operator = ParseBinaryOperator(tree.GetChild(1));

					while (child2.GetType() == tree.GetType())
					{
						m_Exps.Add(NodeFactory.CreateExpression(child2.GetChild(0), lcontext));
						m_Ops.Add(m_Operator);
						m_Operator = ParseBinaryOperator(child2.GetChild(1));
						child2 = child2.GetChild(2);
					}

					m_Exp1 = NodeFactory.CreateExpression(child0, lcontext);
					m_Exp2 = NodeFactory.CreateExpression(child2, lcontext);
				}
				else
				{
					SyntaxAssert(tree.ChildCount == 3, "Unexpected node found");
					m_Operator = ParseBinaryOperator(tree.GetChild(1));
					m_Exp1 = NodeFactory.CreateExpression(child0, lcontext);
					m_Exp2 = NodeFactory.CreateExpression(child2, lcontext);
				}
			}
			else
			{
				// binary right associative op or simple left associative
				SyntaxAssert(tree.ChildCount == 3, "Unexpected node found");
				m_Operator = ParseBinaryOperator(tree.GetChild(1));
				m_Exp1 = NodeFactory.CreateExpression(child0, lcontext);
				m_Exp2 = NodeFactory.CreateExpression(tree.GetChild(2), lcontext);
			}

		}

		private Operator ParseUnaryOperator(IParseTree parseTree)
		{
			string txt = parseTree.GetText();

			switch (txt)
			{
				case "not":
					return Operator.Not;
				case "#":
					return Operator.Size;
				case "-":
					return Operator.Neg;
				default:
					throw SyntaxError("Unexpected unary operator '{0}'", txt);
			}
		}
		private Operator ParseBinaryOperator(IParseTree parseTree)
		{
			string txt = parseTree.GetText();

			switch (txt)
			{
				case "or":
					return Operator.Or;
				case "and":
					return Operator.And;
				case "<":
					return Operator.Less;
				case ">":
					return Operator.Greater;
				case "<=":
					return Operator.LessOrEqual;
				case ">=":
					return Operator.GreaterOrEqual;
				case "~=":
					return Operator.NotEqual;
				case "==":
					return Operator.Equal;
				case "..":
					return Operator.StrConcat;
				case "+":
					return Operator.Add;
				case "-":
					return Operator.Sub;
				case "*":
					return Operator.Mul;
				case "/":
					return Operator.Div;
				case "%":
					return Operator.Mod;
				case "^":
					return Operator.Power;
				default:
					throw SyntaxError("Unexpected binary operator '{0}'", txt);
			}
		}




		public static bool IsOperatorExpression(IParseTree tree)
		{
			return (tree.EnumChilds().Any(t => s_OperatorTypes.Contains(t.GetType())));
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			m_Exp1.Compile(bc);

			if (m_Exps != null)
			{
				for (int i = 0; i < m_Ops.Count; i++)
				{
					CompileOp(bc, m_Ops[i], m_Exps[i]);
				}
			}

			{
				CompileOp(bc, m_Operator, m_Exp2);
			}

		}

		private void CompileOp(Execution.VM.ByteCode bc, Operator op, Expression exp)
		{
			if (op == Operator.Or)
			{
				Instruction i = bc.Emit_Jump(OpCode.JtOrPop, -1);
				exp.Compile(bc);
				i.NumVal = bc.GetJumpPointForNextInstruction();
				return;
			}

			if (op == Operator.And)
			{
				Instruction i = bc.Emit_Jump(OpCode.JfOrPop, -1);
				exp.Compile(bc);
				i.NumVal = bc.GetJumpPointForNextInstruction();
				return;
			}


			if (exp != null)
			{
				exp.Compile(bc);
			}

			bc.Emit_Operator(OperatorToOpCode(op));

			if (ShouldInvertBoolean(op))
				bc.Emit_Operator(OpCode.Not);
		}



		private static bool ShouldInvertBoolean(Operator op)
		{
			return (op == Operator.NotEqual)
				|| (op == Operator.GreaterOrEqual)
				|| (op == Operator.Greater);
		}


		private static OpCode OperatorToOpCode(Operator op)
		{
			switch (op)
			{
				case Operator.Less:
				case Operator.GreaterOrEqual:
					return OpCode.Less;
				case Operator.LessOrEqual:
				case Operator.Greater:
					return OpCode.LessEq;
				case Operator.Equal:
				case Operator.NotEqual:
					return OpCode.Eq;
				case Operator.StrConcat:
					return OpCode.Concat;
				case Operator.Add:
					return OpCode.Add;
				case Operator.Sub:
					return OpCode.Sub;
				case Operator.Mul:
					return OpCode.Mul;
				case Operator.Div:
					return OpCode.Div;
				case Operator.Mod:
					return OpCode.Mod;
				case Operator.Not:
					return OpCode.Not;
				case Operator.Size:
					return OpCode.Len;
				case Operator.Neg:
					return OpCode.Neg;
				case Operator.Power:
					return OpCode.Power;
				default:
					throw new InternalErrorException("Unsupported operator {0}", op);
			}
		}
	}
}




#endif