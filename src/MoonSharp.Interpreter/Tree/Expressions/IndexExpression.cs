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
	class IndexExpression : Expression, IVariable
	{
		Expression m_BaseExp;
		Expression m_IndexExp;

		public IndexExpression(IParseTree node, ScriptLoadingContext lcontext, Expression baseExp, Expression indexExp)
			:base(node, lcontext)
		{
			m_BaseExp = baseExp;
			m_IndexExp = indexExp;
		}



		public override void Compile(Chunk bc)
		{
			m_BaseExp.Compile(bc);
			m_IndexExp.Compile(bc);
			bc.Index();
		}


		public void CompileAssignment(Chunk bc)
		{
			m_BaseExp.Compile(bc);
			m_IndexExp.Compile(bc);
			bc.IndexRef();
		}
	}
}
