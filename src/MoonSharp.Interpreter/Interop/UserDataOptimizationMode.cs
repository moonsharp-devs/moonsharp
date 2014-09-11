using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	[Flags]
	public enum UserDataOptimizationMode
	{
		None,
		Lazy,
		Precomputed,
		HideMembers
	}
}
