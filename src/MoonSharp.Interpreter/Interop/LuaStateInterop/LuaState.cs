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
		public ScriptExecutionContext ExecutionContext { get; private set; }

		public FastStackDynamic<DynValue> Stack { get; private set; }

		public string FunctionName { get; private set; }

		internal LuaState(ScriptExecutionContext executionContext, CallbackArguments args, string functionName)
		{
			ExecutionContext = executionContext;
			Stack = new FastStackDynamic<DynValue>(16);

			for (int i = 0; i < args.Count; i++)
				Stack.Push(args[i]);

			FunctionName = functionName;
		}

		public DynValue GetReturnValue(int retvals)
		{
			if (retvals == 0)
				return DynValue.Nil;
			else if (retvals == 1)
				return Stack.Peek(0);
			else
			{
				DynValue[] rets = new DynValue[retvals];

				for (int i = 0; i < retvals; i++)
					rets[retvals - i - 1] = Stack.Peek(i);

				return DynValue.NewTupleNested(rets);
			}
		}


	}
}
