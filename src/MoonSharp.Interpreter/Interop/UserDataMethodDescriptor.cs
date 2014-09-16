using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop
{
	public class UserDataMethodDescriptor
	{
		public MethodInfo MethodInfo { get; private set; }
		public UserDataDescriptor UserDataDescriptor { get; private set; }
		public bool IsStatic { get; private set; }
		public string Name { get; private set; }

		private Type[] m_Arguments;
		private object[] m_Defaults;

		internal UserDataMethodDescriptor(MethodInfo mi, UserDataDescriptor userDataDescriptor)
		{
			this.MethodInfo = mi;
			this.UserDataDescriptor = userDataDescriptor;
			this.Name = mi.Name;
			this.IsStatic = mi.IsStatic;

			m_Arguments = mi.GetParameters().Select(pi => pi.ParameterType).ToArray();
			m_Defaults = mi.GetParameters().Select(pi => pi.DefaultValue).ToArray();
		}


		internal Func<ScriptExecutionContext, CallbackArguments, DynValue> GetCallback(object obj)
		{
			return (c, a) => ReflectionCallback(obj, c, a);
		}

		DynValue ReflectionCallback(object obj, ScriptExecutionContext context, CallbackArguments args)
		{
			object[] pars = new object[m_Arguments.Length];
			
			for (int i = 0; i < pars.Length; i++)
			{
				pars[i] = ConversionHelper.MoonSharpValueToObjectOfType(args[i], m_Arguments[i], m_Defaults[i]);
			}

			object retv = MethodInfo.Invoke(obj, pars);
			return ConversionHelper.ClrObjectToComplexMoonSharpValue(this.UserDataDescriptor.Repository.OwnerScript, retv);
		}
	}
}
