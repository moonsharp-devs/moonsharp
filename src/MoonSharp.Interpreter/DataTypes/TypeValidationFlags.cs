using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	[Flags]
	public enum TypeValidationFlags
	{
		AllowNil = 0x1,
		AutoConvert = 0x2,

		Default = AutoConvert
	}
}
