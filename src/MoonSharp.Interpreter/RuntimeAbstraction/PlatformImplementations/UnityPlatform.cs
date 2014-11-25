using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.RuntimeAbstraction
{
	class UnityPlatform : MonoPlatform
	{
		public override string Name
		{
			get { return "Unity"; }
		}
	}
}
