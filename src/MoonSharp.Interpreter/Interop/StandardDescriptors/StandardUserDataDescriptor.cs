using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Interop.Converters;
using MoonSharp.Interpreter.Interop.StandardDescriptors;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Standard descriptor for userdata types.
	/// </summary>
	public class StandardUserDataDescriptor : IUserDataDescriptor
	{
		/// <summary>
		/// Gets the name of the descriptor (usually, the name of the type described).
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Gets the type this descriptor refers to
		/// </summary>
		public Type Type { get; private set; }
		/// <summary>
		/// Gets the interop access mode this descriptor uses for members access
		/// </summary>
		public InteropAccessMode AccessMode { get; private set; }
		/// <summary>
		/// Gets a human readable friendly name of the descriptor
		/// </summary>
		public string FriendlyName { get; private set; }

		private Dictionary<string, StandardUserDataOverloadedMethodDescriptor> m_Methods = new Dictionary<string, StandardUserDataOverloadedMethodDescriptor>();
		private Dictionary<string, StandardUserDataPropertyDescriptor> m_Properties = new Dictionary<string, StandardUserDataPropertyDescriptor>();

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataDescriptor"/> class.
		/// </summary>
		/// <param name="type">The type this descriptor refers to.</param>
		/// <param name="accessMode">The interop access mode this descriptor uses for members access</param>
		/// <param name="friendlyName">A human readable friendly name of the descriptor.</param>
		protected internal StandardUserDataDescriptor(Type type, InteropAccessMode accessMode, string friendlyName)
		{
			if (Script.GlobalOptions.Platform.IsRunningOnAOT())
				accessMode = InteropAccessMode.Reflection;

			if (accessMode == InteropAccessMode.Default)
				accessMode = UserData.DefaultAccessMode;

			Type = type;
			Name = type.FullName;
			AccessMode = accessMode;
			FriendlyName = friendlyName;

			if (AccessMode != InteropAccessMode.HideMembers)
			{
				// get first constructor 
				StandardUserDataOverloadedMethodDescriptor constructors = null;

				foreach (ConstructorInfo ci in type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
				{
					if (CheckVisibility(ci.GetCustomAttributes(true), ci.IsPublic))
					{
						var md = new StandardUserDataMethodDescriptor(ci, this.AccessMode);
						if (constructors == null) constructors = new StandardUserDataOverloadedMethodDescriptor();
						constructors.AddOverload(md);
					}
				}

				if (constructors != null) m_Methods.Add("__new", constructors);

				// get methods
				foreach (MethodInfo mi in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
				{
					if (CheckVisibility(mi.GetCustomAttributes(true), mi.IsPublic))
					{
						if (mi.IsSpecialName)
							continue;

						if (!StandardUserDataMethodDescriptor.CheckMethodIsCompatible(mi, false))
							continue;

						var md = new StandardUserDataMethodDescriptor(mi, this.AccessMode);

						if (m_Methods.ContainsKey(md.Name))
						{
							m_Methods[md.Name].AddOverload(md);
						}
						else
						{
							m_Methods.Add(md.Name, new StandardUserDataOverloadedMethodDescriptor(md));
						}
					}
				}

				// get properties
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


		/// <summary>
		/// Performs an "index" "get" operation. This tries to resolve minor variations of member names.
		/// </summary>
		/// <param name="script">The script originating the request</param>
		/// <param name="obj">The object (null if a static request is done)</param>
		/// <param name="index">The index.</param>
		/// <param name="isDirectIndexing">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
		/// <returns></returns>
		public DynValue Index(Script script, object obj, DynValue index, bool isDirectIndexing)
		{
			if (index.Type != DataType.String)
				throw ScriptRuntimeException.BadArgument(1, string.Format("userdata<{0}>.__index", this.Name), "string", index.Type.ToLuaTypeString(), false);

			DynValue v = TryIndex(script, obj, index.String);
			if (v == null) v = TryIndex(script, obj, UpperFirstLetter(index.String));
			if (v == null) v = TryIndex(script, obj, Camelify(index.String));
			if (v == null) v = TryIndex(script, obj, UpperFirstLetter(Camelify(index.String)));

			return v;
		}

		/// <summary>
		/// Tries to perform an indexing operation by checking methods and properties for the given indexName
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <param name="indexName">Member name to be indexed.</param>
		/// <returns></returns>
		protected virtual DynValue TryIndex(Script script, object obj, string indexName)
		{
			StandardUserDataOverloadedMethodDescriptor mdesc;

			if (m_Methods.TryGetValue(indexName, out mdesc))
				return DynValue.NewCallback(mdesc.GetCallback(script, obj));

			StandardUserDataPropertyDescriptor pdesc;

			if (m_Properties.TryGetValue(indexName, out pdesc))
			{
				object o = pdesc.GetValue(obj);
				return ClrToScriptConversions.ObjectToDynValue(script, o);
			}

			return null;
		}

		/// <summary>
		/// Performs an "index" "set" operation. This tries to resolve minor variations of member names.
		/// </summary>
		/// <param name="script">The script originating the request</param>
		/// <param name="obj">The object (null if a static request is done)</param>
		/// <param name="index">The index.</param>
		/// <param name="value">The value to be set</param>
		/// <param name="isDirectIndexing">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
		/// <returns></returns>
		public bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing)
		{
			if (index.Type != DataType.String)
				throw ScriptRuntimeException.BadArgument(1, string.Format("userdata<{0}>.__setindex", this.Name), "string", index.Type.ToLuaTypeString(), false);

			bool v = TrySetIndex(script, obj, index.String, value);
			if (!v) v = TrySetIndex(script, obj, UpperFirstLetter(index.String), value);
			if (!v) v = TrySetIndex(script, obj, Camelify(index.String), value);
			if (!v) v = TrySetIndex(script, obj, UpperFirstLetter(Camelify(index.String)), value);

			return v;
		}

		/// <summary>
		/// Tries to perform an indexing "set" operation by checking methods and properties for the given indexName
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <param name="indexName">Member name to be indexed.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected virtual bool TrySetIndex(Script script, object obj, string indexName, DynValue value)
		{
			StandardUserDataPropertyDescriptor pdesc;

			if (m_Properties.TryGetValue(indexName, out pdesc))
			{
				object o = ScriptToClrConversions.DynValueToObjectOfType(value, pdesc.PropertyInfo.PropertyType, null, false);
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

		/// <summary>
		/// Converts the specified name from underscore_case to camelCase.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Converts the specified name to one with an uppercase first letter (something to Something).
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns></returns>
		protected static  string UpperFirstLetter(string name)
		{
			if (!string.IsNullOrEmpty(name))
				return char.ToUpperInvariant(name[0]) + name.Substring(1);

			return name;
		}

		/// <summary>
		/// Converts this userdata to string
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public string AsString(object obj)
		{
			return (obj != null) ? obj.ToString() : null;
		}

		/// <summary>
		/// Gets the value of an hypothetical metatable for this userdata.
		/// NOT SUPPORTED YET.
		/// </summary>
		/// <param name="script">The script originating the request</param>
		/// <param name="obj">The object (null if a static request is done)</param>
		/// <param name="metaname">The name of the metamember.</param>
		/// <returns></returns>
		public DynValue MetaIndex(Script script, object obj, string metaname)
		{
			// TODO: meta access to overloaded operators ?
			return null;
		}
	}
}
