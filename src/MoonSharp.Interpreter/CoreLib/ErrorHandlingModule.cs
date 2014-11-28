using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule]
	public class ErrorHandlingModule
	{
		[MoonSharpMethod]
		public static DynValue pcall(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args[0];
			DynValue[] a = new DynValue[args.Count - 1];

			for (int i = 1; i < args.Count; i++)
				a[i - 1] = args[i];

			if (args[0].Type == DataType.ClrFunction)
			{
				try
				{
					DynValue ret = args[0].Callback.Invoke(executionContext, a);
					if (ret.Type == DataType.TailCallRequest)
					{
						if (ret.TailCallData.Continuation != null || ret.TailCallData.ErrorHandler != null)
							throw new ScriptRuntimeException("the function passed to pcall cannot be called directly by pcall. wrap in a script function instead.");

						return DynValue.NewTailCallReq(new TailCallData()
						{
							Args = ret.TailCallData.Args,
							Function = ret.TailCallData.Function,
							Continuation = new CallbackFunction(pcall_continuation),
							ErrorHandler = new CallbackFunction(pcall_onerror)
						});
					}
					else if (ret.Type == DataType.YieldRequest)
					{
						throw new ScriptRuntimeException("the function passed to pcall cannot be called directly by pcall. wrap in a script function instead.");
					}
					else
					{
						return DynValue.NewTupleNested(DynValue.True, ret);
					}
				}
				catch (ScriptRuntimeException ex)
				{
					return DynValue.NewTupleNested(DynValue.False, DynValue.NewString(ex.Message));
				}
			}
			else if (args[0].Type != DataType.Function)
			{
				return DynValue.NewTupleNested(DynValue.False, DynValue.NewString("attempt to pcall a non-function"));
			}
			else
			{
				return DynValue.NewTailCallReq(new TailCallData()
				{
					Args = a,
					Function = v,
					Continuation = new CallbackFunction(pcall_continuation),
					ErrorHandler = new CallbackFunction(pcall_onerror)
				});
			}
		}

		public static DynValue pcall_continuation(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return DynValue.NewTupleNested(DynValue.True, args[0]);
		}

		public static DynValue pcall_onerror(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return DynValue.NewTupleNested(DynValue.False, args[0]);
		}


		[MoonSharpMethod]
		public static DynValue xpcall(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			List<DynValue> a = new List<DynValue>();

			for (int i = 0; i < args.Count; i++)
			{
				if (i != 1)
					a.Add(args[i]);
			}

			return pcall(executionContext, new CallbackArguments(a, false));
		}

	}
}
