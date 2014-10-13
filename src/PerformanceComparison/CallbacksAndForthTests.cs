using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using NLua;

namespace PerformanceComparison
{
	[MoonSharpUserData]
	public class ChampionPropertiesComponent
	{
		string name;

		internal void SetFirstName(string p)
		{
			name = p;
		}

		internal void SetLastName(string p)
		{
			name += " " + p;
		}

		public string getName()
		{
			return name;
		}

		internal ChampionPropertiesComponent ToInterface()
		{
			return this;
		}
	}


	class CallbacksAndForthTests
	{
		public static void Main()
		{
			(new CallbacksAndForthTests()).Start();
			//Console.ReadLine();
		}

		private void Start()
		{
			//UserData.DefaultAccessMode = InteropAccessMode.Preoptimized;
			UserData.RegisterAssembly();

			//var nLuaState = new Lua();
			var moonSharpState = new Script();

			const string script = @"
			a = """"
			onUpdate = function(championPropertiesComponent)
				a = championPropertiesComponent:getName()
			end
		"; 

			//nLuaState.DoString(script);
			moonSharpState.DoString(script);

			var championProperties = new ChampionPropertiesComponent();
			championProperties.SetFirstName("John");
			championProperties.SetLastName("Smith"); 

			//var nLuaFunction = (LuaFunction)nLuaState["onUpdate"];
			var moonSharpFunction = (Closure)moonSharpState.Globals["onUpdate"];

			int startTime, endTime;

			//// Test NLua
			//startTime = Environment.TickCount;
			//for (int i = 0; i < 100000; i++) nLuaFunction.Call(championProperties.ToInterface());
			//endTime = Environment.TickCount;
			//Console.WriteLine("NLua : {0}", endTime - startTime);

			// Test MoonSharp
			startTime = Environment.TickCount;
			//DynValue v = DynValue.FromObject(moonSharpState, championProperties.ToInterface());
			for (int i = 0; i < 2000000; i++) moonSharpFunction.Call(championProperties.ToInterface());
			endTime = Environment.TickCount;
			Console.WriteLine("MoonSharp : {0}", endTime - startTime);
		}

	}
}
