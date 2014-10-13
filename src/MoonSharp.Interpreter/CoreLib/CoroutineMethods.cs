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
			if (args[0].Type != DataType.Function && args[0].Type != DataType.ClrFunction)
				args.AsType(0, "create", DataType.Function); // this throws

			return executionContext.GetScript().CreateCoroutine(args[0]);
		}

		[MoonSharpMethod]
		public static DynValue wrap(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			if (args[0].Type != DataType.Function && args[0].Type != DataType.ClrFunction)
				args.AsType(0, "wrap", DataType.Function); // this throws

			DynValue v = create(executionContext, args);
			DynValue c = DynValue.NewCallback(__wrap_wrapper);
			c.Callback.AdditionalData = v;
			return c;
		}

		public static DynValue __wrap_wrapper(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue handle = (DynValue)executionContext.AdditionalData;
			return handle.Coroutine.Resume(args.List.ToArray());
		}

		[MoonSharpMethod]
		public static DynValue resume(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue handle = args.AsType(0, "resume", DataType.Thread);

			try
			{
				DynValue ret = handle.Coroutine.Resume(args.List.Skip(1).ToArray());

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
			Coroutine C = executionContext.GetCallingCoroutine();
			return DynValue.NewTuple(DynValue.NewCoroutine(C), DynValue.NewBoolean(C.State == CoroutineState.Main));
		}

		[MoonSharpMethod]
		public static DynValue status(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue handle = args.AsType(0, "status", DataType.Thread);
			Coroutine running = executionContext.GetCallingCoroutine();
			CoroutineState cs = handle.Coroutine.State;

			switch (cs)
			{
				case CoroutineState.Main:
				case CoroutineState.Running:
					return (handle.Coroutine == running) ?
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
