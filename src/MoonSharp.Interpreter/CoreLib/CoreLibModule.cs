using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter
{
	[Flags]
	public enum CoreLibModules
	{
		TableIterators = 0x1,
		Metatables = 0x2,


		Default_HardSandbox = TableIterators,
		Default_SoftSandbox = Default_HardSandbox | Metatables,
		Default = Default_SoftSandbox,
	}

	public static class CoreLib_Ext
	{
		public static Table RegisterCoreModules(this Table t, CoreLibModules modules = CoreLibModules.Default)
		{
			if ((modules & CoreLibModules.TableIterators) != 0)
				t.RegisterModuleType<TableIterators>();


			return t;
		}

	}

}
