using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	internal static class PlatformAutoSelector
	{
		public static bool IsRunningOnMono { get; private set; }
		public static bool IsRunningOnClr4 { get; private set; }
		public static bool IsRunningOnUnity { get; private set; }
		public static bool IsPortableFramework { get; private set; }


		internal static IPlatformAccessor GetDefaultPlatform()
		{
#if PCL
			IsPortableFramework = true;
#else
			IsRunningOnUnity = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Any(t => t.FullName.StartsWith("UnityEngine."));
#endif

			IsRunningOnMono = (Type.GetType("Mono.Runtime") != null);

			IsRunningOnClr4 = (Type.GetType("System.Lazy`1") != null);

#if PCL
			return null;
#else
			return new StandardPlatformAccessor();
#endif
		}


	}
}
