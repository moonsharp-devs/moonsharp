//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;

//namespace MoonSharp.VsCodeDebugger.SDK
//{
//	public static class ObjectDynamic_Ext
//	{
//		public static T Get<T>(this object obj, string property, T defval = default(T))
//		{
//			PropertyInfo pi = obj.GetType().GetProperty(property);

//			if (pi == null)
//				return defval;

//			return (T)pi.GetValue(obj, null);
//		}





//	}
//}
