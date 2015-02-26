using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.CoreLib.IO
{
	public class StandardIOFileUserDataBase : StreamFileUserDataBase
	{
		protected override string Close()
		{
			return ("cannot close standard file");
		}

		public static StandardIOFileUserDataBase CreateInputStream(Stream stream)
		{
			var f = new StandardIOFileUserDataBase();
			f.Initialize(stream, new StreamReader(stream), null);
			return f;
		}

		public static StandardIOFileUserDataBase CreateOutputStream(Stream stream)
		{
			var f = new StandardIOFileUserDataBase();
			f.Initialize(stream, null, new StreamWriter(stream));
			return f;
		}

	}

}
