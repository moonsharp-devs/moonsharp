using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop
{
	public class UserDataDescriptor
	{
		public string Name { get; private set; }
		public Type Type { get; private set; }
		public UserDataOptimizationMode OptimizationMode { get; private set; }
		public UserDataRepository Repository { get; private set; }

		private Dictionary<string, UserDataMethodDescriptor> m_Methods = new Dictionary<string, UserDataMethodDescriptor>();
		private Dictionary<string, UserDataPropertyDescriptor> m_Properties = new Dictionary<string, UserDataPropertyDescriptor>();

		internal UserDataDescriptor(UserDataRepository repository, Type type, UserDataOptimizationMode optimizationMode)
		{
			Repository = repository;
			Type = type;
			Name = type.FullName;
			OptimizationMode = optimizationMode;

			if (OptimizationMode != UserDataOptimizationMode.HideMembers)
			{
				foreach (MethodInfo mi in type.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
				{
					var md = new UserDataMethodDescriptor(mi, this);

					if (m_Methods.ContainsKey(md.Name))
						throw new ArgumentException(string.Format("{0}.{1} has overloads", Name, md.Name));

					m_Methods.Add(md.Name, md);
				}

				foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
				{
					var pd = new UserDataPropertyDescriptor(pi, this);
					m_Properties.Add(pd.Name, pd);
				}
			}
		}


		public DynValue Index(object obj, string idxname)
		{
			if (m_Methods.ContainsKey(idxname))
			{
				return DynValue.NewCallback(m_Methods[idxname].GetCallback(obj));
			}

			if (m_Properties.ContainsKey(idxname))
			{
				object o = m_Properties[idxname].GetValue(obj);
				return Converter.ClrObjectToComplexMoonSharpValue(this.Repository.OwnerScript, o);
			}

			throw ScriptRuntimeException.UserDataMissingField(this.Name, idxname);
		}

		public void SetIndex(object obj, string idxname, DynValue value)
		{
			if (m_Properties.ContainsKey(idxname))
			{
				object o = Converter.MoonSharpValueToClrObject(value);
				m_Properties[idxname].SetValue(obj, o, value.Type);
			}
			else
			{
				throw ScriptRuntimeException.UserDataMissingField(this.Name, idxname);
			}
		}
	}
}
