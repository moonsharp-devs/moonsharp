using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib.IO;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace = "io")]
	public class IoModule
	{
		public enum DefaultFiles
		{
			In,
			Out,
			Err
		}

		public static void MoonSharpInit(Table globalTable, Table ioTable)
		{
			UserData.RegisterType<FileUserDataBase>(InteropAccessMode.Default, "file");

			Table meta = new Table(ioTable.OwnerScript);
			DynValue __index = DynValue.NewCallback(new CallbackFunction(__index_callback, "__index_callback"));
			meta.Set("__index", __index);
			ioTable.MetaTable = meta;

			stdin = StandardIOFileUserDataBase.CreateInputStream(Console.OpenStandardInput());
			stdout = StandardIOFileUserDataBase.CreateOutputStream(Console.OpenStandardOutput());
			stderr = StandardIOFileUserDataBase.CreateOutputStream(Console.OpenStandardError());
		}

		private static DynValue __index_callback(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			string name = args[1].CastToString();

			if (name == "stdin")
				return UserData.Create(stdin);
			else if (name == "stdout")
				return UserData.Create(stdout);
			else if (name == "stderr")
				return UserData.Create(stderr);
			else
				return DynValue.Nil;
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
			SetDefaultFile(executionContext.GetScript(), file, fileHandle);
		}

		public static void SetDefaultFile(Script script, DefaultFiles file, FileUserDataBase fileHandle)
		{
			Table R = script.Registry;
			R.Set("853BEAAF298648839E2C99D005E1DF94_" + file.ToString(), UserData.Create(fileHandle));
		}

		public static void SetDefaultFile(Script script, DefaultFiles file, Stream stream)
		{
			if (file == DefaultFiles.In)
				SetDefaultFile(script, file, StandardIOFileUserDataBase.CreateInputStream(stream));
			else
				SetDefaultFile(script, file, StandardIOFileUserDataBase.CreateOutputStream(stream));
		}


		[MoonSharpMethod]
		public static DynValue close(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			FileUserDataBase outp = args.AsUserData<FileUserDataBase>(0, "close", true) ?? GetDefaultFile(executionContext, DefaultFiles.Out);
			return outp.close(executionContext, args);
		}

		[MoonSharpMethod]
		public static DynValue flush(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			FileUserDataBase outp = args.AsUserData<FileUserDataBase>(0, "close", true) ?? GetDefaultFile(executionContext, DefaultFiles.Out);
			outp.flush();
			return DynValue.True;
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
			if (args.Count == 0 || args[0].IsNil())
			{
				var file = GetDefaultFile(executionContext, defaultFiles);
				return UserData.Create(file);
			}

			FileUserDataBase inp = null;

			if (args[0].Type == DataType.String || args[0].Type == DataType.Number)
			{
				string fileName = args[0].CastToString();
				inp = Open(fileName, GetUTF8Encoding(), defaultFiles == DefaultFiles.In ? "r" : "w");
			}
			else
			{
				inp = args.AsUserData<FileUserDataBase>(0, defaultFiles == DefaultFiles.In ? "input" : "output", false);
			}

			SetDefaultFile(executionContext, defaultFiles, inp);

			return UserData.Create(inp);
		}

		private static Encoding GetUTF8Encoding()
		{
			return new System.Text.UTF8Encoding(false); 
		}

		[MoonSharpMethod]
		public static DynValue lines(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			string filename = args.AsType(0, "lines", DataType.String, false).String;

			try
			{
				List<DynValue> readLines = new List<DynValue>();

				using (var stream = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite | System.IO.FileShare.Delete))
				{
					using (var reader = new System.IO.StreamReader(stream))
					{
						while (!reader.EndOfStream)
						{
							string line = reader.ReadLine();
							readLines.Add(DynValue.NewString(line));
						}
					}
				}

				readLines.Add(DynValue.Nil);

				return DynValue.FromObject(executionContext.GetScript(), readLines.Select(s => s));
			}
			catch (Exception ex)
			{
				throw new ScriptRuntimeException(IoExceptionToLuaMessage(ex, filename));
			}
		}

		[MoonSharpMethod]
		public static DynValue open(ScriptExecutionContext executionContext, CallbackArguments args)
		{
			string filename = args.AsType(0, "open", DataType.String, false).String;
			DynValue vmode = args.AsType(1, "open", DataType.String, true);
			DynValue vencoding = args.AsType(2, "open", DataType.String, true);

			string mode = vmode.IsNil() ? "r" : vmode.String;

			string invalidChars = mode.Replace("+", "")
				.Replace("r", "")
				.Replace("a", "")
				.Replace("w", "")
				.Replace("b", "")
				.Replace("t", "");

			if (invalidChars.Length > 0)
				throw ScriptRuntimeException.BadArgument(1, "open", "invalid mode");


			try
			{
				string encoding = vencoding.IsNil() ? null : vencoding.String;

				// list of codes: http://msdn.microsoft.com/en-us/library/vstudio/system.text.encoding%28v=vs.90%29.aspx.
				// In addition, "binary" is available.
				Encoding e = null;
				bool isBinary = mode.Contains('b');

				if (encoding == "binary")
				{
					isBinary = true;
					e = new BinaryEncoding();
				}
				else if (encoding == null)
				{
					if (!isBinary) e = GetUTF8Encoding();
					else e = new BinaryEncoding();
				}
				else
				{
					if (isBinary)
						throw new ScriptRuntimeException("Can't specify encodings other than nil or 'binary' for binary streams.");

					e = Encoding.GetEncoding(encoding);
				}

				return UserData.Create(Open(filename, e, mode));
			}
			catch (Exception ex)
			{
				return DynValue.NewTuple(DynValue.Nil,
					DynValue.NewString(IoExceptionToLuaMessage(ex, filename)));
			}

		}

		public static string IoExceptionToLuaMessage(Exception ex, string filename)
		{
			if (ex is System.IO.FileNotFoundException)
				return string.Format("{0}: No such file or directory", filename);
			else
				return ex.Message;
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
			FileUserDataBase file = Open(System.IO.Path.GetTempFileName(), GetUTF8Encoding(), "w");
			return UserData.Create(file);
		}

		private static FileUserDataBase Open(string filename, Encoding encoding, string mode)
		{
			return new FileUserData(filename, encoding, mode);
		}



	}

}
