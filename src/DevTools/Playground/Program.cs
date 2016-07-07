using MoonSharp.Interpreter;
using System;
using System.Diagnostics;
using System.IO;

namespace Test
{
	[MoonSharpUserData]
	public class API_Hooks
	{
		public event EventHandler Test;

		public void RaiseTheEvent()
		{
			if (Test != null)
				Test(this, EventArgs.Empty);
		}

	}

	class Program
	{
		static void Main(string[] args)
		{
			string code = @"
function handler(o, a)
    print('test1',o, a);
end

function handler2(o, a)
    print('test2', o, a);
end

Hooks.test.add(handler)
Hooks.test.add(handler2)
Hooks.raiseTheEvent()
";

			UserData.RegisterAssembly();
			UserData.RegisterType<EventArgs>();


			// *****************************************************
			// *** DUMP 
			// *****************************************************
			Script script = new Script(CoreModules.Preset_Default);

			script.Globals["Hooks"] = new API_Hooks();

			DynValue chunk = script.LoadString(code);

			using (Stream stream = new FileStream(@"c:\temp\issue140.dump", FileMode.Create, FileAccess.Write))
				script.Dump(chunk, stream);

			// *****************************************************
			// *** EXECUTE DUMPED
			// *****************************************************

			Script script2 = new Script(CoreModules.Preset_Default);
			script2.Globals["Hooks"] = new API_Hooks();
			script2.DoFile(@"c:\temp\issue140.dump");


			Console.ReadKey();
		}
	}
}