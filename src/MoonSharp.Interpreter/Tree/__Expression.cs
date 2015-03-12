using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree
{
	abstract class Expression : NodeBase
	{
		protected Expression(IParseTree node, ScriptLoadingContext lcontext)
			: base(node, lcontext)
		{ }

		public Expression(ScriptLoadingContext lcontext)
			: base(null, lcontext)
		{ }

		public virtual string GetFriendlyDebugName()
		{
			return null;
		}

		public abstract DynValue Eval(ScriptExecutionContext context);

		public virtual SymbolRef FindDynamic(ScriptExecutionContext context)
		{
			return null;
		}


		internal static List<Expression> ExprList(ScriptLoadingContext lcontext)
		{
			List<Expression> exps = new List<Expression>();

			while(true)
			{
				exps.Add(Expr(lcontext));

				if (lcontext.Lexer.Current().Type != TokenType.Comma)
					break;

				lcontext.Lexer.Next();
			} 

			return exps; //+++
		}

		internal static Expression Expr(ScriptLoadingContext lcontext)
		{
			return SubExpr(lcontext, true);
		}

		internal static Expression SubExpr(ScriptLoadingContext lcontext, bool isPrimary)
		{
			Expression e = null;

			Token T = lcontext.Lexer.Current();

			if (T.IsUnaryOperator())
			{
				lcontext.Lexer.Next();
				e = SubExpr(lcontext, false);

				// check for power operator "damnedness"
				Token unaryOp = T;
				T = lcontext.Lexer.Current();

				if (isPrimary && T.Type == TokenType.Op_Pwr)
				{
					List<Expression> powerChain = new List<Expression>();
					powerChain.Add(e);

					while (isPrimary && T.Type == TokenType.Op_Pwr)
					{
						lcontext.Lexer.Next();
						powerChain.Add(SubExpr(lcontext, false));
						T = lcontext.Lexer.Current();
					}

					e = powerChain[powerChain.Count - 1];

					for (int i = powerChain.Count - 2; i >= 0; i--)
					{
						e = new PowerOperatorExpression(powerChain[i], e, lcontext);
					}
				}

				e = new UnaryOperatorExpression(lcontext, e, unaryOp);
			}
			else
			{
				e = SimpleExp(lcontext);
			}

			T = lcontext.Lexer.Current();

			if (isPrimary && T.IsBinaryOperator())
			{
				object chain = BinaryOperatorExpression.BeginOperatorChain();

				BinaryOperatorExpression.AddExpressionToChain(chain, e);

				while (T.IsBinaryOperator())
				{
					BinaryOperatorExpression.AddOperatorToChain(chain, T);
					lcontext.Lexer.Next();
					Expression right = SubExpr(lcontext, false);
					BinaryOperatorExpression.AddExpressionToChain(chain, right);
					T = lcontext.Lexer.Current();
				}

				e = BinaryOperatorExpression.CommitOperatorChain(chain, lcontext);
			}

			return e;
		}

		internal static Expression SimpleExp(ScriptLoadingContext lcontext)
		{
			Token t = lcontext.Lexer.Current();

			switch (t.Type)
			{
				case TokenType.Number:
				case TokenType.Number_Hex:
				case TokenType.Number_HexFloat:
				case TokenType.String:
				case TokenType.String_Long:
				case TokenType.Nil:
				case TokenType.True:
				case TokenType.False:
					lcontext.Lexer.Next();
					return new LiteralExpression(lcontext, t);
				case TokenType.VarArgs:
					lcontext.Lexer.Next();
					return new SymbolRefExpression(t, lcontext);
				case TokenType.Brk_Open_Curly:
					{
						Expression tc = new TableConstructor(lcontext);
						CheckMatch(lcontext, "{", TokenType.Brk_Close_Curly);
						return tc;
					}
				case TokenType.Function:
					throw new NotImplementedException();
				default:
					return PrimaryExp(lcontext);
			}

		}

		private static Expression PrimaryExp(ScriptLoadingContext lcontext)
		{
			Expression e = PrefixExp(lcontext);

			while (true)
			{
				Token T = lcontext.Lexer.Current();
				Token thisCallName = null;

				switch (T.Type)
				{
					case TokenType.Dot:
						{
							Token name = lcontext.Lexer.Next();
							CheckTokenType(name, TokenType.Name);
							LiteralExpression le = new LiteralExpression(lcontext, DynValue.NewString(name.Text));
							lcontext.Lexer.Next();
							return new IndexExpression(e, le, lcontext);
						}
					case TokenType.Brk_Open_Square:
						{
							lcontext.Lexer.Next(); // skip bracket
							Expression index = Expr(lcontext);
							CheckMatch(lcontext, T.Text, TokenType.Brk_Close_Square);
							return new IndexExpression(e, index, lcontext);
						}
					case TokenType.Colon:
							thisCallName = lcontext.Lexer.Next();
							CheckTokenType(thisCallName, TokenType.Name);
							lcontext.Lexer.Next();
							goto case TokenType.Brk_Open_Round;
					case TokenType.Brk_Open_Round:
					case TokenType.String:
					case TokenType.String_Long:
					case TokenType.Brk_Open_Curly:
							return new FunctionCallExpression(lcontext, e, thisCallName);
					default: 
						return e;
				}
			}
		}

		private static void CheckTokenType(Token t, TokenType tokenType)
		{
			if (t.Type != tokenType)
				throw new SyntaxErrorException("Unexpected token '{0}'", t.Text);
		}

		private static Expression PrefixExp(ScriptLoadingContext lcontext)
		{
			Token T = lcontext.Lexer.Current();
			switch (T.Type)
			{
				case TokenType.Brk_Open_Round:
					lcontext.Lexer.Next();
					Expression e = Expr(lcontext);
					CheckMatch(lcontext, T.Text, TokenType.Brk_Close_Round);
					return e;
				case TokenType.Name:
					lcontext.Lexer.Next();
					return new SymbolRefExpression(T, lcontext);
				default:
					throw new SyntaxErrorException("unexpected symbol near '{0}'", T.Text);
			}
		}

		protected static void CheckMatch(ScriptLoadingContext lcontext, string tokenDesc, TokenType tokenType)
		{
			if (lcontext.Lexer.Current().Type != tokenType)
				throw new SyntaxErrorException("Mismatched '{0}' near '{1}'", tokenDesc, lcontext.Lexer.Current().Text);

			lcontext.Lexer.Next();
		}

		private static Expression SingleVar(ScriptLoadingContext lcontext)
		{
			throw new NotImplementedException();
		}

	}
}
