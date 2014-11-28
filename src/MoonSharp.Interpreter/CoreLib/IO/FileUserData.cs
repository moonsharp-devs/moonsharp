using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.CoreLib.IO
{
	class FileUserData : StreamFileUserDataBase
	{
		public FileUserData(string filename, Encoding encoding, string mode)
		{
			Stream stream = new FileStream(filename, ParseFileMode(mode), ParseFileAccess(mode), FileShare.ReadWrite | FileShare.Delete);
			StreamReader reader = (stream.CanRead) ? new StreamReader(stream, encoding) : null;
			StreamWriter writer = (stream.CanWrite) ? new StreamWriter(stream, encoding) : null;

			base.Initialize(stream, reader, writer);
		}

		private FileAccess ParseFileAccess(string mode)
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

		private FileMode ParseFileMode(string mode)
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


	}
}
