using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree
{
	abstract class NodeBase
	{
		protected internal IParseTree TreeNode { get; private set; }
		protected ScriptLoadingContext LoadingContext { get; private set; }

		protected NodeBase(IParseTree treeNode, ScriptLoadingContext loadingContext)
		{
			TreeNode = treeNode;
			LoadingContext = loadingContext;
		}

		public Exception SyntaxError(string format, params object[] args)
		{
			return new SyntaxErrorException(TreeNode, format, args);
		}

		public void SyntaxAssert(bool condition, string format, params object[] args)
		{
			if (!condition)
				throw  SyntaxError(format, args);
		}

		public abstract void Compile(ByteCode bc);

		protected static SourceRef BuildSourceRef(ScriptLoadingContext lcontext, IToken token, ITerminalNode terminalNode)
		{
			return RegisterSourceRef(lcontext, new SourceRef(lcontext.Source.SourceID, token.Column, token.Column + terminalNode.GetText().Length, token.Line, token.Line, true));
		}

		protected static SourceRef BuildSourceRef(ScriptLoadingContext lcontext, IToken token1, IToken token2 = null)
		{
			token2 = token2 ?? token1;
			return RegisterSourceRef(lcontext, new SourceRef(lcontext.Source.SourceID, token1.Column, token2.Column + token2.Text.Length, token1.Line, token2.Line, true));
		}

		protected static SourceRef BuildSourceRef(ScriptLoadingContext lcontext, ITerminalNode terminalNode)
		{
			return BuildSourceRef(lcontext, terminalNode.Symbol, terminalNode);
		}

		private static SourceRef RegisterSourceRef(ScriptLoadingContext lcontext, SourceRef sourceRef)
		{
			lcontext.Source.Refs.Add(sourceRef);
			return sourceRef;
		}

	}
}
