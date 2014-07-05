using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter
{
	public sealed class CallbackFunction
	{
		Func<ScriptExecutionContext, CallbackArguments, DynValue> m_CallBack;
		public Table Closure { get; set; }

		public CallbackFunction(Func<ScriptExecutionContext, CallbackArguments, DynValue> callBack)
		{
			m_CallBack = callBack;
		}

		public DynValue Invoke(ScriptExecutionContext executionContext, IList<DynValue> args)
		{
			return m_CallBack(executionContext, new  CallbackArguments(args));
		}

	}
}
