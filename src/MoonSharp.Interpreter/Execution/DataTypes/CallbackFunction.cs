using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public sealed class CallbackFunction
	{
		Func<RValue[], RValue> m_CallBack;

		public CallbackFunction(Func<RValue[], RValue> callBack)
		{
			m_CallBack = callBack;
		}

		public RValue Invoke(RValue[] args)
		{
			return m_CallBack(args);
		}

	}
}
