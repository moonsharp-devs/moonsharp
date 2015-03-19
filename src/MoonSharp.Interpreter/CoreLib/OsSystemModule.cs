using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace = "os")]
	public class OsSystemModule
	{
		[MoonSharpMethod]
		public static DynValue execute(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v = args.AsType(0, "execute", DataType.String, true);

			if (v.IsNil())
			{
				return DynValue.NewBoolean(true);
			}
			else
			{
				try
				{
					int exitCode = Script.Platform.OS_Execute(v.String);

					return DynValue.NewTuple(
						DynValue.Nil,
						DynValue.NewString("exit"),
						DynValue.NewNumber(exitCode));
				}
				catch (Exception)
				{
					// +++ bad to swallow.. 
					return DynValue.Nil;
				}
			}
		}

		[MoonSharpMethod]
		public static DynValue exit(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue v_exitCode = args.AsType(0, "exit", DataType.Number, true);
			int exitCode = 0;

			if (v_exitCode.IsNotNil())
				exitCode = (int)v_exitCode.Number;

			Script.Platform.OS_ExitFast(exitCode);

			throw new InvalidOperationException("Unreachable code.. reached.");
		}

		[MoonSharpMethod]
		public static DynValue getenv(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			DynValue varName = args.AsType(0, "getenv", DataType.String, false);

			string val = Script.Platform.GetEnvironmentVariable(varName.String);

			if (val == null)
				return DynValue.Nil;
			else
				return DynValue.NewString(val);
		}

		[MoonSharpMethod]
		public static DynValue remove(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			string fileName = args.AsType(0, "remove", DataType.String, false).String;

			try
			{
				if (Script.Platform.OS_FileExists(fileName))
				{
					Script.Platform.OS_FileDelete(fileName);
					return DynValue.True;
				}
				else
				{
					return DynValue.NewTuple(
						DynValue.Nil,
						DynValue.NewString("{0}: No such file or directory.", fileName),
						DynValue.NewNumber(-1));
				}
			}
			catch (Exception ex)
			{
				return DynValue.NewTuple(DynValue.Nil, DynValue.NewString(ex.Message), DynValue.NewNumber(-1));
			}
		}

		[MoonSharpMethod]
		public static DynValue rename(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			string fileNameOld = args.AsType(0, "rename", DataType.String, false).String;
			string fileNameNew = args.AsType(1, "rename", DataType.String, false).String;

			try
			{
				if (!Script.Platform.OS_FileExists(fileNameOld))
				{
					return DynValue.NewTuple(DynValue.Nil,
						DynValue.NewString("{0}: No such file or directory.", fileNameOld),
						DynValue.NewNumber(-1));
				}

				Script.Platform.OS_FileMove(fileNameOld, fileNameNew);
				return DynValue.True;
			}
			catch (Exception ex)
			{
				return DynValue.NewTuple(DynValue.Nil, DynValue.NewString(ex.Message), DynValue.NewNumber(-1));
			}
		}

		[MoonSharpMethod]
		public static DynValue setlocale(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return DynValue.NewString("n/a");
		}

		[MoonSharpMethod]
		public static DynValue tmpname(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return DynValue.NewString(Script.Platform.IO_OS_GetTempFilename());
		}
	}
}
