using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Interop;

namespace MoonSharp.Interpreter
{
	public sealed class CallbackFunction
	{
		Func<ScriptExecutionContext, CallbackArguments, DynValue> m_CallBack;
		public Table Closure { get; set; }
		private static InteropAccessMode m_DefaultAccessMode = InteropAccessMode.LazyOptimized;

		public CallbackFunction(Func<ScriptExecutionContext, CallbackArguments, DynValue> callBack)
		{
			m_CallBack = callBack;
		}

		public DynValue Invoke(ScriptExecutionContext executionContext, IList<DynValue> args)
		{
			return m_CallBack(executionContext, new  CallbackArguments(args));
		}

		public static InteropAccessMode DefaultAccessMode
		{
			get { return m_DefaultAccessMode; }
			set
			{
				if (value == InteropAccessMode.Default || value == InteropAccessMode.HideMembers || value == InteropAccessMode.BackgroundOptimized)
					throw new ArgumentException("DefaultAccessMode");

				m_DefaultAccessMode = value;
			}
		}

		public static CallbackFunction FromDelegate(Script script, Delegate del, InteropAccessMode accessMode = InteropAccessMode.Default)
		{
			if (accessMode == InteropAccessMode.Default)
				accessMode = m_DefaultAccessMode;

			UserDataMethodDescriptor descr = new UserDataMethodDescriptor(del.Method, accessMode);
			return new CallbackFunction(descr.GetCallback(script, del.Target));
		}


		public static CallbackFunction FromMethodInfo(Script script, System.Reflection.MethodInfo mi, InteropAccessMode accessMode = InteropAccessMode.Default)
		{
			if (accessMode == InteropAccessMode.Default)
				accessMode = m_DefaultAccessMode;

			UserDataMethodDescriptor descr = new UserDataMethodDescriptor(mi, accessMode);
			return new CallbackFunction(descr.GetCallback(script, null));
		}
	}
}
