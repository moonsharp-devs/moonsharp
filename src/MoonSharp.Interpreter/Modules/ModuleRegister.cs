using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter
{
	public static class ModuleRegister
	{
		public static Table RegisterCoreModules(this Table table, CoreModules modules)
		{
			if (modules.Has(CoreModules.GlobalConsts)) RegisterConstants(table);
			if (modules.Has(CoreModules.TableIterators)) RegisterModuleType<TableIterators>(table);
			if (modules.Has(CoreModules.Metatables)) RegisterModuleType<MetaTableMethods>(table);
			if (modules.Has(CoreModules.String)) RegisterModuleType<StringModule>(table);

			return table;
		}



		public static Table RegisterConstants(this Table table)
		{
			table["_G"] = DynValue.NewTable(table);
			table["_VERSION"] = DynValue.NewString(string.Format("Moon# {0}", 
				Assembly.GetExecutingAssembly().GetName().Version.Major,
				Assembly.GetExecutingAssembly().GetName().Version.Minor));
			table["_MOONSHARP"] = DynValue.NewString(Assembly.GetExecutingAssembly().GetName().Version.ToString());

			return table;
		}



		public static Table RegisterModuleType(this Table gtable, Type t)
		{
			Table table = CreateModuleNamespace(gtable, t);

			foreach (MethodInfo mi in t.GetMethods(BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic).Where(_mi => _mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).Length > 0))
			{
				MoonSharpMethodAttribute attr = (MoonSharpMethodAttribute)mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).First();

				ParameterInfo[] pi = mi.GetParameters();

				if (pi.Length != 2 || pi[0].ParameterType != typeof(ScriptExecutionContext)
					|| pi[1].ParameterType != typeof(CallbackArguments) || mi.ReturnType != typeof(DynValue))
				{
					throw new ArgumentException(string.Format("Method {0} does not have the right signature.", mi.Name));
				}

				Func<ScriptExecutionContext, CallbackArguments, DynValue> func = (Func<ScriptExecutionContext, CallbackArguments, DynValue>)Delegate.CreateDelegate(typeof(Func<ScriptExecutionContext, CallbackArguments, DynValue>), mi);

				string name = (!string.IsNullOrEmpty(attr.Name)) ? attr.Name : mi.Name;

				table[name] = DynValue.NewCallback(func);
			}

			RegisterLuaScripts(table, t);

			return gtable;
		}

		private static void RegisterLuaScripts(Table table, Type t)
		{
			//throw new NotImplementedException();
		}

		private static Table CreateModuleNamespace(Table gtable, Type t)
		{
			MoonSharpModuleAttribute attr = (MoonSharpModuleAttribute)t.GetCustomAttributes(typeof(MoonSharpModuleAttribute), false).First();

			if (string.IsNullOrEmpty(attr.Namespace))
			{
				return gtable;
			}
			else
			{
				Table table = new Table();
				gtable[attr.Namespace] = DynValue.NewTable(table);

				DynValue loaded = gtable.RawGet("_LOADED");

				if (loaded == null || loaded.Type != DataType.Table)
				{
					gtable["_LOADED"] = loaded = DynValue.NewTable();
				}

				loaded.Table[attr.Namespace] = DynValue.NewTable(table);

				return table;
			}
		}

		public static Table RegisterModuleType<T>(this Table table)
		{
			return RegisterModuleType(table, typeof(T));
		}

		public static Table RegisterModuleObject(this Table gtable, object o)
		{
			Type t = o.GetType();
			Table table = CreateModuleNamespace(gtable, t);

			foreach (MethodInfo mi in t.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic).Where(_mi => _mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).Length > 0))
			{
				MoonSharpMethodAttribute attr = (MoonSharpMethodAttribute)mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).First();

				ParameterInfo[] pi = mi.GetParameters();

				if (pi.Length != 2 || pi[0].ParameterType != typeof(ScriptExecutionContext)
					|| pi[1].ParameterType != typeof(CallbackArguments) || mi.ReturnType != typeof(DynValue))
				{
					throw new ArgumentException(string.Format("Method {0} does not have the right signature.", mi.Name));
				}

				Func<ScriptExecutionContext, CallbackArguments, DynValue> func = (Func<ScriptExecutionContext, CallbackArguments, DynValue>)
					Delegate.CreateDelegate(typeof(Func<ScriptExecutionContext, CallbackArguments, DynValue>), o, mi);

				string name = (!string.IsNullOrEmpty(attr.Name)) ? attr.Name : mi.Name;

				table[name] = DynValue.NewCallback(func);
			}

			RegisterLuaScripts(table, t);

			return gtable;
		}


	}
}
