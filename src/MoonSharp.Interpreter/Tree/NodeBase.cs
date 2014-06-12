using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
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

		public Exception RuntimeError(string format, params object[] args)
		{
			return new ScriptRuntimeException(TreeNode, format, args);
		}

		public void SyntaxAssert(bool condition, string format, params object[] args)
		{
			if (!condition)
				throw  SyntaxError(format, args);
		}

		public void RuntimeAssert(bool condition, string format, params object[] args)
		{
			if (!condition)
				throw  RuntimeError(format, args);
		}

		public abstract void Compile(Chunk bc);




	}
}
