using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.CoreLib.IO
{
	class StandardIOFileUserDataBase : StreamFileUserDataBase
	{
		protected override string Close()
		{
			return ("cannot close standard file");
		}
	}


	class StdinFileUserData : StandardIOFileUserDataBase
	{
		public StdinFileUserData()
		{
			Stream stream = Console.OpenStandardInput();
			StreamReader reader = new StreamReader(stream);
			Initialize(stream, reader, null);
		}
	}

	class StdoutFileUserData : StandardIOFileUserDataBase
	{
		public StdoutFileUserData()
		{
			Stream stream = Console.OpenStandardOutput();
			StreamWriter writer = new StreamWriter(stream);
			Initialize(stream, null, writer);
		}
	}

	class StderrFileUserData : StandardIOFileUserDataBase
	{
		public StderrFileUserData()
		{
			Stream stream = Console.OpenStandardError();
			StreamWriter writer = new StreamWriter(stream);
			Initialize(stream, null, writer);
		}
	}
}
