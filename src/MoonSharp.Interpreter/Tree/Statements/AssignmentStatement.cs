using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


		public AssignmentStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			while (true)
			{
				Token name = CheckTokenType(lcontext, TokenType.Name);

				var localVar = lcontext.Scope.TryDefineLocal(name.Text);
				var symbol = new SymbolRefExpression(lcontext, localVar);
				m_LValues.Add(symbol);

				if (lcontext.Lexer.Current.Type != TokenType.Comma)
					break;

				lcontext.Lexer.Next();
			}

			CheckTokenType(lcontext, TokenType.Op_Assignment);

			m_RValues = Expression.ExprList(lcontext);
		}


		public AssignmentStatement(ScriptLoadingContext lcontext, Expression firstExpression)
			: base(lcontext)
		{
			m_LValues.Add(CheckVar(lcontext, firstExpression));

			while (lcontext.Lexer.Current.Type == TokenType.Comma)
			{
				lcontext.Lexer.Next();
				Expression e = Expression.PrimaryExp(lcontext);
				m_LValues.Add(CheckVar(lcontext, e));
			}

			CheckTokenType(lcontext, TokenType.Op_Assignment);

			m_RValues = Expression.ExprList(lcontext);
		}

		private IVariable CheckVar(ScriptLoadingContext lcontext, Expression firstExpression)
		{
			IVariable v = firstExpression as IVariable;

			if (v == null)
				throw new SyntaxErrorException("unexpected symbol near '{0}' - not a l-value", lcontext.Lexer.Current);

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
