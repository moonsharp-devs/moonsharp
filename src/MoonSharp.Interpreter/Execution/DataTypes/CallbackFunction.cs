using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public sealed class CallbackFunction
	{
		Func<IExecutionContext, CallbackArguments, RValue> m_CallBack;

		public CallbackFunction(Func<IExecutionContext, CallbackArguments, RValue> callBack)
		{
			m_CallBack = callBack;
		}

		public RValue Invoke(IExecutionContext executionContext, IList<RValue> args)
		{
			return m_CallBack(executionContext, new  CallbackArguments(args));
		}

	}
}
