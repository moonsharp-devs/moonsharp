using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib.IO;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace = "io")]
	public class IoMethods
	{
		enum DefaultFiles
		{
			In,
			Out,
			Err
		}

		public static void MoonSharpInit(Table globalTable, Table stringTable)
		{
			UserData.RegisterType<FileUserDataBase>();
		}

		static FileUserDataBase stdin;

		static FileUserDataBase stdout;

		static FileUserDataBase stderr;

		static FileUserDataBase GetDefaultFile(ScriptExecutionContext executionContext, DefaultFiles file)
		{
			Table R = executionContext.GetScript().Registry;

			DynValue ff = R.Get("853BEAAF298648839E2C99D005E1DF94_" + file.ToString());

			if (ff.IsNil())
			{
				switch (file)
				{
					case DefaultFiles.In:
						return stdin;
					case DefaultFiles.Out:
						return stdout;
					case DefaultFiles.Err:
						return stderr;
					default:
						throw new InternalErrorException("DefaultFiles value defaulted");
				}
			}
			else
			{
				return ff.CheckUserDataType<FileUserDataBase>("getdefaultfile(" + file.ToString() + ")");
			}
		}

		static void SetDefaultFile(ScriptExecutionContext executionContext, DefaultFiles file, FileUserDataBase fileHandle)
		{
			Table R = executionContext.GetScript().Registry;
			R.Set("853BEAAF298648839E2C99D005E1DF94_" + file.ToString(), UserData.Create(fileHandle));
		}
		


		[MoonSharpMethod]
		public static DynValue close(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			FileUserDataBase outp = args.AsUserData<FileUserDataBase>(0, "close", true) ?? GetDefaultFile(executionContext, DefaultFiles.Out);
			outp.close();
			return DynValue.Void;
		}

		[MoonSharpMethod]
		public static DynValue flush(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			FileUserDataBase outp = args.AsUserData<FileUserDataBase>(0, "close", true) ?? GetDefaultFile(executionContext, DefaultFiles.Out);
			outp.flush();
			return DynValue.Void;
		}


		[MoonSharpMethod]
		public static DynValue input(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return HandleDefaultStreamSetter(executionContext, args, DefaultFiles.In);
		}

		[MoonSharpMethod]
		public static DynValue output(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			return HandleDefaultStreamSetter(executionContext, args, DefaultFiles.Out);
		}

		private static DynValue HandleDefaultStreamSetter(ScriptExecutionContext executionContext, CallbackArguments args, DefaultFiles defaultFiles)
		{
			if (args.Count == 0)
			{
				var file = GetDefaultFile(executionContext, defaultFiles);
				return UserData.Create(file);
			}

			FileUserDataBase inp = null;

			if (args[0].Type == DataType.String || args[0].Type == DataType.Number)
			{
				string fileName = args[0].CastToString();
				inp = Open(fileName, Encoding.UTF8, "w");
			}
			else
			{
				inp = args.AsUserData<FileUserDataBase>(0, "input", false);
			}

			SetDefaultFile(executionContext, defaultFiles, inp);

			return UserData.Create(inp);
		}

		[MoonSharpMethod]
		public static DynValue lines(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			string filename = args.AsType(0, "lines", DataType.String, false).String;

			try
			{
				string[] readLines = System.IO.File.ReadAllLines(filename);

				IEnumerable<DynValue> retLines = readLines
					.Select(s => DynValue.NewString(s))
					.Concat(new DynValue[] { DynValue.Nil });

				return DynValue.FromObject(executionContext.GetScript(), retLines);
			}
			catch(Exception ex)
			{
				throw new ScriptRuntimeException(ex);
			}
		}

		[MoonSharpMethod]
		public static DynValue open(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			string filename = args.AsType(0, "open", DataType.String, false).String;
			DynValue vmode = args.AsType(1, "open", DataType.String, true);
			DynValue vencoding = args.AsType(2, "open", DataType.String, true);

			string mode = vmode.IsNil() ? "r" : vmode.String;
			string encoding = vencoding.IsNil() ? null : vencoding.String;

			// list of codes: http://msdn.microsoft.com/en-us/library/vstudio/system.text.encoding%28v=vs.90%29.aspx.
			// In addition, "binary" is available.
			Encoding e = null;
			bool isBinary = mode.Contains('b');

			if (encoding == "binary")
			{
				isBinary = true;
			}
			else if (encoding == null)
			{
				if (!isBinary) e = Encoding.UTF8;
			}
			else
			{
				if (isBinary)
					throw new ScriptRuntimeException("Can't specify encodings other than nil or 'binary' for binary streams.");

				e = Encoding.GetEncoding(encoding);
			}

			return UserData.Create(Open(filename, e, mode));
		}

		[MoonSharpMethod]
		public static DynValue type(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			if (args[0].Type != DataType.UserData)
				return DynValue.Nil;

			FileUserDataBase file = args[0].UserData.Object as FileUserDataBase;

			if (file == null)
				return DynValue.Nil;
			else if (file.isopen())
				return DynValue.NewString("file");
			else
				return DynValue.NewString("closed file");
		}

		[MoonSharpMethod]
		public static DynValue read(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			FileUserDataBase file = GetDefaultFile(executionContext, DefaultFiles.In);
			return file.read(executionContext, args);
		}

		[MoonSharpMethod]
		public static DynValue write(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			FileUserDataBase file = GetDefaultFile(executionContext, DefaultFiles.Out);
			return file.write(executionContext, args);
		}

		[MoonSharpMethod]
		public static DynValue tmpfile(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			FileUserDataBase file = Open(System.IO.Path.GetTempFileName(), Encoding.UTF8, "w");
			return UserData.Create(file);
		}

		private static FileUserDataBase Open(string filename, Encoding encoding, string mode)
		{
			return new FileUserData(filename, encoding, mode);
		}



	}

}
