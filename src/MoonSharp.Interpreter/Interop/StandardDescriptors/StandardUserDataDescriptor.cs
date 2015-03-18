using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop
{
	public class StandardUserDataDescriptor : IUserDataDescriptor
	{
		public string Name { get; private set; }
		public Type Type { get; private set; }
		public InteropAccessMode AccessMode { get; private set; }
		public string FriendlyName { get; private set; }

		private Dictionary<string, StandardUserDataMethodDescriptor> m_Methods = new Dictionary<string, StandardUserDataMethodDescriptor>();
		private Dictionary<string, StandardUserDataPropertyDescriptor> m_Properties = new Dictionary<string, StandardUserDataPropertyDescriptor>();

		protected internal StandardUserDataDescriptor(Type type, InteropAccessMode accessMode, string friendlyName)
		{
			if (MoonSharp.Interpreter.RuntimeAbstraction.Platform.Current.IsAOT())
				accessMode = InteropAccessMode.Reflection;

			if (accessMode == InteropAccessMode.Default)
				accessMode = UserData.DefaultAccessMode;

			Type = type;
			Name = type.FullName;
			AccessMode = accessMode;
			FriendlyName = friendlyName;

			if (AccessMode != InteropAccessMode.HideMembers)
			{
				foreach (ConstructorInfo ci in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
				{
					if (CheckVisibility(ci.GetCustomAttributes(true), ci.IsPublic))
					{
						var md = new StandardUserDataMethodDescriptor(ci, this.AccessMode);
						m_Methods.Add("__new", md);
						break;
					}
				}

				foreach (MethodInfo mi in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Static))
				{
					if (CheckVisibility(mi.GetCustomAttributes(true), mi.IsPublic))
					{
						if (mi.IsSpecialName)
							continue;

						var md = new StandardUserDataMethodDescriptor(mi, this.AccessMode);

						if (m_Methods.ContainsKey(md.Name))
							continue;
						//throw new ArgumentException(string.Format("{0}.{1} has overloads", Name, md.Name));

						m_Methods.Add(md.Name, md);
					}
				}

				foreach (PropertyInfo pi in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
				{
					if (CheckVisibility(pi.GetCustomAttributes(true), IsPropertyInfoPublic(pi)))
					{
						var pd = new StandardUserDataPropertyDescriptor(pi, this.AccessMode);
						m_Properties.Add(pd.Name, pd);
					}
				}
			}
		}

		private bool IsPropertyInfoPublic(PropertyInfo pi)
		{
			MethodInfo getter = pi.GetGetMethod();
			MethodInfo setter = pi.GetSetMethod();

			return (getter != null && getter.IsPublic) || (setter != null && setter.IsPublic);
		}

		private bool CheckVisibility(object[] attributes, bool isPublic)
		{
			MoonSharpVisibleAttribute va = attributes.OfType<MoonSharpVisibleAttribute>().SingleOrDefault();

			if (va != null)
				return va.Visible;
			else
				return isPublic;
		}


		public DynValue Index(Script script, object obj, DynValue index)
		{
			if (index.Type != DataType.String)
				throw ScriptRuntimeException.BadArgument(1, string.Format("userdata<{0}>.__index", this.Name), "string", index.Type.ToLuaTypeString(), false);

			DynValue v = TryIndex(script, obj, index.String);
			if (v == null) v = TryIndex(script, obj, UpperFirstLetter(index.String));
			if (v == null) v = TryIndex(script, obj, Camelify(index.String));
			if (v == null) v = TryIndex(script, obj, UpperFirstLetter(Camelify(index.String)));

			return v;
		}

		protected virtual DynValue TryIndex(Script script, object obj, string indexName)
		{
			StandardUserDataMethodDescriptor mdesc;

			if (m_Methods.TryGetValue(indexName, out mdesc))
				return DynValue.NewCallback(mdesc.GetCallback(script, obj));

			StandardUserDataPropertyDescriptor pdesc;

			if (m_Properties.TryGetValue(indexName, out pdesc))
			{
				object o = pdesc.GetValue(obj);
				return ConversionHelper.ClrObjectToComplexMoonSharpValue(script, o);
			}

			return null;
		}

		public bool SetIndex(Script script, object obj, DynValue index, DynValue value)
		{
			if (index.Type != DataType.String)
				throw ScriptRuntimeException.BadArgument(1, string.Format("userdata<{0}>.__setindex", this.Name), "string", index.Type.ToLuaTypeString(), false);

			bool v = TrySetIndex(script, obj, index.String, value);
			if (!v) v = TrySetIndex(script, obj, UpperFirstLetter(index.String), value);
			if (!v) v = TrySetIndex(script, obj, Camelify(index.String), value);
			if (!v) v = TrySetIndex(script, obj, UpperFirstLetter(Camelify(index.String)), value);

			return v;
		}

		protected virtual bool TrySetIndex(Script script, object obj, string indexName, DynValue value)
		{
			StandardUserDataPropertyDescriptor pdesc;

			if (m_Properties.TryGetValue(indexName, out pdesc))
			{
				object o = ConversionHelper.MoonSharpValueToObjectOfType(value, pdesc.PropertyInfo.PropertyType, null);
				pdesc.SetValue(obj, o, value.Type);
				return true;
			}
			else
			{
				return false;
			}
		}

		internal void Optimize()
		{
			foreach (var m in this.m_Methods.Values)
				m.Optimize();

			foreach (var m in this.m_Properties.Values)
			{
				m.OptimizeGetter();
				m.OptimizeSetter();
			}

		}

		protected static string Camelify(string name)
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

		protected static  string UpperFirstLetter(string name)
		{
			if (!string.IsNullOrEmpty(name))
				return char.ToUpperInvariant(name[0]) + name.Substring(1);

			return name;
		}

		public string AsString(object obj)
		{
			return (obj != null) ? obj.ToString() : null;
		}

		public DynValue MetaIndex(Script script, object obj, string metaname)
		{
			// TODO: meta access to overloaded operators ?
			return null;
		}
	}
}
