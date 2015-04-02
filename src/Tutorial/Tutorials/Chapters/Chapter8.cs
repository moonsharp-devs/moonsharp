using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace Tutorials.Chapters
{
	[Tutorial]
	static class Chapter08
	{
		[Tutorial]
		static void EmbeddedResourceScriptLoader()
		{
			Script script = new Script();
			script.Options.ScriptLoader = new EmbeddedResourcesScriptLoader(Assembly.GetExecutingAssembly());
			script.DoFile("Scripts/Test.lua");
		}

		private class MyCustomScriptLoader : ScriptLoaderBase
		{
			public override object LoadFile(string file, Table globalContext)
			{
				return string.Format("print ([[A request to load '{0}' has been made]])", file);
			}

			public override bool ScriptFileExists(string name)
			{
				return true;
			}
		}

		[Tutorial]
		static void CustomScriptLoader()
		{
			Script script = new Script();

			script.Options.ScriptLoader = new MyCustomScriptLoader() 
			{ 
				ModulePaths = new string[] { "?_module.lua" } 
			};

			script.DoString(@"
				require 'somemodule'
				f = loadfile 'someothermodule.lua'
				f()
			");
		}


	}
}
