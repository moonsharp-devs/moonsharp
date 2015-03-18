using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.RuntimeAbstraction
{
	public abstract class Platform
	{
		private static Platform s_Current;

		static Platform()
		{
			bool onUnity = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Any(t => t.FullName.StartsWith("UnityEngine."));

			if (Type.GetType("Mono.Runtime") != null)
			{
				if (onUnity)
				{
					s_Current = new UnityPlatform();
				}
				else
				{
					bool onXamarinDroid = AppDomain.CurrentDomain
						.GetAssemblies()
						.SelectMany(a => a.GetTypes())
						.Any(t => t.FullName.StartsWith("Android.App."));

					if (onXamarinDroid)
					{
						s_Current = new XamarinAndroidPlatform();
					}
					else
					{
						s_Current = new MonoPlatform();
					}
				}
			}
			else if (onUnity)
			{
				s_Current = new UnityWebPlatform();
			}
			else if (Type.GetType("System.Lazy`1") != null)
			{
				s_Current = new Clr4Platform();
			}
			else
			{
				s_Current = new Clr2Platform();
			}

			System.Diagnostics.Debug.WriteLine(string.Format("MoonSharp {0} running over {1}.",
				Script.VERSION, s_Current.Name));
		}


		public static Platform Current
		{
			get { return s_Current; }
		}


		public abstract string Name { get; }
		public abstract string GetEnvironmentVariable(string variable);


		public abstract CoreModules FilterSupportedCoreModules(CoreModules module);


		public virtual bool IsAOT()
		{
			return false;
		}



		public bool AreCoreModulesFullySupported(CoreModules modules)
		{
			return FilterSupportedCoreModules(modules) == modules;
		}

	}
}
