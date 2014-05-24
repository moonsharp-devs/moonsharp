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

namespace MoonSharp.Interpreter.Tree
{
	abstract class Statement : NodeBase
	{
		public Statement(IParseTree tree, ScriptLoadingContext lcontext)
			: base(tree, lcontext)
		{ }

		public abstract ExecutionFlow Exec(RuntimeScope scope);


		public RValue GetReturnValueAtReturnPoint(ExecutionFlow flow)
		{
			if (flow.Type == ExecutionFlowType.Return)
			{
				return flow.ReturnValue.ToSimplestValue();
			}
			else if (flow.Type == ExecutionFlowType.None)
			{
				return RValue.Nil;
			}
			else
			{
				throw RuntimeError("Can't process flow control '{0}'.", flow.Type.ToString().ToLower());
			}
		}

		public static ExecutionFlow PairMultipleAssignment<T>(RuntimeScope scope, T[] lValues, Expression[] rValues, Action<T, RuntimeScope, RValue> setter)
		{
			int li = 0;

			for (int ri = 0; ri < rValues.Length && li < lValues.Length; ri++, li++)
			{
				RValue vv = rValues[ri].Eval(scope);

				if ((ri != rValues.Length - 1)||(vv.Type != DataType.Tuple))
				{
					setter(lValues[li], scope, vv.ToSingleValue());
					// Debug.WriteLine(string.Format("{0} <- {1}", li, vv.ToSingleValue()));
				}
				else
				{
					for (int rri = 0; rri < vv.Tuple.Length && li < lValues.Length; rri++, li++)
					{
						setter(lValues[li], scope, vv.Tuple[rri].ToSingleValue());
						// Debug.WriteLine(string.Format("{0} <- {1}", li, vv.Tuple[rri].ToSingleValue()));
					}
				}
			}

			return ExecutionFlow.None;
		}

		protected ExecutionFlow ExecuteStatementInBlockScope(Statement s, RuntimeScope scope, RuntimeScopeFrame stackframe, SymbolRef symb = null, RValue varvalue = null)
		{
			scope.PushFrame(stackframe);

			if (symb != null && varvalue != null)
				scope.Assign(symb, varvalue);

			ExecutionFlow flow = s.Exec(scope);

			scope.PopFrame(stackframe);

			return flow;
		}



	}



}
