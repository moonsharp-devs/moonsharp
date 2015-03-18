using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.CoreLib.IO
{
	public class FileUserData : StreamFileUserDataBase
	{
		public FileUserData(Script script, string filename, Encoding encoding, string mode)
		{
			Stream stream = Script.Platform.OpenFileForIO(script, filename, encoding, mode);

			StreamReader reader = (stream.CanRead) ? new StreamReader(stream, encoding) : null;
			StreamWriter writer = (stream.CanWrite) ? new StreamWriter(stream, encoding) : null;

			base.Initialize(stream, reader, writer);
		}
	}
}
