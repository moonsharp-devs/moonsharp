using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;

using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class AssignmentStatement : Statement
	{
		List<IVariable> m_LValues = new List<IVariable>();
		List<Expression> m_RValues;
		SourceRef m_Ref;


		public AssignmentStatement(ScriptLoadingContext lcontext, Token startToken)
			: base(lcontext)
		{
			List<string> names = new List<string>();

			Token first = startToken;

			while (true)
			{
				Token name = CheckTokenType(lcontext, TokenType.Name);
				names.Add(name.Text);

				if (lcontext.Lexer.Current.Type != TokenType.Comma)
					break;

				lcontext.Lexer.Next();
			}

			if (lcontext.Lexer.Current.Type == TokenType.Op_Assignment)
			{
				CheckTokenType(lcontext, TokenType.Op_Assignment);
				m_RValues = Expression.ExprList(lcontext);
			}
			else
			{
				m_RValues = new List<Expression>();
			}

			foreach (string name in names)
			{
				var localVar = lcontext.Scope.TryDefineLocal(name);
				var symbol = new SymbolRefExpression(lcontext, localVar);
				m_LValues.Add(symbol);
			}

			Token last = lcontext.Lexer.Current;
			m_Ref = first.GetSourceRefUpTo(last);
			lcontext.Source.Refs.Add(m_Ref);

		}


		public AssignmentStatement(ScriptLoadingContext lcontext, Expression firstExpression, Token first)
			: base(lcontext)
		{
			m_LValues.Add(CheckVar(lcontext, firstExpression));

			while (lcontext.Lexer.Current.Type == TokenType.Comma)
			{
				lcontext.Lexer.Next();
				Expression e = Expression.PrimaryExp(lcontext);
				m_LValues.Add(CheckVar(lcontext, e));
			}

			string assignmentType = lcontext.Lexer.Current.Text;

			CheckTokenType(lcontext, TokenType.Op_Assignment);

			m_RValues = Expression.ExprList(lcontext);

			// Replace e.g. "a += b" with "a = a + b"
			if (assignmentType != "=")
			{
				TokenType ArithmeticOperation = assignmentType switch
				{
					"+=" => TokenType.Op_Add,
					"-=" => TokenType.Op_MinusOrSub,
					"*=" => TokenType.Op_Mul,
					"/=" => TokenType.Op_Div,
					"%=" => TokenType.Op_Mod,
					"^=" => TokenType.Op_Pwr,
					"..=" => TokenType.Op_Concat,
					_ => throw new InternalErrorException($"Assignment operator not recognised: {assignmentType}"),
				};

				object operatorChain = BinaryOperatorExpression.BeginOperatorChain();
				BinaryOperatorExpression.AddExpressionToChain(operatorChain, firstExpression);
				BinaryOperatorExpression.AddOperatorToChain(operatorChain, new Token(ArithmeticOperation, first.SourceId, first.FromLine, first.FromCol, first.ToLine, first.ToCol, first.PrevLine, first.PrevCol));
				BinaryOperatorExpression.AddExpressionToChain(operatorChain, m_RValues[0]);
				m_RValues[0] = BinaryOperatorExpression.CommitOperatorChain(operatorChain, lcontext);
			}

			Token last = lcontext.Lexer.Current;
			m_Ref = first.GetSourceRefUpTo(last);
			lcontext.Source.Refs.Add(m_Ref);
		}

		private IVariable CheckVar(ScriptLoadingContext lcontext, Expression firstExpression)
		{
			IVariable v = firstExpression as IVariable;

			if (v == null)
				throw new SyntaxErrorException(lcontext.Lexer.Current, "unexpected symbol near '{0}' - not a l-value", lcontext.Lexer.Current);

			return v;
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			using (bc.EnterSource(m_Ref))
			{
				foreach (var exp in m_RValues)
				{
					exp.Compile(bc);
				}

				for (int i = 0; i < m_LValues.Count; i++)
					m_LValues[i].CompileAssignment(bc,
							Math.Max(m_RValues.Count - 1 - i, 0), // index of r-value
							i - Math.Min(i, m_RValues.Count - 1)); // index in last tuple

				bc.Emit_Pop(m_RValues.Count);
			}
		}
	}
}
