using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	public class UserData
	{
		public object Object { get; set; }

		public string Id { get; set;  }

		public Table Metatable { get; set; }
	}
}
