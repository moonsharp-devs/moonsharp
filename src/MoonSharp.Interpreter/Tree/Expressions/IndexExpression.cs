using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
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

		public override RValue Eval(RuntimeScope scope)
		{
			RValue baseValue = m_BaseExp.Eval(scope).ToSimplestValue();
			RValue indexValue = m_IndexExp.Eval(scope).ToSimplestValue();

			if (baseValue.Type != DataType.Table)
			{
				throw new LuaRuntimeException(this.TreeNode, "Can't index: {0}", baseValue.Type);
			}
			else
			{
				return baseValue.Table[indexValue];
			}
		}

		public void SetValue(RuntimeScope scope, RValue rValue)
		{
			RValue baseValue = m_BaseExp.Eval(scope).ToSimplestValue();
			RValue indexValue = m_IndexExp.Eval(scope).ToSimplestValue();

			baseValue.Table[indexValue] = rValue;
		}



		public void CompileAssignment(Execution.VM.Chunk bc)
		{
			throw new NotImplementedException();
		}
	}
}
