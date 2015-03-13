using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class FunctionCallStatement : Statement
	{
		Expression m_FunctionCallExpression;

		public FunctionCallStatement(ScriptLoadingContext lcontext, Expression functionCallExpression)
			: base(lcontext)
		{
			m_FunctionCallExpression = functionCallExpression;
		}


		public override void Compile(ByteCode bc)
		{
			m_FunctionCallExpression.Compile(bc);
			bc.Emit_Pop();
		}
	}
}
