using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Expressions
{
	class TableConstructor : Expression 
	{
		List<Expression> m_PositionalValues = new List<Expression>();
		List<KeyValuePair<Expression, Expression>> m_CtorArgs = new List<KeyValuePair<Expression, Expression>>();

		public TableConstructor(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			// here lexer is at the '{', go on
			CheckTokenType(lcontext, TokenType.Brk_Open_Curly);

			while (lcontext.Lexer.Current.Type != TokenType.Brk_Close_Curly)
			{
				switch (lcontext.Lexer.Current.Type)
				{
					case TokenType.Name:
						{
							Token assign = lcontext.Lexer.PeekNext();

							if (assign.Type == TokenType.Op_Assignment)
								StructField(lcontext);
							else
								ArrayField(lcontext);
						}
						break;
					case TokenType.Brk_Open_Square:
						MapField(lcontext);
						break;
					default:
						ArrayField(lcontext);
						break;
				}

				Token curr = lcontext.Lexer.Current;

				if (curr.Type == TokenType.Comma || curr.Type == TokenType.SemiColon)
				{
					lcontext.Lexer.Next();
				}
				else
				{
					CheckTokenType(lcontext, TokenType.Brk_Close_Curly);
					break;
				}
			}

			if (lcontext.Lexer.Current.Type == TokenType.Brk_Close_Curly)
				lcontext.Lexer.Next();
		}

		private void MapField(ScriptLoadingContext lcontext)
		{
			lcontext.Lexer.Next(); // skip '['

			Expression key = Expr(lcontext);

			CheckTokenType(lcontext, TokenType.Brk_Close_Square);

			CheckTokenType(lcontext, TokenType.Op_Assignment);

			Expression value = Expr(lcontext);

			m_CtorArgs.Add(new KeyValuePair<Expression, Expression>(key, value));
		}

		private void StructField(ScriptLoadingContext lcontext)
		{
			Expression key = new LiteralExpression(lcontext, DynValue.NewString(lcontext.Lexer.Current.Text));
			lcontext.Lexer.Next();

			CheckTokenType(lcontext, TokenType.Op_Assignment);

			Expression value = Expr(lcontext);

			m_CtorArgs.Add(new KeyValuePair<Expression, Expression>(key, value));
		}


		private void ArrayField(ScriptLoadingContext lcontext)
		{
			Expression e = Expr(lcontext);
			m_PositionalValues.Add(e);
		}


		public TableConstructor(LuaParser.TableconstructorContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			var fieldlist = context.fieldlist();

			if (fieldlist != null)
			{
				foreach (var field in fieldlist.field())
				{
					var keyval = field.keyexp;
					var name = field.NAME();

					if (keyval != null)
					{
						Expression exp = NodeFactory.CreateExpression(keyval, lcontext);

						m_CtorArgs.Add(new KeyValuePair<Expression,Expression>(
							exp,
							NodeFactory.CreateExpression(field.keyedexp, lcontext)));
					}
					else if (name != null)
					{
						m_CtorArgs.Add(new KeyValuePair<Expression, Expression>(
							new ANTLR_LiteralExpression(field, lcontext, DynValue.NewString(name.GetText())),
							NodeFactory.CreateExpression(field.namedexp, lcontext)));
					}
					else 
					{
						m_PositionalValues.Add(NodeFactory.CreateExpression(field.positionalexp, lcontext));
					}
				}

			}
		}



		public override void Compile(Execution.VM.ByteCode bc)
		{
			bc.Emit_NewTable();

			foreach (var kvp in m_CtorArgs)
			{
				kvp.Key.Compile(bc);
				kvp.Value.Compile(bc);
				bc.Emit_TblInitN();
			}

			for (int i = 0; i < m_PositionalValues.Count; i++ )
			{
				m_PositionalValues[i].Compile(bc);
				bc.Emit_TblInitI(i == m_PositionalValues.Count - 1);
			}
		}


		public override DynValue Eval(ScriptExecutionContext context)
		{
			throw new DynamicExpressionException("Dynamic Expressions cannot define new tables.");
		}
	}
}
