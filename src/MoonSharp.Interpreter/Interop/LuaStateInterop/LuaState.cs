using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop.LuaStateInterop
{
	/// <summary>
	/// 
	/// </summary>
	public class LuaState
	{
		private List<DynValue> m_Stack;

		public ScriptExecutionContext ExecutionContext { get; private set; }
		public string FunctionName { get; private set; }

		internal LuaState(ScriptExecutionContext executionContext, CallbackArguments args, string functionName)
		{
			ExecutionContext = executionContext;
			m_Stack = new List<DynValue>(16);

			for (int i = 0; i < args.Count; i++)
				m_Stack.Add(args[i]);

			FunctionName = functionName;
		}

		public DynValue Top(int pos = 0)
		{
			return m_Stack[m_Stack.Count - 1 - pos];
		}

		public DynValue At(int pos)
		{
			return m_Stack[pos - 1];
		}

		public int Count
		{
			get { return m_Stack.Count; }
		}

		public void Push(DynValue v)
		{
			m_Stack.Add(v);
		}

		public DynValue Pop()
		{
			var v = Top();
			m_Stack.RemoveAt(m_Stack.Count - 1);
			return v;
		}

		public DynValue GetReturnValue(int retvals)
		{
			if (retvals == 0)
				return DynValue.Nil;
			else if (retvals == 1)
				return Top();
			else
			{
				DynValue[] rets = new DynValue[retvals];

				for (int i = 0; i < retvals; i++)
					rets[retvals - i - 1] = Top(i);

				return DynValue.NewTupleNested(rets);
			}
		}


	}
}
