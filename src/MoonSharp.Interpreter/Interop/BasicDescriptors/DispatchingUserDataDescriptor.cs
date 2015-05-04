using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop.StandardDescriptors
{
	public abstract class DispatchingUserDataDescriptor : IUserDataDescriptor
	{
		private int m_ExtMethodsVersion = 0;
		private Dictionary<string, StandardUserDataOverloadedMethodDescriptor> m_MetaMethods = new Dictionary<string, StandardUserDataOverloadedMethodDescriptor>();
		private Dictionary<string, StandardUserDataOverloadedMethodDescriptor> m_Methods = new Dictionary<string, StandardUserDataOverloadedMethodDescriptor>();
		private Dictionary<string, StandardUserDataPropertyDescriptor> m_Properties = new Dictionary<string, StandardUserDataPropertyDescriptor>();
		private Dictionary<string, StandardUserDataFieldDescriptor> m_Fields = new Dictionary<string, StandardUserDataFieldDescriptor>();
		private Dictionary<string, StandardUserDataEventDescriptor> m_Events = new Dictionary<string, StandardUserDataEventDescriptor>();

		/// <summary>
		/// The special name used by CLR for indexer getters
		/// </summary>
		protected const string SPECIALNAME_INDEXER_GET = "get_Item";
		/// <summary>
		/// The special name used by CLR for indexer setters
		/// </summary>
		protected const string SPECIALNAME_INDEXER_SET = "set_Item";

		/// <summary>
		/// The special name used by CLR for explicit cast conversions
		/// </summary>
		protected const string SPECIALNAME_CAST_EXPLICIT = "op_Explicit";
		/// <summary>
		/// The special name used by CLR for implicit cast conversions
		/// </summary>
		protected const string SPECIALNAME_CAST_IMPLICIT = "op_Implicit";

	
		/// <summary>
		/// Gets the name of the descriptor (usually, the name of the type described).
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Gets the type this descriptor refers to
		/// </summary>
		public Type Type { get; private set; }
		/// <summary>
		/// Gets a human readable friendly name of the descriptor
		/// </summary>
		public string FriendlyName { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataDescriptor"/> class.
		/// </summary>
		/// <param name="type">The type this descriptor refers to.</param>
		/// <param name="accessMode">The interop access mode this descriptor uses for members access</param>
		protected DispatchingUserDataDescriptor(Type type, string friendlyName = null)
		{
			Type = type;
			Name = type.FullName;
			FriendlyName = friendlyName ?? type.Name;
		}

		/// <summary>
		/// Adds a constructor to the members list.
		/// </summary>
		/// <param name="desc">The descriptor.</param>
		public void AddConstructor(StandardUserDataMethodDescriptor desc)
		{
			AddMethod("__new", desc);
		}


		/// <summary>
		/// Adds a method to the members list.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="desc">The descriptor.</param>
		public void AddMethod(string name, StandardUserDataMethodDescriptor desc)
		{
			if (desc != null)
			{
				if (m_Methods.ContainsKey(name))
				{
					m_Methods[name].AddOverload(desc);
				}
				else
				{
					m_Methods.Add(name, new StandardUserDataOverloadedMethodDescriptor(name, this.Type, desc));
				}
			}
		}

		/// <summary>
		/// Adds a method to the metamethods list.
		/// </summary>
		/// <param name="name">The name of the metamethod.</param>
		/// <param name="desc">The desc.</param>
		public void AddMetaMethod(string name, StandardUserDataMethodDescriptor desc)
		{
			if (desc != null)
			{
				if (m_MetaMethods.ContainsKey(name))
				{
					m_MetaMethods[name].AddOverload(desc);
				}
				else
				{
					m_MetaMethods.Add(name, new StandardUserDataOverloadedMethodDescriptor(name, this.Type, desc));
				}
			}
		}

		/// <summary>
		/// Adds a property to the member list
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="desc">The descriptor.</param>
		public void AddProperty(string name, StandardUserDataPropertyDescriptor desc)
		{
			if (desc != null)
				m_Properties.Add(name, desc);
		}

		/// <summary>
		/// Adds a field to the member list
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="desc">The descriptor.</param>
		public void AddField(string name, StandardUserDataFieldDescriptor desc)
		{
			if (desc != null)
				m_Fields.Add(name, desc);
		}

		/// <summary>
		/// Adds an event to the member list
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="desc">The descriptor.</param>
		public void AddEvent(string name, StandardUserDataEventDescriptor desc)
		{
			if (desc != null)
				m_Events.Add(name, desc);
		}

		/// <summary>
		/// Performs an "index" "get" operation. This tries to resolve minor variations of member names.
		/// </summary>
		/// <param name="script">The script originating the request</param>
		/// <param name="obj">The object (null if a static request is done)</param>
		/// <param name="index">The index.</param>
		/// <param name="isDirectIndexing">If set to true, it's indexed with a name, if false it's indexed through brackets.</param>
		/// <returns></returns>
		public virtual DynValue Index(Script script, object obj, DynValue index, bool isDirectIndexing)
		{
			if (!isDirectIndexing)
			{
				StandardUserDataOverloadedMethodDescriptor mdesc = m_Methods.GetOrDefault(SPECIALNAME_INDEXER_GET);

				if (mdesc != null)
					return ExecuteIndexer(mdesc, script, obj, index, null);
			}

			index = index.ToScalar();

			if (index.Type != DataType.String)
				throw ScriptRuntimeException.BadArgument(1, string.Format("userdata<{0}>.__index", this.Name), "string", index.Type.ToLuaTypeString(), false);

			DynValue v = TryIndex(script, obj, index.String);
			if (v == null) v = TryIndex(script, obj, UpperFirstLetter(index.String));
			if (v == null) v = TryIndex(script, obj, Camelify(index.String));
			if (v == null) v = TryIndex(script, obj, UpperFirstLetter(Camelify(index.String)));

			if (v == null && m_ExtMethodsVersion < UserData.GetExtensionMethodsChangeVersion())
			{
				m_ExtMethodsVersion = UserData.GetExtensionMethodsChangeVersion();

				v = TryIndexOnExtMethod(script, obj, index.String);
				if (v == null) v = TryIndexOnExtMethod(script, obj, UpperFirstLetter(index.String));
				if (v == null) v = TryIndexOnExtMethod(script, obj, Camelify(index.String));
				if (v == null) v = TryIndexOnExtMethod(script, obj, UpperFirstLetter(Camelify(index.String)));
			}

			return v;
		}


		/// <summary>
		/// Tries to perform an indexing operation by checking newly added extension methods for the given indexName.
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <param name="indexName">Member name to be indexed.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		private DynValue TryIndexOnExtMethod(Script script, object obj, string indexName)
		{
			List<StandardUserDataMethodDescriptor> methods = UserData.GetExtensionMethodsByName(indexName)
						.Where(d => d.ExtensionMethodType != null && d.ExtensionMethodType.IsAssignableFrom(this.Type))
						.ToList();

			if (methods != null && methods.Count > 0)
			{
				var ext = new StandardUserDataOverloadedMethodDescriptor(indexName, this.Type);
				ext.SetExtensionMethodsSnapshot(UserData.GetExtensionMethodsChangeVersion(), methods);
				m_Methods.Add(indexName, ext);
				return DynValue.NewCallback(ext.GetCallback(script, obj));
			}

			return null;
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
				return pdesc.GetValue(script, obj);

			StandardUserDataFieldDescriptor fdesc;

			if (m_Fields.TryGetValue(indexName, out fdesc))
				return fdesc.GetValue(script, obj);

			StandardUserDataEventDescriptor edesc;

			if (m_Events.TryGetValue(indexName, out edesc))
				return edesc.GetValue(script, obj);

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
		public virtual bool SetIndex(Script script, object obj, DynValue index, DynValue value, bool isDirectIndexing)
		{
			if (!isDirectIndexing)
			{
				StandardUserDataOverloadedMethodDescriptor mdesc = m_Methods.GetOrDefault(SPECIALNAME_INDEXER_SET);

				if (mdesc != null)
				{
					ExecuteIndexer(mdesc, script, obj, index, value);
					return true;
				}
			}

			index = index.ToScalar();

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
			StandardUserDataPropertyDescriptor pdesc = m_Properties.GetOrDefault(indexName);

			if (pdesc != null)
			{
				pdesc.SetValue(script, obj, value);
				return true;
			}
			else
			{
				StandardUserDataFieldDescriptor fdesc = m_Fields.GetOrDefault(indexName);

				if (fdesc != null)
				{
					fdesc.SetValue(script, obj, value);
					return true;
				}
				else
				{
					return false;
				}
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

			foreach (var m in this.m_Fields.Values)
				m.OptimizeGetter();
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
		protected static string UpperFirstLetter(string name)
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
		public virtual string AsString(object obj)
		{
			return (obj != null) ? obj.ToString() : null;
		}



		/// <summary>
		/// Executes the specified indexer method.
		/// </summary>
		/// <param name="mdesc">The method descriptor</param>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <param name="index">The indexer parameter</param>
		/// <param name="value">The dynvalue to set on a setter, or null.</param>
		/// <returns></returns>
		/// <exception cref="System.NotImplementedException"></exception>
		protected virtual DynValue ExecuteIndexer(StandardUserDataOverloadedMethodDescriptor mdesc, Script script, object obj, DynValue index, DynValue value)
		{
			var callback = mdesc.GetCallback(script, obj);

			IList<DynValue> values;

			if (index.Type == DataType.Tuple)
			{
				if (value == null)
				{
					values = index.Tuple;
				}
				else
				{
					values = new List<DynValue>(index.Tuple);
					values.Add(value);
				}
			}
			else
			{
				if (value == null)
				{
					values = new DynValue[] { index };
				}
				else
				{
					values = new DynValue[] { index, value };
				}
			}

			CallbackArguments args = new CallbackArguments(values, false);
			ScriptExecutionContext execCtx = script.CreateDynamicExecutionContext();

			return callback(execCtx, args);
		}


		/// <summary>
		/// Gets a "meta" operation on this userdata. If a descriptor does not support this functionality,
		/// it should return "null" (not a nil). 
		/// See <see cref="IUserDataDescriptor.MetaIndex" /> for further details.
		/// 
		/// If a method exists marked with <see cref="MoonSharpUserDataMetamethodAttribute" /> for the specific
		/// metamethod requested, that method is returned.
		/// 
		/// If the above fails, the following dispatching occur:
		/// 
		/// __add, __sub, __mul, __div, __mod and __unm are dispatched to C# operator overloads (if they exist)
		/// __eq is dispatched to System.Object.Equals.
		/// __lt and __le are dispatched IComparable.Compare, if the type implements IComparable or IComparable{object}
		/// __len is dispatched to Length and Count properties, if those exist.
		/// __iterator is handled if the object implements IEnumerable or IEnumerator.
		/// __tonumber is dispatched to implicit or explicit conversion operators to standard numeric types.
		/// __tobool is dispatched to an implicit or explicit conversion operator to bool. If that fails, operator true is used.
		/// 
		/// <param name="script">The script originating the request</param>
		/// <param name="obj">The object (null if a static request is done)</param>
		/// <param name="metaname">The name of the metamember.</param>
		/// </summary>
		/// <returns></returns>
		public virtual DynValue MetaIndex(Script script, object obj, string metaname)
		{
			StandardUserDataOverloadedMethodDescriptor desc = m_MetaMethods.GetOrDefault(metaname);

			if (desc != null)
				return desc.GetCallbackAsDynValue(script, obj);

			switch (metaname)
			{
				case "__add":
					return DispatchMetaOnMethod(script, obj, "op_Addition");
				case "__sub":
					return DispatchMetaOnMethod(script, obj, "op_Subtraction");
				case "__mul":
					return DispatchMetaOnMethod(script, obj, "op_Multiply");
				case "__div":
					return DispatchMetaOnMethod(script, obj, "op_Division");
				case "__mod":
					return DispatchMetaOnMethod(script, obj, "op_Modulus");
				case "__unm":
					return DispatchMetaOnMethod(script, obj, "op_UnaryNegation");
				case "__eq":
					return MultiDispatchEqual(script, obj);
				case "__lt":
					return MultiDispatchLessThan(script, obj);
				case "__le":
					return MultiDispatchLessThanOrEqual(script, obj);
				case "__len":
					return TryDispatchLength(script, obj);
				case "__tonumber":
					return TryDispatchToNumber(script, obj);
				case "__tobool":
					return TryDispatchToBool(script, obj);
				case "__iterator":
					return ClrToScriptConversions.EnumerationToDynValue(script, obj);
				default:
					return null;
			}
		}

		#region MetaMethodsDispatching


		private int PerformComparison(object obj, object p1, object p2)
		{
			IComparable comp = (IComparable)obj;

			if (comp != null)
			{
				if (object.ReferenceEquals(obj, p1))
					return comp.CompareTo(p2);
				else if (object.ReferenceEquals(obj, p2))
					return -comp.CompareTo(p1);
			}

			throw new InternalErrorException("unexpected case");
		}


		private DynValue MultiDispatchLessThanOrEqual(Script script, object obj)
		{
			IComparable comp = obj as IComparable;
			if (comp != null)
			{
				return DynValue.NewCallback(
					(context, args) =>
						DynValue.NewBoolean(PerformComparison(obj, args[0].ToObject(), args[1].ToObject()) <= 0));
			}

			return null;
		}

		private DynValue MultiDispatchLessThan(Script script, object obj)
		{
			IComparable comp = obj as IComparable;
			if (comp != null)
			{
				return DynValue.NewCallback(
					(context, args) =>
						DynValue.NewBoolean(PerformComparison(obj, args[0].ToObject(), args[1].ToObject()) < 0));
			}

			return null;
		}

		private DynValue TryDispatchLength(Script script, object obj)
		{
			if (obj == null) return null;

			var lenprop = m_Properties.GetOrDefault("Length");
			if (lenprop != null) return lenprop.GetGetterCallbackAsDynValue(script, obj);

			var countprop = m_Properties.GetOrDefault("Count");
			if (countprop != null) return countprop.GetGetterCallbackAsDynValue(script, obj);

			return null;
		}


		private DynValue MultiDispatchEqual(Script script, object obj)
		{
			return DynValue.NewCallback(
				(context, args) => DynValue.NewBoolean(CheckEquality(obj, args[0].ToObject(), args[1].ToObject())));
		}


		private bool CheckEquality(object obj, object p1, object p2)
		{
			if (obj != null)
			{
				if (object.ReferenceEquals(obj, p1))
					return obj.Equals(p2);
				else if (object.ReferenceEquals(obj, p2))
					return obj.Equals(p1);
			}

			if (p1 != null) return p1.Equals(p2);
			else if (p2 != null) return p2.Equals(p1);
			else return true;
		}

		private DynValue DispatchMetaOnMethod(Script script, object obj, string methodName)
		{
			StandardUserDataOverloadedMethodDescriptor desc = m_Methods.GetOrDefault(methodName);

			if (desc != null)
				return desc.GetCallbackAsDynValue(script, obj);
			else
				return null;
		}


		private DynValue TryDispatchToNumber(Script script, object obj)
		{
			foreach (Type t in NumericConversions.NumericTypesOrdered)
			{
				var name = t.GetConversionMethodName();
				var v = DispatchMetaOnMethod(script, obj, name);
				if (v != null) return v;
			}
			return null;
		}


		private DynValue TryDispatchToBool(Script script, object obj)
		{
			var name = typeof(bool).GetConversionMethodName();
			var v = DispatchMetaOnMethod(script, obj, name);
			if (v != null) return v;
			return DispatchMetaOnMethod(script, obj, "op_True");
		}

		#endregion

	}
}
