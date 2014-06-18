using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter
{
	[Flags]
	public enum CoreModules
	{
		GlobalConsts = 0x1,
		TableIterators = 0x2,
		Metatables = 0x4,


		Preset_HardSandbox = GlobalConsts | TableIterators,
		Preset_SoftSandbox = Preset_HardSandbox | Metatables,
		Preset_Default = Preset_SoftSandbox,
	}

	public static class CoreModules_ExtensionMethods
	{
		public static bool Has(this CoreModules val, CoreModules flag)
		{
			return (val & flag) == flag;
		}
	}


}
