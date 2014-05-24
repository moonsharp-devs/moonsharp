using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Grammar;

namespace MoonSharp.Interpreter.Tree.Statements
{
	class ForEachLoopStatement : Statement
	{
		RuntimeScopeFrame m_StackFrame;
		SymbolRef[] m_Names;
		Expression[] m_RValues;
		Statement m_Block;


		public ForEachLoopStatement(LuaParser.Stat_foreachloopContext context, ScriptLoadingContext lcontext)
			: base(context, lcontext)
		{
			context.explist();

			var explist = context.explist();

			m_RValues = explist
			.exp()
			.Select(e => NodeFactory.CreateExpression(e, lcontext))
			.ToArray();

			lcontext.Scope.PushBlock();

			m_Names = context.namelist().NAME()
				.Select(n => n.GetText())
				.Select(n => lcontext.Scope.DefineLocal(n))
				.ToArray();
			
			m_Block = NodeFactory.CreateStatement(context.block(), lcontext);

			m_StackFrame = lcontext.Scope.Pop();
		}




		public override ExecutionFlow Exec(RuntimeScope scope)
		{
			List<RValue> values = m_RValues
				.Select(r => r.Eval(scope))
				.SelectMany(r => r.UnpackedTuple())
				.ToList();

			while (values.Count < 3)
				values.Add(RValue.Nil);

			RValue F = values[0];
			RValue[] args = new RValue[2];

			args[0] = values[1];
			args[1] = values[2];


			while (true)
			{
				if (F.Type != DataType.ClrFunction)
					throw RuntimeError("Attempt to call non-function");

				RValue tuple = F.Callback.Invoke(scope, args);

				scope.PushFrame(m_StackFrame);

				int iv = 0;
				RValue firstVal = null;

				foreach (var vv in tuple.UnpackedTuple())
				{
					if (iv >= m_Names.Length)
						break;

					scope.Assign(m_Names[iv], vv);

					if (firstVal == null)
						args[1] = firstVal = vv;

					++iv;
				}


				while (iv < m_Names.Length)
				{
					scope.Assign(m_Names[iv], RValue.Nil);
					++iv;
				}

				if (firstVal == null || firstVal.Type == DataType.Nil)
				{
					scope.PopFrame(m_StackFrame);
					return ExecutionFlow.None;
				}

				ExecutionFlow flow = m_Block.Exec(scope);

				scope.PopFrame(m_StackFrame);

				if (flow.Type == ExecutionFlowType.Return)
					return flow;

				if (flow.Type == ExecutionFlowType.Break)
					return ExecutionFlow.None;
			}
		}
	}
}
