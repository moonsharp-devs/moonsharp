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
		OS_Time = 0x800,
		OS_System = 0x1000,
		__RESERVED1__ = 0x2000,
		IO = 0x4000,
		Dynamic = 0x8000,


		Preset_HardSandbox = GlobalConsts | TableIterators | String | Table | Basic | Math | Bit32,
		Preset_SoftSandbox = Preset_HardSandbox | Metatables | ErrorHandling | Coroutine | OS_Time | Dynamic,
		Preset_Default = Preset_SoftSandbox | LoadMethods | OS_System | IO,
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
