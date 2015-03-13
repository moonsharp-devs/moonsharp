using System;
using System.Collections.Generic;
using MoonSharp.Interpreter.Diagnostics;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Statements;
using MoonSharp.Interpreter.Debugging;
using Antlr4.Runtime;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree
{
	abstract class Statement : NodeBase
	{
		public Statement(IParseTree tree, ScriptLoadingContext lcontext)
			: base(tree, lcontext)
		{ }

		public Statement(ScriptLoadingContext lcontext)
			: base(null, lcontext)
		{ }


		protected static Statement CreateStatement(ScriptLoadingContext lcontext, out bool forceLast)
		{
			Token tkn = lcontext.Lexer.Current;

			forceLast = false;

			switch (tkn.Type)
			{
				case TokenType.SemiColon:
					lcontext.Lexer.Next();
					return new EmptyStatement(lcontext);
				case TokenType.If:
					return new IfStatement(lcontext);
				case TokenType.While:
					return new WhileStatement(lcontext);
				case TokenType.Do:
					return new ScopeBlockStatement(lcontext);
				case TokenType.For:
					return DispatchForLoopStatement(lcontext);
				case TokenType.Repeat:
					return new RepeatStatement(lcontext);
				case TokenType.Function:
					return new FunctionDefinitionStatement(lcontext, false);
				case TokenType.Local:
					lcontext.Lexer.Next();
					if (lcontext.Lexer.Current.Type == TokenType.Function)
						return new FunctionDefinitionStatement(lcontext, true);
					else
						return new AssignmentStatement(lcontext);
				case TokenType.Return:
					forceLast = true;
					return new ReturnStatement(lcontext);
				case TokenType.Break:
					forceLast = true;
					return new BreakStatement(lcontext);
				default:
					{
						Expression exp = Expression.PrimaryExp(lcontext);

						if (exp is FunctionCallExpression)
							return new FunctionCallStatement(lcontext, exp);
						else
							return new AssignmentStatement(lcontext, exp);
					}
			}
		}

		private static Statement DispatchForLoopStatement(ScriptLoadingContext lcontext)
		{
			//	for Name ‘=’ exp ‘,’ exp [‘,’ exp] do block end | 
			//	for namelist in explist do block end | 		

			CheckTokenType(lcontext, TokenType.For);

			Token name = CheckTokenType(lcontext, TokenType.Name);

			if (lcontext.Lexer.Current.Type == TokenType.Op_Assignment)
				return new ForLoopStatement(lcontext, name);
			else
				return new ForEachLoopStatement(lcontext, name);
		}




	}



}
