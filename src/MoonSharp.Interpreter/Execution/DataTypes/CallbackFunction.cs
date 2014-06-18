using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public sealed class CallbackFunction
	{
		Func<IExecutionContext, CallbackArguments, DynValue> m_CallBack;

		public CallbackFunction(Func<IExecutionContext, CallbackArguments, DynValue> callBack)
		{
			m_CallBack = callBack;
		}

		public DynValue Invoke(IExecutionContext executionContext, IList<DynValue> args)
		{
			return m_CallBack(executionContext, new  CallbackArguments(args));
		}

	}
}
