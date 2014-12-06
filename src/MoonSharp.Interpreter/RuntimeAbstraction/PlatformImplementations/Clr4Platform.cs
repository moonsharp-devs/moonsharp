using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.RuntimeAbstraction
{
	class Clr4Platform : Clr2Platform
	{
		public override string Name
		{
			get { return "clr-4"; }
		}
	}
}
