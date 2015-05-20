using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	public static class DescriptorHelpers
	{
		/// <summary>
		/// Gets the list of "metamethods" a MethodInfo intends to implement by inspecting its
		/// <see cref="MoonSharpUserDataMetamethodAttribute" />.
		/// </summary>
		/// <param name="mi">The mi.</param>
		/// <returns></returns>
		public static List<string> GetMoonSharpMetaNamesFromAttributes(this MethodInfo mi)
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

		public static bool IsPropertyInfoPublic(this PropertyInfo pi)
		{
			MethodInfo getter = pi.GetGetMethod();
			MethodInfo setter = pi.GetSetMethod();

			return (getter != null && getter.IsPublic) || (setter != null && setter.IsPublic);
		}


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
		public static bool? GetMoonSharpVisibilityFromAttributes(this MemberInfo mi)
		{
			if (mi == null)
				return false;

			MoonSharpVisibleAttribute va = mi.GetCustomAttributes(true).OfType<MoonSharpVisibleAttribute>().SingleOrDefault();

			if (va != null)
				return va.Visible;
			else
				return null;
		}


		public static IUserDataMemberDescriptor AsMemberOfType(this IUserDataMemberDescriptor md, UserDataMemberType type)
		{
			if (md.MemberType == type)
				return md;

			return null;
		}



	}
}
