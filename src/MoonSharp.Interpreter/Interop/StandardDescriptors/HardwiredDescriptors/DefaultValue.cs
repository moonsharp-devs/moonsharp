using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop.StandardDescriptors.HardwiredDescriptors
{
	public sealed class DefaultValue
	{
		public static readonly DefaultValue Instance = new DefaultValue();
	}
}
