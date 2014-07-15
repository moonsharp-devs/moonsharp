using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.CoreLib
{
	[MoonSharpModule(Namespace="math")]
	public class MathModule
	{
		[MoonSharpConstant]
		public const double pi = Math.PI;


	}
}
