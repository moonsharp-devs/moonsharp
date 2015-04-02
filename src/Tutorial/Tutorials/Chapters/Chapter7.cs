using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;

namespace Tutorials.Chapters
{
	[Tutorial]
	static class Chapter07
	{
		[Tutorial]
		static void ErrorHandling()
		{
			try
			{
				string scriptCode = @"    
					return obj.calcHypotenuse(3, 4);
				";

				Script script = new Script();
				DynValue res = script.DoString(scriptCode);
			}
			catch (ScriptRuntimeException ex)
			{
				Console.WriteLine("Doh! An error occured! {0}", ex.DecoratedMessage);
			}
		}

		static void DoError()
		{
			throw new ScriptRuntimeException("This is an exceptional message, no pun intended.");
		}

		[Tutorial]
		static string ErrorGen()
		{
			string scriptCode = @"    
				local _, msg = pcall(DoError);
				return msg;
			";

			Script script = new Script();
			script.Globals["DoError"] = (Action)DoError;
			DynValue res = script.DoString(scriptCode);
			return res.String;
		}
	}
}
