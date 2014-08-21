using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	public class MethodAdapter
	{
		private class ArgType
		{
			public DataType Type;
			public bool Nullable;
		}

		public MethodAdapter(MethodInfo mi)
		{
			foreach (var arg in mi.GetParameters())
			{
				
			}
		}






	}
}
