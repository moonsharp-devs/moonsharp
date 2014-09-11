using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop;

namespace MoonSharp.Interpreter
{
	public class UserData
	{
		public object Object { get; set; }

		public UserDataDescriptor Descriptor { get; set; }
	}
}
