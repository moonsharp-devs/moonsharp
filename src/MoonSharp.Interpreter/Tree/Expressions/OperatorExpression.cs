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

		public override RValue Eval(RuntimeScope scope)
		{
			RValue v1 = m_Exp1.Eval(scope);

			if (v1.Type == DataType.Tuple)
				v1 = v1.ToSimplestValue();

			if (m_Operator == Operator.Or && v1.TestAsBoolean())
				return RValue.True;

			if (m_Operator == Operator.And && !(v1.TestAsBoolean()))
				return RValue.False;

			RValue v2 = m_Exp2 != null ? m_Exp2.Eval(scope) : v1;

			if (v2.Type == DataType.Tuple)
				v2 = v2.ToSimplestValue();

			return ExecuteOperator(m_Operator, v1, v2);
		}

		public static RValue ExecuteOperator(Operator op, RValue v1, RValue v2)
		{
			switch (op)
			{
				case Operator.Or:
					return v2.AsBoolean();
				case Operator.And:
					return v2.AsBoolean();
				case Operator.Less:
					return new RValue(v1.Number < v2.Number).AsReadOnly();
				case Operator.Greater:
					return new RValue(v1.Number > v2.Number).AsReadOnly();
				case Operator.LessOrEqual:
					return new RValue(v1.Number < v2.Number).AsReadOnly();
				case Operator.GreaterOrEqual:
					return new RValue(v1.Number >= v2.Number).AsReadOnly();
				case Operator.NotEqual:
					return new RValue(v1.Number != v2.Number).AsReadOnly();
				case Operator.Equal:
					return new RValue(v1.Number == v2.Number).AsReadOnly();
				case Operator.StrConcat:
					return new RValue(v1.String + v2.String).AsReadOnly();
				case Operator.Add:
					return new RValue(v1.Number + v2.Number).AsReadOnly();
				case Operator.Sub:
					return new RValue(v1.Number - v2.Number).AsReadOnly();
				case Operator.Mul:
					return new RValue(v1.Number * v2.Number).AsReadOnly();
				case Operator.Div:
					return new RValue(v1.Number / v2.Number).AsReadOnly();
				case Operator.Mod:
					return new RValue(v1.Number % v2.Number).AsReadOnly();
				case Operator.Not:
					return new RValue(!v1.Boolean);
				case Operator.Size:
					return v1.GetLength();
				case Operator.Neg:
					return new RValue(-v1.Number).AsReadOnly();
				case Operator.Power:
					return new RValue(Math.Pow(v1.Number, v2.Number)).AsReadOnly();
				default:
					throw new NotImplementedException();
			}
		}

		public static bool IsOperatorExpression(IParseTree tree)
		{
			return (tree.EnumChilds().Any(t => s_OperatorTypes.Contains(t.GetType())));
		}

		public override void Compile(Execution.VM.Chunk bc)
		{
			m_Exp1.Compile(bc);

			if (m_Operator == Operator.Or)
			{
				Instruction i = bc.Jump(OpCode.JtOrPop, -1);
				m_Exp2.Compile(bc);
				i.NumVal = bc.GetJumpPointForNextInstruction();
				return;
			}

			if (m_Operator == Operator.And)
			{
				Instruction i = bc.Jump(OpCode.JfOrPop, -1);
				m_Exp2.Compile(bc);
				i.NumVal = bc.GetJumpPointForNextInstruction();
				return;
			}
			

			if (m_Exp2 != null)
			{
				m_Exp2.Compile(bc);
			}

			bc.Operator(OperatorToOpCode(m_Operator));

			if (ShouldInvertBoolean(m_Operator))
				bc.Operator(OpCode.Not);
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
