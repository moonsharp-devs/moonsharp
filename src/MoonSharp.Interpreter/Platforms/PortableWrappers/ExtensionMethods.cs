#if PCL || ((!UNITY_EDITOR) && (ENABLE_DOTNET))
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
	internal static class Pcl_ExtensionMethods
	{
		public static bool Contains(this string str, char chr)
		{
			return str.Contains(chr.ToString());
		}
	}
}


#endif