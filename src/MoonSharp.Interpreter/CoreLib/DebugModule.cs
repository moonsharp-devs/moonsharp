using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Debugging;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace = "debug")]
	public class DebugModule
	{
		[MoonSharpMethod]
		public static DynValue debug(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			Script script = executionContext.GetScript();

			while (true)
			{
				try
				{
					string cmd = script.Options.DebugInput();

					if (cmd == "cont")
						return DynValue.Void;

					DynValue v = script.LoadString(cmd, null, "stdin");
					DynValue result = script.Call(v);
					script.Options.DebugPrint(string.Format("={0}", result));
				}
				catch (ScriptRuntimeException ex)
				{
					script.Options.DebugPrint(string.Format("{0}", ex.DecoratedMessage ?? ex.Message));
				}
				catch (Exception ex)
				{
					script.Options.DebugPrint(string.Format("{0}", ex.Message));
				}
			}
		}

		[MoonSharpMethod]
		public static DynValue getuservalue(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args[0];

			if (v.Type != DataType.UserData)
				return DynValue.Nil;

			return v.UserData.UserValue ?? DynValue.Nil;
		}

		[MoonSharpMethod]
		public static DynValue setuservalue(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args.AsType(0, "setuservalue", DataType.UserData, false);
			DynValue t = args.AsType(0, "setuservalue", DataType.Table, true);

			return v.UserData.UserValue = t;
		}

		[MoonSharpMethod]
		public static DynValue getregistry(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return DynValue.NewTable(executionContext.GetScript().Registry);
		}

		[MoonSharpMethod]
		public static DynValue getmetatable(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args[0];
			Script S = executionContext.GetScript();

			if (v.Type.CanHaveTypeMetatables())
				return DynValue.NewTable(S.GetTypeMetatable(v.Type));
			else if (v.Type == DataType.Table)
				return DynValue.NewTable(v.Table.MetaTable);
			else
				return DynValue.Nil;
		}

		[MoonSharpMethod]
		public static DynValue setmetatable(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args[0];
			DynValue t = args.AsType(1, "setmetatable", DataType.Table, true);
			Table m = (t.IsNil()) ? null : t.Table;
			Script S = executionContext.GetScript();

			if (v.Type.CanHaveTypeMetatables())
				S.SetTypeMetatable(v.Type, m);
			else if (v.Type == DataType.Table)
				v.Table.MetaTable = m;
			else
				throw new ScriptRuntimeException("cannot debug.setmetatable on type {0}", v.Type.ToErrorTypeString());

			return v;
		}

		[MoonSharpMethod]
		public static DynValue getupvalue(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			var index = (int)args.AsType(1, "getupvalue", DataType.Number, false).Number - 1;

			if (args[0].Type == DataType.ClrFunction)
				return DynValue.Nil;

			var fn = args.AsType(0, "getupvalue", DataType.Function, false).Function;

			var closure = fn.ClosureContext;

			if (index < 0 || index >= closure.Count)
				return DynValue.Nil;

			return DynValue.NewTuple(
				DynValue.NewString(closure.Symbols[index]),
				closure[index]);
		}


		[MoonSharpMethod]
		public static DynValue upvalueid(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			var index = (int)args.AsType(1, "getupvalue", DataType.Number, false).Number - 1;

			if (args[0].Type == DataType.ClrFunction)
				return DynValue.Nil;

			var fn = args.AsType(0, "getupvalue", DataType.Function, false).Function;

			var closure = fn.ClosureContext;

			if (index < 0 || index >= closure.Count)
				return DynValue.Nil;

			return DynValue.NewNumber(closure[index].ReferenceID);
		}


		[MoonSharpMethod]
		public static DynValue setupvalue(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			var index = (int)args.AsType(1, "setupvalue", DataType.Number, false).Number - 1;

			if (args[0].Type == DataType.ClrFunction)
				return DynValue.Nil;

			var fn = args.AsType(0, "setupvalue", DataType.Function, false).Function;

			var closure = fn.ClosureContext;

			if (index < 0 || index >= closure.Count)
				return DynValue.Nil;

			closure[index] = args[2];

			return DynValue.NewString(closure.Symbols[index]);
		}


		[MoonSharpMethod]
		public static DynValue traceback(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			StringBuilder sb = new StringBuilder();

			DynValue vmessage = args[0];
			DynValue vlevel = args[1];

			double defaultSkip = 1.0;

			Coroutine cor = executionContext.GetCallingCoroutine();

			if (vmessage.Type == DataType.Thread)
			{
				cor = vmessage.Coroutine;
				vmessage = args[1];
				vlevel = args[2];
				defaultSkip = 0.0;
			}

			if (vmessage.IsNotNil() && vmessage.Type != DataType.String && vmessage.Type != DataType.Number)
			{
				return vmessage;
			}

			string message = vmessage.CastToString();

			int skip = (int)((vlevel.CastToNumber()) ?? defaultSkip);

			WatchItem[] stacktrace = cor.GetStackTrace(Math.Max(0, skip));

			if (message != null)
				sb.AppendLine(message);

			sb.AppendLine("stack traceback:");

			foreach (WatchItem wi in stacktrace)
			{
				string name;

				if (wi.Name == null)
					if (wi.RetAddress < 0)
						name = "main chunk";
					else
						name = "?";
				else
					name = "function '" + wi.Name + "'";

				string loc = wi.Location != null ? wi.Location.FormatLocation(executionContext.GetScript()) : "[clr]";
				sb.AppendFormat("\t{0}: in {1}\n", loc, name);
			}

			return DynValue.NewString(sb);
		}


		//[MoonSharpMethod]
		//public static DynValue getinfo(ScriptExecutionContext executionContext, CallbackArguments args)
		//{
		//	Coroutine cor = executionContext.GetCallingCoroutine();
		//	int vfArgIdx = 0;

		//	if (args[0].Type == DataType.Thread)
		//		cor = args[0].Coroutine;

		//	DynValue vf = args[vfArgIdx+0];
		//	DynValue vwhat = args[vfArgIdx+1];

		//	args.AsType(vfArgIdx + 1, "getinfo", DataType.String, true);
			
		//	string what = vwhat.CastToString() ?? "nfSlu";

		//	DynValue vt = DynValue.NewTable(executionContext.GetScript());
		//	Table t = vt.Table;

		//	if (vf.Type == DataType.Function)
		//	{
		//		Closure f = vf.Function;
		//		executionContext.GetInfoForFunction
		//	}
		//	else if (vf.Type == DataType.ClrFunction)
		//	{

		//	}
		//	else if (vf.Type == DataType.Number || vf.Type == DataType.String)
		//	{

		//	}
		//	else
		//	{
		//		args.AsType(vfArgIdx + 0, "getinfo", DataType.Number, true);
		//	}

		//	return vt;


		//}

	}
}
