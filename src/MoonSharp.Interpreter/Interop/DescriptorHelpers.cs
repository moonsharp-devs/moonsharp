using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Helper extension methods used to simplify some parts of userdata descriptor implementations
	/// </summary>
	public static class DescriptorHelpers
	{
		/// <summary>
		/// Determines whether a 
		/// <see cref="MoonSharpVisibleAttribute" /> is changing visibility of a member
		/// to scripts.
		/// </summary>
		/// <param name="mi">The member to check.</param>
		/// <returns>
		/// <c>true</c> if visibility is forced visible, 
		/// <c>false</c> if visibility is forced hidden or the specified MemberInfo is null,
		/// <c>if no attribute was found</c>
		/// </returns>
		public static bool? GetVisibilityFromAttributes(this MemberInfo mi)
		{
			if (mi == null)
				return false;

			MoonSharpVisibleAttribute va = mi.GetCustomAttributes(true).OfType<MoonSharpVisibleAttribute>().SingleOrDefault();

			if (va != null)
				return va.Visible;
			else
				return null;
		}


		/// <summary>
		/// Determines whether the specified PropertyInfo is visible publicly (either the getter or the setter is public).
		/// </summary>
		/// <param name="pi">The PropertyInfo.</param>
		/// <returns></returns>
		public static bool IsPropertyInfoPublic(this PropertyInfo pi)
		{
			MethodInfo getter = pi.GetGetMethod();
			MethodInfo setter = pi.GetSetMethod();

			return (getter != null && getter.IsPublic) || (setter != null && setter.IsPublic);
		}

		public static List<string> GetMetaNamesFromAttributes(this MethodInfo mi)
		{
			return mi.GetCustomAttributes(typeof(MoonSharpUserDataMetamethodAttribute), true)
				.OfType<MoonSharpUserDataMetamethodAttribute>()
				.Select(a => a.Name)
				.ToList();
		}

		public static string GetConversionMethodName(this Type type)
		{
			StringBuilder sb = new StringBuilder(type.Name);

			for (int i = 0; i < sb.Length; i++)
				if (!char.IsLetterOrDigit(sb[i])) sb[i] = '_';

			return "__to" + sb.ToString();
		}


	}
}
