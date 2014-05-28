using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Antlr4.Runtime.Tree;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;
using MoonSharp.Interpreter.Grammar;
using MoonSharp.Interpreter.Tree.Expressions;

namespace MoonSharp.Interpreter.Tree
{
	class FunctionCall : NodeBase
	{
		Expression[] m_Arguments;
		string m_Name;

		public FunctionCall(LuaParser.NameAndArgsContext nameAndArgs, ScriptLoadingContext lcontext)
			: base(nameAndArgs, lcontext)
		{
			var name = nameAndArgs.NAME();

			m_Name = name != null ? name.GetText().Trim() : null;

			m_Arguments = nameAndArgs.args().children.SelectMany(t => NodeFactory.CreateExpressions(t, lcontext)).Where(t => t != null).ToArray();
		}

		public RValue Invoke(RuntimeScope scope, RValue value)
		{
			RValue[] args;

			if (value.Type == DataType.Table)
			{
				var method = value.Table[new RValue(m_Name)];

				args = new RValue[] { value }.Union(
						m_Arguments
						.Select(exp => exp.Eval(scope))
						.SelectMany(val => val.ToArrayOfValues())
					)
					.ToArray();

				value = method;
			}
			else
			{
				args = m_Arguments
					.Select(exp => exp.Eval(scope))
					.SelectMany(val => val.ToArrayOfValues())
					.ToArray();
			}

			if (value.Type == DataType.ClrFunction)
			{
				return value.Callback.Invoke(scope, args);
			}
			else
			{
				throw RuntimeError("Function was expected, but a {0} was passed.", value.Type.ToString());
			}
		}


		public override void Compile(Execution.VM.Chunk bc)
		{
			if (!string.IsNullOrEmpty(m_Name))
			{
				bc.TempOp(OpCode.TmpPeek, 0);
				bc.Literal(new RValue(m_Name));
				bc.Index();
			}


			for (int i = 0; i < m_Arguments.Length; i++)
				m_Arguments[i].Compile(bc);

			bc.Reverse(m_Arguments.Length);

			if (string.IsNullOrEmpty(m_Name))
			{
				bc.Call(m_Arguments.Length);
			}
			else
			{
				bc.TempOp(OpCode.TmpPush, 0);
				bc.TempOp(OpCode.TmpClear, 0);
				bc.Call(m_Arguments.Length + 1);
			}
		}
	}
}
