// Portions taken from https://github.com/windowsgamessamples/UnityPorting

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
	// for some reason FXCORE profile in Unity has binding flags.. outside it doesn't.
	// WTF this framework fragmentation is a freaking MESS.
#if NETFX_CORE && !UNITY_5
 	[Flags]
	public enum BindingFlags
	{
		Default,
		Public,
		Instance,
		InvokeMethod,
		NonPublic,
		Static,
		FlattenHierarchy,
		DeclaredOnly
	}
#endif

	public static class AssemblyTools
	{
		public static Assembly GetCallingAssembly()
		{
#if NETFX_CORE
			throw new NotSupportedException("Assembly.GetCallingAssembly is not supported on Windows Store Apps");
#else
			return Assembly.GetCallingAssembly();
#endif
		}
	}

	public static class ReflectionExtensions
	{
#if !NETFX_CORE
		public static bool CheckIsValueType(this Type t)
		{
			return t.IsValueType;
		}

		public static Assembly GetAssembly(this Type t)
		{
			return t.Assembly;
		}
				
		public static Type GetBaseType(this Type t)
		{
			return t.BaseType;
		}

		public static bool CheckIsGenericType(this Type t)
		{
			return t.IsGenericType;
		}
				
		public static bool CheckIsGenericTypeDefinition(this Type t)
		{
			return t.IsGenericTypeDefinition;
		}

		public static bool CheckIsEnum(this Type t)
		{
			return t.IsEnum;
		}
		
		public static bool CheckIsNestedPublic(this Type t)
		{
			return t.IsNestedPublic;
		}
				
		public static bool CheckIsAbstract(this Type t)
		{
			return t.IsAbstract;
		}

		public static bool CheckIsInterface(this Type t)
		{
			return t.IsInterface;
		}
#else
		public static bool CheckIsInterface(this Type t)
		{
			return t.GetTypeInfo().IsInterface;
		}		
		
		public static bool CheckIsNestedPublic(this Type t)
		{
			return t.GetTypeInfo().IsNestedPublic;
		}
		public static bool CheckIsAbstract(this Type t)
		{
			return t.GetTypeInfo().IsAbstract;
		}

		public static bool CheckIsEnum(this Type t)
		{
			return t.GetTypeInfo().IsEnum;
		}

		public static bool CheckIsGenericTypeDefinition(this Type t)
		{
			return t.GetTypeInfo().IsGenericTypeDefinition;
		}

		public static bool CheckIsGenericType(this Type t)
		{
			return t.GetTypeInfo().IsGenericType;
		}

		public static Attribute[] GetCustomAttributes(this Type t, bool inherit)
		{
			return t.GetTypeInfo().GetCustomAttributes(inherit).ToArray();
		}

		public static Attribute[] GetCustomAttributes(this Type t, Type at, bool inherit)
		{
			return t.GetTypeInfo().GetCustomAttributes(at, inherit).ToArray();
		}

		public static Type[] GetInterfaces(this Type t)
		{
			return t.GetTypeInfo().ImplementedInterfaces.ToArray();
		}

		public static bool IsInstanceOfType(this Type t, object o)
		{
			if (o == null)
				return false;

			return t.IsAssignableFrom(o.GetType());
		}

		public static Type GetBaseType(this Type t)
		{
			return t.GetTypeInfo().BaseType;
		}

		public static Assembly GetAssembly(this Type t)
		{
			return t.GetTypeInfo().Assembly;
		}

		public static bool CheckIsValueType(this Type t)
		{
			return t.GetTypeInfo().IsValueType;
		}
		public static MethodInfo GetAddMethod(this EventInfo ei, bool _dummy = false)
		{
			return ei.AddMethod;
		}

		public static MethodInfo GetRemoveMethod(this EventInfo ei, bool _dummy = false)
		{
			return ei.RemoveMethod;
		}

		public static MethodInfo GetGetMethod(this PropertyInfo pi, bool _dummy = false)
		{
			return pi.GetMethod;
		}

		public static MethodInfo GetSetMethod(this PropertyInfo pi, bool _dummy = false)
		{
			return pi.SetMethod;
		}

		public static Type GetInterface(this Type type, string name)
		{
			return type.GetTypeInfo().ImplementedInterfaces.FirstOrDefault(t => t.Name == name);
		}

		public static PropertyInfo[] GetProperties(this Type type, BindingFlags flags = BindingFlags.Default)
		{
			return type.GetTypeInfo().DeclaredProperties != null ? type.GetTypeInfo().DeclaredProperties.ToArray() : new PropertyInfo[0];
		}

		public static PropertyInfo GetProperty(this Type type, string name)
		{
			return type.GetProperties().FirstOrDefault(pi => pi.Name == name);
		}

		public static Type[] GetNestedTypes(this Type type, BindingFlags flags = BindingFlags.Default)
		{
			return type.GetTypeInfo().DeclaredNestedTypes != null ? type.GetTypeInfo().DeclaredNestedTypes.Select(ti => ti.AsType()).ToArray() : new Type[0];
		}

		public static EventInfo[] GetEvents(this Type type, BindingFlags flags = BindingFlags.Default)
		{
			return type.GetTypeInfo().DeclaredEvents != null ? type.GetTypeInfo().DeclaredEvents.ToArray() : new EventInfo[0];
		}


		public static ConstructorInfo[] GetConstructors(this Type type, BindingFlags flags = BindingFlags.Default)
		{
			return type.GetTypeInfo().DeclaredConstructors != null ? type.GetTypeInfo().DeclaredConstructors.ToArray() : new ConstructorInfo[0];
		}


		public static MethodInfo[] GetMethods(this Type type, BindingFlags flags = BindingFlags.Default)
		{
			return type.GetTypeInfo().DeclaredMethods != null ? type.GetTypeInfo().DeclaredMethods.ToArray() : new MethodInfo[0];
		}

		public static MemberInfo[] GetMembers(this Type t, BindingFlags flags = BindingFlags.Default)
		{
			return t.GetTypeInfo().DeclaredMembers != null ? t.GetTypeInfo().DeclaredMembers.ToArray() : new MemberInfo[0];
		}

		public static FieldInfo[] GetFields(this Type t, BindingFlags flags = BindingFlags.Default)
		{
			return t.GetTypeInfo().DeclaredFields != null ? t.GetTypeInfo().DeclaredFields.ToArray() : new FieldInfo[0];
		}

		public static MethodInfo GetMethod(this Type type, string name)
		{
			return GetMethod(type, name, BindingFlags.Default, null);
		}

		public static MethodInfo GetMethod(this Type type, string name, Type[] types)
		{
			return GetMethod(type, name, BindingFlags.Default, types);
		}

		public static MethodInfo GetMethod(this Type t, string name, BindingFlags flags)
		{
			return GetMethod(t, name, flags, null);
		}

		public static MethodInfo GetMethod(Type t, string name, BindingFlags flags, Type[] parameters)
		{
			var ti = t.GetTypeInfo();
			var methods = ti.GetDeclaredMethods(name);
			foreach (var m in methods)
			{
				var plist = m.GetParameters();
				bool match = true;
				foreach (var param in plist)
				{
					bool valid = true;
					if (parameters != null)
					{
						foreach (var ptype in parameters)
							valid &= ptype == param.ParameterType;
					}
					match &= valid;
				}
				if (match)
					return m;
			}
			return null;
		}

		public static Type[] GetGenericArguments(this Type t)
		{
			var ti = t.GetTypeInfo();
			return ti.GenericTypeArguments;
		}

		public static bool IsAssignableFrom(this Type current, Type toCompare)
		{
			return current.GetTypeInfo().IsAssignableFrom(toCompare.GetTypeInfo());
		}

		public static bool IsSubclassOf(this Type type, System.Type parent)
		{
			return parent.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo());
		}
#endif
	}
}
