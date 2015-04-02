using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace Tutorials.Chapters
{
	[Tutorial]
	static class Chapter05
	{
		static StringBuilder GetString()
		{
			return new StringBuilder("myString!");
		}

		[Tutorial]
		public static void StringBuilderCustomConverter()
		{
			Script script = new Script();

			script.Globals["getstr"] = (Func<StringBuilder>)GetString;

			DynValue fn = script.LoadString("print(getstr())");

			fn.Function.Call();

			Script.GlobalOptions.CustomConverters.SetClrToScriptCustomConversion<StringBuilder>(
				v => DynValue.NewString(v.ToString().ToUpper()));

			fn.Function.Call();
		}
	}
}
