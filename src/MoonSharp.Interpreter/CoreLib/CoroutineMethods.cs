using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Execution.VM;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace = "coroutine")]
	public class CoroutineMethods
	{
		[MoonSharpMethod]
		public static DynValue create(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args.AsType(0, "create", DataType.Function);
			return executionContext.CoroutineCreate(v.Function);
		}

		[MoonSharpMethod]
		public static DynValue wrap(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = create(executionContext, args);
			DynValue c = DynValue.NewCallback(__wrap_wrapper);
			c.Callback.Closure = new Table(executionContext.GetScript());
			c.Callback.Closure.Set("_HANDLE", v);
			return c;
		}


		public static DynValue __wrap_wrapper(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue handle = executionContext.Closure.Get("_HANDLE");
			return executionContext.CoroutineResume(handle, args.List.ToArray());
		}


		[MoonSharpMethod]
		public static DynValue resume(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue handle = args.AsType(0, "resume", DataType.Thread);

			try
			{
				DynValue ret = executionContext.CoroutineResume(handle, args.List.Skip(1).ToArray());

				List<DynValue> retval = new List<DynValue>();
				retval.Add(DynValue.True);

				if (ret.Type == DataType.Tuple)
					DynValue.ExpandArgumentsToList(ret.Tuple, retval);
				else
					retval.Add(ret);

				return DynValue.NewTuple(retval.ToArray());
			}
			catch (ScriptRuntimeException ex)
			{
				return DynValue.NewTuple(
					DynValue.False,
					DynValue.NewString(ex.Message));
			}
		}

		[MoonSharpMethod]
		public static DynValue yield(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return DynValue.NewYieldReq(args.List.ToArray());
		}



		[MoonSharpMethod]
		public static DynValue running(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue handle = executionContext.CoroutineRunning();
			CoroutineState cs = executionContext.CoroutineGetState(handle);

			return DynValue.NewTuple(
				handle,
				DynValue.NewBoolean(cs == CoroutineState.Main));
		}

		[MoonSharpMethod]
		public static DynValue status(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue handle = args.AsType(0, "status", DataType.Thread);
			CoroutineState cs = executionContext.CoroutineGetState(handle);

			switch (cs)
			{
				case CoroutineState.Main:
				case CoroutineState.Running:
					return (executionContext.CoroutineIsRunning(handle)) ?
						DynValue.NewString("running") :
						DynValue.NewString("normal");
				case CoroutineState.NotStarted:
				case CoroutineState.Suspended:
					return DynValue.NewString("suspended");
				case CoroutineState.Dead:
					return DynValue.NewString("dead");
				default:
					throw new InternalErrorException("Unexpected coroutine state {0}", cs);
			}
	
		}


	}
}
