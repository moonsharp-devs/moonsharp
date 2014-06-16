using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter
{
	public static class ModuleRegister
	{
		public static Table RegisterModuleType(this Table table, Type t)
		{
			foreach (MethodInfo mi in t.GetMethods(BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public).Where(_mi => _mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).Length > 0))
			{
				MoonSharpMethodAttribute attr = (MoonSharpMethodAttribute)mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).First();

				ParameterInfo[] pi = mi.GetParameters();

				if (pi.Length != 2 || pi[0].ParameterType != typeof(IExecutionContext)
					|| pi[1].ParameterType != typeof(CallbackArguments) || mi.ReturnType != typeof(RValue))
				{
					throw new ArgumentException(string.Format("Method {0} does not have the right signature.", mi.Name));
				}

				Func<IExecutionContext, CallbackArguments, RValue> func = (Func<IExecutionContext, CallbackArguments, RValue>)Delegate.CreateDelegate(typeof(Func<IExecutionContext, CallbackArguments, RValue>), mi);

				string name = (!string.IsNullOrEmpty(attr.Name)) ? attr.Name : mi.Name;

				table[name] = new RValue(new CallbackFunction(func));
			}

			return table;
		}

		public static Table RegisterModuleType<T>(this Table table)
		{
			return RegisterModuleType(table, typeof(T));
		}

		public static Table RegisterModuleObject(this Table table, object o)
		{
			Type t = o.GetType();

			foreach (MethodInfo mi in t.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Public).Where(_mi => _mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).Length > 0))
			{
				MoonSharpMethodAttribute attr = (MoonSharpMethodAttribute)mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).First();

				ParameterInfo[] pi = mi.GetParameters();

				if (pi.Length != 2 || pi[0].ParameterType != typeof(IExecutionContext)
					|| pi[1].ParameterType != typeof(CallbackArguments) || mi.ReturnType != typeof(RValue))
				{
					throw new ArgumentException(string.Format("Method {0} does not have the right signature.", mi.Name));
				}

				Func<IExecutionContext, CallbackArguments, RValue> func = (Func<IExecutionContext, CallbackArguments, RValue>)
					Delegate.CreateDelegate(typeof(Func<IExecutionContext, CallbackArguments, RValue>), o, mi);

				string name = (!string.IsNullOrEmpty(attr.Name)) ? attr.Name : mi.Name;

				table[name] = new RValue(new CallbackFunction(func));
			}

			return table;
		}


	}
}
