using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.RuntimeAbstraction
{
	class MonoPlatform : Clr2Platform
	{
		public override string Name
		{
			get { return "mono"; }
		}
	}
}
