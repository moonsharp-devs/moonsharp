using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Interop;

namespace MoonSharp.Interpreter
{
	public static class ModuleRegister
	{
		public static Table RegisterCoreModules(this Table table, CoreModules modules)
		{
			if (modules.Has(CoreModules.GlobalConsts)) RegisterConstants(table);
			if (modules.Has(CoreModules.TableIterators)) RegisterModuleType<TableIterators>(table);
			if (modules.Has(CoreModules.Basic)) RegisterModuleType<BasicMethods>(table);
			if (modules.Has(CoreModules.Metatables)) RegisterModuleType<MetaTableMethods>(table);
			if (modules.Has(CoreModules.String)) RegisterModuleType<StringModule>(table);
			if (modules.Has(CoreModules.LoadMethods)) RegisterModuleType<LoadMethods>(table);
			if (modules.Has(CoreModules.Table)) RegisterModuleType<TableModule>(table);
			if (modules.Has(CoreModules.Table)) RegisterModuleType<TableModule_Globals>(table);
			if (modules.Has(CoreModules.ErrorHandling)) RegisterModuleType<ErrorHandling>(table);
			if (modules.Has(CoreModules.Math)) RegisterModuleType<MathModule>(table);
			if (modules.Has(CoreModules.Coroutine)) RegisterModuleType<CoroutineMethods>(table);
			if (modules.Has(CoreModules.Bit32)) RegisterModuleType<Bit32Module>(table);
			if (modules.Has(CoreModules.Dynamic)) RegisterModuleType<DynamicModule>(table);

			return table;
		}



		public static Table RegisterConstants(this Table table)
		{
			table.Set("_G", DynValue.NewTable(table));
			table.Set("_VERSION", DynValue.NewString(string.Format("MoonSharp {0}", 
				Assembly.GetExecutingAssembly().GetName().Version)));
			table.Set("_MOONSHARP", DynValue.NewString(Assembly.GetExecutingAssembly().GetName().Version.ToString()));

			return table;
		}



		public static Table RegisterModuleType(this Table gtable, Type t)
		{
			Table table = CreateModuleNamespace(gtable, t);

			foreach (MethodInfo mi in t.GetMethods(BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.NonPublic))
			{
				if (mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).Length > 0)
				{
					MoonSharpMethodAttribute attr = (MoonSharpMethodAttribute)mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).First();

					if (!ConversionHelper.CheckCallbackSignature(mi))
							throw new ArgumentException(string.Format("Method {0} does not have the right signature.", mi.Name));

					Func<ScriptExecutionContext, CallbackArguments, DynValue> func = (Func<ScriptExecutionContext, CallbackArguments, DynValue>)Delegate.CreateDelegate(typeof(Func<ScriptExecutionContext, CallbackArguments, DynValue>), mi);

					string name = (!string.IsNullOrEmpty(attr.Name)) ? attr.Name : mi.Name;

					table.Set(name, DynValue.NewCallback(func));
				}
				else if (mi.Name == "MoonSharpInit")
				{
					object[] args = new object[2] { gtable, table };
					mi.Invoke(null, args);
				}
			}

			foreach (FieldInfo fi in t.GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public | BindingFlags.NonPublic).Where(_mi => _mi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).Length > 0))
			{
				MoonSharpMethodAttribute attr = (MoonSharpMethodAttribute)fi.GetCustomAttributes(typeof(MoonSharpMethodAttribute), false).First();
				string name = (!string.IsNullOrEmpty(attr.Name)) ? attr.Name : fi.Name;

				RegisterScriptField(fi, null, table, t, name);
			}
			foreach (FieldInfo fi in t.GetFields(BindingFlags.Static | BindingFlags.GetField | BindingFlags.Public | BindingFlags.NonPublic).Where(_mi => _mi.GetCustomAttributes(typeof(MoonSharpConstantAttribute), false).Length > 0))
			{
				MoonSharpConstantAttribute attr = (MoonSharpConstantAttribute)fi.GetCustomAttributes(typeof(MoonSharpConstantAttribute), false).First();
				string name = (!string.IsNullOrEmpty(attr.Name)) ? attr.Name : fi.Name;

				RegisterScriptFieldAsConst(fi, null, table, t, name);
			}

			return gtable;
		}

		private static void RegisterScriptFieldAsConst(FieldInfo fi, object o, Table table, Type t, string name)
		{
			if (fi.FieldType == typeof(string))
			{
				string val = fi.GetValue(o) as string;
				table.Set(name, DynValue.NewString(val));
			}
			else if (fi.FieldType == typeof(double))
			{
				double val = (double)fi.GetValue(o);
				table.Set(name, DynValue.NewNumber(val));
			}
			else
			{
				throw new ArgumentException(string.Format("Field {0} does not have the right type - it must be string or double.", name));
			}
		}

		private static void RegisterScriptField(FieldInfo fi, object o, Table table, Type t, string name)
		{
			if (fi.FieldType != typeof(string))
			{
				throw new ArgumentException(string.Format("Field {0} does not have the right type - it must be string.", name));
			}

			string val = fi.GetValue(o) as string;

			DynValue fn = table.OwnerScript.LoadFunction(val, table, name);

			table.Set(name, fn);
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
				Table table = new Table(gtable.OwnerScript);
				gtable.Set(attr.Namespace, DynValue.NewTable(table));

				DynValue package = gtable.RawGet("package");

				if (package == null || package.Type != DataType.Table)
				{
					gtable.Set("package", package = DynValue.NewTable(gtable.OwnerScript));
				}


				DynValue loaded = package.Table.RawGet("loaded");

				if (loaded == null || loaded.Type != DataType.Table)
				{
					package.Table.Set("loaded", loaded = DynValue.NewTable(gtable.OwnerScript));
				}

				loaded.Table.Set(attr.Namespace, DynValue.NewTable(table));

				return table;
			}
		}

		public static Table RegisterModuleType<T>(this Table table)
		{
			return RegisterModuleType(table, typeof(T));
		}


	}
}
