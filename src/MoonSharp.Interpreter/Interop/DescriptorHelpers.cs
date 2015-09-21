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

		/// <summary>
		/// Gets the list of metamethod names from attributes - in practice the list of metamethods declared through
		/// <see cref="MoonSharpUserDataMetamethodAttribute" /> .
		/// </summary>
		/// <param name="mi">The mi.</param>
		/// <returns></returns>
		public static List<string> GetMetaNamesFromAttributes(this MethodInfo mi)
		{
			return mi.GetCustomAttributes(typeof(MoonSharpUserDataMetamethodAttribute), true)
				.OfType<MoonSharpUserDataMetamethodAttribute>()
				.Select(a => a.Name)
				.ToList();
		}

		/// <summary>
		/// Gets the Types implemented in the assembly, catching the ReflectionTypeLoadException just in case..
		/// </summary>
		/// <param name="asm">The assebly</param>
		/// <returns></returns>
		public static Type[] SafeGetTypes(this Assembly asm)
		{
			try
			{
				return asm.GetTypes();
			}
			catch (ReflectionTypeLoadException)
			{
				return new Type[0];
			}
		}




		/// <summary>
		/// Gets the name of a conversion method to be exposed to Lua scripts
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns></returns>
		public static string GetConversionMethodName(this Type type)
		{
			StringBuilder sb = new StringBuilder(type.Name);

			for (int i = 0; i < sb.Length; i++)
				if (!char.IsLetterOrDigit(sb[i])) sb[i] = '_';

			return "__to" + sb.ToString();
		}


		/// <summary>
		/// Gets all implemented types by a given type
		/// </summary>
		/// <param name="t">The t.</param>
		/// <returns></returns>
		public static IEnumerable<Type> GetAllImplementedTypes(this Type t)
		{
			for (Type ot = t; ot != null; ot = ot.BaseType)
				yield return ot;

			foreach (Type it in t.GetInterfaces())
				yield return it;
		}


		/// <summary>
		/// Converts the specified name from underscore_case to camelCase.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static string Camelify(string name)
		{
			StringBuilder sb = new StringBuilder(name.Length);

			bool lastWasUnderscore = false;
			for (int i = 0; i < name.Length; i++)
			{
				if (name[i] == '_' && i != 0)
				{
					lastWasUnderscore = true;
				}
				else
				{
					if (lastWasUnderscore)
						sb.Append(char.ToUpperInvariant(name[i]));
					else
						sb.Append(name[i]);

					lastWasUnderscore = false;
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Converts the specified name to one with an uppercase first letter (something to Something).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		public static string UpperFirstLetter(string name)
		{
			if (!string.IsNullOrEmpty(name))
				return char.ToUpperInvariant(name[0]) + name.Substring(1);

			return name;
		}




	}
}
