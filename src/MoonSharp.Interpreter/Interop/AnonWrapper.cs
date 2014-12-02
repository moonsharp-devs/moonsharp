using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	public class AnonWrapper
	{
	}

	public class AnonWrapper<T> : AnonWrapper
	{
		public AnonWrapper()
		{
		}

		public AnonWrapper(T o)
		{
			Value = o;
		}

		public T Value { get; set; }
	}

}
