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
		}

		Operator m_Operator;
		Expression m_Exp1 = null;
		Expression m_Exp2 = null;

		public OperatorExpression(LuaParser.ExpContext tree, ScriptLoadingContext lcontext)
			: base(tree, lcontext)
		{
			if (s_OperatorTypes.Contains(tree.GetChild(0).GetType()))
			{
				// unary op
				SyntaxAssert(tree.ChildCount == 2, "Unexpected node found");
				m_Operator = ParseUnaryOperator(tree.GetChild(0));
				m_Exp1 = NodeFactory.CreateExpression(tree.GetChild(1), lcontext);
			}
			else
			{
				// binary op
				SyntaxAssert(tree.ChildCount == 3, "Unexpected node found");
				m_Operator = ParseBinaryOperator(tree.GetChild(1));
				m_Exp1 = NodeFactory.CreateExpression(tree.GetChild(0), lcontext);
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

			if (m_Operator == Operator.Or)
			{
				Instruction i = bc.Emit_Jump(OpCode.JtOrPop, -1);
				m_Exp2.Compile(bc);
				i.NumVal = bc.GetJumpPointForNextInstruction();
				return;
			}

			if (m_Operator == Operator.And)
			{
				Instruction i = bc.Emit_Jump(OpCode.JfOrPop, -1);
				m_Exp2.Compile(bc);
				i.NumVal = bc.GetJumpPointForNextInstruction();
				return;
			}
			

			if (m_Exp2 != null)
			{
				m_Exp2.Compile(bc);
			}

			bc.Emit_Operator(OperatorToOpCode(m_Operator));

			if (ShouldInvertBoolean(m_Operator))
				bc.Emit_Operator(OpCode.Not);
		}

		private bool ShouldInvertBoolean(Operator op)
		{
			return (op == Operator.NotEqual)
				|| (op == Operator.GreaterOrEqual)
				|| (op == Operator.Greater);
		}


		private OpCode OperatorToOpCode(Operator op)
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
					throw new InternalErrorException("Unsupported operator {0}", m_Operator);
			}
		}
	}
}
