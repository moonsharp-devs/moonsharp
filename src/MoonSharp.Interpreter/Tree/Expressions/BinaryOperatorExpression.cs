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
	/// <summary>
	/// 
	/// </summary>
	class BinaryOperatorExpression : Expression
	{
		[Flags]
		private enum Operator
		{
			NotAnOperator = 0, 
			
			Or = 0x1, 
			And = 0x2,
			Less = 0x4,
			Greater = 0x8,
			LessOrEqual = 0x10,

			GreaterOrEqual = 0x20,
			NotEqual = 0x40,
			Equal = 0x80,
			StrConcat = 0x100,
			Add = 0x200,
			Sub = 0x400,
			Mul = 0x1000,
			Div = 0x2000,
			Mod = 0x4000,
		}


		class Node
		{
			public Expression Expr;
			public Operator Op;
			public Node Prev;
			public Node Next;
		}

		class LinkedList
		{
			public Node Nodes;
			public Node Last;
		}

		const Operator MUL_DIV_MOD = Operator.Mul | Operator.Div | Operator.Mod;
		const Operator ADD_SUB = Operator.Add | Operator.Sub;
		const Operator STRCAT = Operator.StrConcat;
		const Operator COMPARES = Operator.Less | Operator.Greater | Operator.GreaterOrEqual | Operator.LessOrEqual | Operator.Equal | Operator.NotEqual;
		const Operator LOGIC_AND = Operator.And;
		const Operator LOGIC_OR = Operator.Or;

		private static Operator CreateLinkedList(LinkedList list, IParseTree root, ScriptLoadingContext lcontext)
		{
			Operator opfound = 0;

			foreach (IParseTree tt in root.EnumChilds())
			{
				Node n = null;

				if (tt is LuaParser.OperatorbinaryContext)
				{
					Operator op = ParseBinaryOperator(tt);
					opfound |= op;
					n = new Node() { Op = op };
				}
				else
				{
					if (tt is LuaParser.Exp_binaryContext)
					{
						Operator op = CreateLinkedList(list, tt, lcontext);
						opfound |= op;
					}
					else
					{
						n = new Node() { Expr = NodeFactory.CreateExpression(tt, lcontext) };
					}
				}

				if (n != null)
				{
					if (list.Nodes == null)
					{
						list.Nodes = list.Last = n;
					}
					else
					{
						list.Last.Next = n;
						n.Prev = list.Last;
						list.Last = n;
					}
				}
			}

			return opfound;
		}


		/// <summary>
		/// Creates a sub tree of binary expressions
		/// </summary>
		public static Expression CreateSubTree(IParseTree tree, ScriptLoadingContext lcontext)
		{
			LinkedList list = new LinkedList();

			Operator opfound = CreateLinkedList(list, tree, lcontext);

			Node nodes = list.Nodes;

			if ((opfound & MUL_DIV_MOD) != 0)
				nodes = PrioritizeLeftAssociative(tree, nodes, lcontext, MUL_DIV_MOD);

			if ((opfound & ADD_SUB) != 0)
				nodes = PrioritizeLeftAssociative(tree, nodes, lcontext, ADD_SUB);

			if ((opfound & STRCAT) != 0)
				nodes = PrioritizeRightAssociative(tree, nodes, lcontext, STRCAT);

			if ((opfound & COMPARES) != 0)
				nodes = PrioritizeLeftAssociative(tree, nodes, lcontext, COMPARES);

			if ((opfound & LOGIC_AND) != 0)
				nodes = PrioritizeLeftAssociative(tree, nodes, lcontext, LOGIC_AND);

			if ((opfound & LOGIC_OR) != 0)
				nodes = PrioritizeLeftAssociative(tree, nodes, lcontext, LOGIC_OR);


			if (nodes.Next != null || nodes.Prev != null)
				throw new InternalErrorException("Expression reduction didn't work! - 1");
			if (nodes.Expr == null)
				throw new InternalErrorException("Expression reduction didn't work! - 2");
			
			return nodes.Expr;
		}

		private static Node PrioritizeLeftAssociative(IParseTree tree, Node nodes, ScriptLoadingContext lcontext, Operator operatorsToFind)
		{
			for (Node N = nodes; N != null; N = N.Next)
			{
				Operator o = N.Op;

				if ((o & operatorsToFind) != 0)
				{
					N.Op = Operator.NotAnOperator;
					N.Expr = new BinaryOperatorExpression(tree, N.Prev.Expr, N.Next.Expr, o, lcontext);
					N.Prev = N.Prev.Prev;
					N.Next = N.Next.Next;

					if (N.Next != null)
						N.Next.Prev = N;

					if (N.Prev != null)
						N.Prev.Next = N;
					else
						nodes = N;
				}
			}

			return nodes;
		}

		private static Node PrioritizeRightAssociative(IParseTree tree, Node nodes, ScriptLoadingContext lcontext, Operator operatorsToFind)
		{
			Node last;
			for (last = nodes; last.Next != null; last = last.Next) ;

			for (Node N = last; N != null; N = N.Prev)
			{
				Operator o = N.Op;

				if ((o & operatorsToFind) != 0)
				{
					N.Op = Operator.NotAnOperator;
					N.Expr = new BinaryOperatorExpression(tree, N.Prev.Expr, N.Next.Expr, o, lcontext);
					N.Prev = N.Prev.Prev;
					N.Next = N.Next.Next;

					if (N.Next != null)
						N.Next.Prev = N;

					if (N.Prev != null)
						N.Prev.Next = N;
					else
						nodes = N;
				}
			}

			return nodes;
		}




		private static Operator ParseBinaryOperator(IParseTree parseTree)
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
				default:
					throw new InternalErrorException("Unexpected binary operator '{0}'", txt);
			}
		}




		Expression m_Exp1, m_Exp2;
		Operator m_Operator;



		private BinaryOperatorExpression(IParseTree tree, Expression exp1, Expression exp2, Operator op, ScriptLoadingContext lcontext)
			: base (tree, lcontext)
		{
			m_Exp1 = exp1;
			m_Exp2 = exp2;
			m_Operator = op;
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
				default:
					throw new InternalErrorException("Unsupported operator {0}", op);
			}
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

		public override DynValue Eval(ScriptExecutionContext context)
		{
			DynValue v1 = m_Exp1.Eval(context).ToScalar();

			if (m_Operator == Operator.Or)
			{
				if (v1.CastToBool())
					return v1;
				else
					return m_Exp2.Eval(context).ToScalar();
			}

			if (m_Operator == Operator.And)
			{
				if (!v1.CastToBool())
					return v1;
				else
					return m_Exp2.Eval(context).ToScalar();
			}

			DynValue v2 = m_Exp2.Eval(context).ToScalar();

			if ((m_Operator & COMPARES) != 0)
			{
				return DynValue.NewBoolean(EvalComparison(v1, v2, m_Operator));				
			}
			else if (m_Operator == Operator.StrConcat)
			{
				string s1 = v1.CastToString();
				string s2 = v2.CastToString();

				if (s1 == null || s2 == null)
					throw new DynamicExpressionException("Attempt to perform concatenation on non-strings.");

				return DynValue.NewString(s1 + s2);
			}
			else
			{
				return DynValue.NewNumber(EvalArithmetic(v1, v2));
			}
		}

		private double EvalArithmetic(DynValue v1, DynValue v2)
		{
			double? nd1 = v1.CastToNumber();
			double? nd2 = v2.CastToNumber();

			if (nd1 == null || nd2 == null)
				throw new DynamicExpressionException("Attempt to perform arithmetic on non-numbers.");

			double d1 = nd1.Value;
			double d2 = nd2.Value;

			switch (m_Operator)
			{
				case Operator.Add:
					return d1 + d2;
				case Operator.Sub:
					return d1 - d2;
				case Operator.Mul:
					return d1 * d2;
				case Operator.Div:
					return d1 / d2;
				case Operator.Mod:
					{
						double mod = Math.IEEERemainder(d1, d2);
						if (mod < 0) mod += d2;
						return mod;
					}
				default:
					throw new DynamicExpressionException("Unsupported operator {0}", m_Operator);
			}
		}

		private bool EvalComparison(DynValue l, DynValue r, Operator op)
		{
			switch (op)
			{
				case Operator.Less:
					if (l.Type == DataType.Number && r.Type == DataType.Number)
					{
						return (l.Number < r.Number);
					}
					else if (l.Type == DataType.String && r.Type == DataType.String)
					{
						return (l.String.CompareTo(r.String) < 0);
					}
					else
					{
						throw new DynamicExpressionException("Attempt to compare non-numbers, non-strings.");
					}
				case Operator.LessOrEqual:
					if (l.Type == DataType.Number && r.Type == DataType.Number)
					{
						return (l.Number <= r.Number);
					}
					else if (l.Type == DataType.String && r.Type == DataType.String)
					{
						return (l.String.CompareTo(r.String) <= 0);
					}
					else
					{
						throw new DynamicExpressionException("Attempt to compare non-numbers, non-strings.");
					}
				case Operator.Equal:
					if (object.ReferenceEquals(r, l))
					{
						return true;
					}
					else if (r.Type != l.Type)
					{
						if ((l.Type == DataType.Nil && r.Type == DataType.Void)
							|| (l.Type == DataType.Void && r.Type == DataType.Nil))
							return true;
						else
							return false;
					}
					else
					{
						return r.Equals(l);
					}
				case Operator.Greater:
					return !EvalComparison(l, r, Operator.LessOrEqual);
				case Operator.GreaterOrEqual:
					return !EvalComparison(l, r, Operator.Less);
				case Operator.NotEqual:
					return !EvalComparison(l, r, Operator.Equal);
				default:
					throw new DynamicExpressionException("Unsupported operator {0}", op);
			}
		}
	}
}
