using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class FunctionCallExpression : Expression
	{
		List<Expression> m_Arguments;
		Expression m_Function;
		string m_Name;
		string m_DebugErr;


		public FunctionCallExpression(ScriptLoadingContext lcontext, Expression function, Token thisCallName)
			: base(lcontext)
		{
			m_Name = thisCallName != null ? thisCallName.Text : null;
			m_DebugErr = function.GetFriendlyDebugName();
			m_Function = function;

			switch (lcontext.Lexer.Current.Type)
			{
				case TokenType.Brk_Open_Round:
					lcontext.Lexer.Next();
					Token t = lcontext.Lexer.Current;
					if (t.Type == TokenType.Brk_Close_Round)
					{
						m_Arguments = new List<Expression>();
						lcontext.Lexer.Next();
					}
					else
					{
						m_Arguments = ExprList(lcontext);
						CheckMatch(lcontext, "(", TokenType.Brk_Close_Round);
					}
					break;
				case TokenType.String:
				case TokenType.String_Long:
					{
						m_Arguments = new List<Expression>();
						Expression le = new LiteralExpression(lcontext, lcontext.Lexer.Current);
						lcontext.Lexer.Next();
						m_Arguments.Add(le);
					}
					break;
				case TokenType.Brk_Open_Curly:
					{
						m_Arguments = new List<Expression>();
						m_Arguments.Add(new TableConstructor(lcontext));
					}
					break;
				default:
					throw new SyntaxErrorException("function arguments expected");
			}
		}

		public override void Compile(Execution.VM.ByteCode bc)
		{
			m_Function.Compile(bc);

			int argslen = m_Arguments.Count;

			if (!string.IsNullOrEmpty(m_Name))
			{
				bc.Emit_Copy(0);
				bc.Emit_Literal(DynValue.NewString(m_Name));
				bc.Emit_Index();
				bc.Emit_Swap(0, 1);
				++argslen;
			}

			for (int i = 0; i < m_Arguments.Count; i++)
				m_Arguments[i].Compile(bc);

			if (!string.IsNullOrEmpty(m_Name))
			{
				bc.Emit_ThisCall(argslen, m_DebugErr);
			}
			else
			{
				bc.Emit_Call(argslen, m_DebugErr);
			}
		}

		public override DynValue Eval(ScriptExecutionContext context)
		{
			throw new DynamicExpressionException("Dynamic Expressions cannot call functions.");
		}

	}
}
