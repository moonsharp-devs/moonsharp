using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Platforms
{
	internal static class PlatformAutoSelector
	{
		internal static IPlatformAccessor GetDefaultPlatform()
		{
			return null; // new StandardPlatformAccessor();
		}


	}
}
