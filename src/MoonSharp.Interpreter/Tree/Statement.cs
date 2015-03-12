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
			Token tkn = lcontext.Lexer.Current();

			forceLast = false;

			switch (tkn.Type)
			{
				case TokenType.If:
					throw new NotImplementedException();
				case TokenType.While:
					throw new NotImplementedException();
				case TokenType.Do:
					throw new NotImplementedException();
				case TokenType.For:
					throw new NotImplementedException();
				case TokenType.Repeat:
					throw new NotImplementedException();
				case TokenType.Function:
					throw new NotImplementedException();
				case TokenType.Local:
					throw new NotImplementedException();
				case TokenType.Return:
					forceLast = true;
					return new ReturnStatement(lcontext);
				case TokenType.Break:
					forceLast = true;
					throw new NotImplementedException();
				default:
					throw new NotImplementedException();
			}
		}


	}



}
