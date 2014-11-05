using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	[Flags]
	public enum CoreModules
	{
		None = 0,

		Basic = 0x40,
		GlobalConsts = 0x1,
		TableIterators = 0x2,
		Metatables = 0x4,
		String = 0x8,
		LoadMethods = 0x10,
		Table = 0x20,
		ErrorHandling = 0x80,
		Math = 0x100,
		Coroutine = 0x200,
		Bit32 = 0x400,
		Os_Time = 0x800,
		Os_System = 0x1000,
		File = 0x2000,
		Io = 0x4000,
		Dynamic = 0x8000,



		Preset_HardSandbox = GlobalConsts | TableIterators | String | Table | Basic | Math | Bit32,
		Preset_SoftSandbox = Preset_HardSandbox | Metatables | ErrorHandling | Coroutine | Os_Time | Dynamic,
		Preset_Default = Preset_SoftSandbox | LoadMethods | Os_System | File | Io,
		Preset_Complete = Preset_Default,

	}

	public static class CoreModules_ExtensionMethods
	{
		public static bool Has(this CoreModules val, CoreModules flag)
		{
			return (val & flag) == flag;
		}
	}


}
