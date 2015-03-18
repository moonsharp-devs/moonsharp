#if !PCL
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	public class StandardPlatformAccessor : PlatformAccessorBase
	{
		public StandardPlatformAccessor()
			: base()
		{ }

		public StandardPlatformAccessor(params string[] modulePaths)
			: base(modulePaths)
		{ }


		public static FileAccess ParseFileAccess(string mode)
		{
			mode = mode.Replace("b", "");

			if (mode == "r")
				return FileAccess.Read;
			else if (mode == "r+")
				return FileAccess.ReadWrite;
			else if (mode == "w")
				return FileAccess.Write;
			else if (mode == "w+")
				return FileAccess.ReadWrite;
			else
				return FileAccess.ReadWrite;
		}

		public static FileMode ParseFileMode(string mode)
		{
			mode = mode.Replace("b", "");

			if (mode == "r")
				return FileMode.Open;
			else if (mode == "r+")
				return FileMode.OpenOrCreate;
			else if (mode == "w")
				return FileMode.Create;
			else if (mode == "w+")
				return FileMode.Truncate;
			else
				return FileMode.Append;
		}


		public override Stream OpenFileForIO(Script script, string filename, Encoding encoding, string mode)
		{
			return new FileStream(filename, ParseFileMode(mode), ParseFileAccess(mode), FileShare.ReadWrite | FileShare.Delete);
		}

		public override string GetEnvironmentVariable(string envvarname)
		{
			return Environment.GetEnvironmentVariable(envvarname);
		}

		public override bool FileExists(string name)
		{
			return File.Exists(name);
		}

		public override object LoadFile(Script script, string file, Table globalContext)
		{
			return new FileStream(file, FileMode.Open, FileAccess.Read);
		}



		public override Stream GetStandardStream(StandardFileType type)
		{
			switch (type)
			{
				case StandardFileType.StdIn:
					return Console.OpenStandardInput();
				case StandardFileType.StdOut:
					return Console.OpenStandardOutput();
				case StandardFileType.StdErr:
					return Console.OpenStandardError();
				default:
					throw new ArgumentException("type");
			}
		}

		public override void DefaultPrint(string content)
		{
			Console.WriteLine(content);
		}

		public override string DefaultInput()
		{
			return Console.ReadLine();
		}
	}
}
#endif