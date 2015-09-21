using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Utility class which may be used to set properties on an object of type T, from values contained in a Lua table.
	/// Properties must be decorated with the <see cref="MoonSharpPropertyAttribute"/>.
	/// This is a generic version of <see cref="PropertyTableAssigner"/>.
	/// </summary>
	/// <typeparam name="T">The type of the object.</typeparam>
	public class PropertyTableAssigner<T>
	{
		PropertyTableAssigner m_InternalAssigner;

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyTableAssigner{T}"/> class.
		/// </summary>
		/// <param name="expectedMissingProperties">The expected missing properties, that is expected fields in the table with no corresponding property in the object.</param>
		public PropertyTableAssigner(params string[] expectedMissingProperties)
		{
			m_InternalAssigner = new PropertyTableAssigner(typeof(T), expectedMissingProperties);
		}

		/// <summary>
		/// Adds an expected missing property, that is an expected field in the table with no corresponding property in the object.
		/// </summary>
		/// <param name="name">The name.</param>
		public void AddExpectedMissingProperty(string name)
		{
			m_InternalAssigner.AddExpectedMissingProperty(name);
		}

		/// <summary>
		/// Assigns properties from tables to an object.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="data">The table.</param>
		/// <exception cref="System.ArgumentNullException">Object is null</exception>
		/// <exception cref="ScriptRuntimeException">A field does not correspond to any property and that property is not one of the expected missing ones.</exception>
		public void AssignObject(T obj, Table data)
		{
			m_InternalAssigner.AssignObject(obj, data);
		}

		/// <summary>
		/// Gets the type-unsafe assigner corresponding to this object.
		/// </summary>
		/// <returns></returns>
		public PropertyTableAssigner GetTypeUnsafeAssigner()
		{
			return m_InternalAssigner;
		}
	}


	/// <summary>
	/// Utility class which may be used to set properties on an object from values contained in a Lua table.
	/// Properties must be decorated with the <see cref="MoonSharpPropertyAttribute"/>.
	/// See <see cref="PropertyTableAssigner{T}"/> for a generic compile time type-safe version.
	/// </summary>
	public class PropertyTableAssigner 
	{
		Type m_Type;
		Dictionary<string, PropertyInfo> m_PropertyMap = new Dictionary<string, PropertyInfo>();

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyTableAssigner"/> class.
		/// </summary>
		/// <param name="type">The type of the object.</param>
		/// <param name="expectedMissingProperties">The expected missing properties, that is expected fields in the table with no corresponding property in the object.</param>
		/// <exception cref="System.ArgumentException">
		/// Type cannot be a value type.
		/// </exception>
		public PropertyTableAssigner(Type type, params string[] expectedMissingProperties)
		{
			m_Type = type;

			if (m_Type.IsValueType)
				throw new ArgumentException("Type cannot be a value type.");

			foreach(string property in expectedMissingProperties)
			{
				m_PropertyMap.Add(property, null);
			}

			foreach (PropertyInfo pi in m_Type.GetProperties(BindingFlags.Instance|BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public))
			{
				foreach (MoonSharpPropertyAttribute attr in pi.GetCustomAttributes(true).OfType<MoonSharpPropertyAttribute>())
				{
					string name = attr.Name ?? pi.Name;

					if (m_PropertyMap.ContainsKey(name))
					{
						throw new ArgumentException(string.Format("Type {0} has two definitions for MoonSharp property {1}", m_Type.FullName, name));
					}
					else
					{
						m_PropertyMap.Add(name, pi);
					}
				}
			}

		}

		/// <summary>
		/// Adds an expected missing property, that is an expected field in the table with no corresponding property in the object.
		/// </summary>
		/// <param name="name">The name.</param>
		public void AddExpectedMissingProperty(string name)
		{
			m_PropertyMap.Add(name, null);
		}


		private bool TryAssignProperty(object obj, string name, DynValue value)
		{
			if (m_PropertyMap.ContainsKey(name))
			{
				PropertyInfo pi = m_PropertyMap[name];

				if (pi != null)
				{
					object o = Interop.Converters.ScriptToClrConversions.DynValueToObjectOfType(value,
						pi.PropertyType, null, false);

					pi.GetSetMethod(true).Invoke(obj, new object[] { o });
				}

				return true;
			}

			return false;
		}

		private void AssignProperty(object obj, string name, DynValue value)
		{
			if (TryAssignProperty(obj, name, value)) return;
			if (TryAssignProperty(obj, DescriptorHelpers.UpperFirstLetter(name), value)) return;
			if (TryAssignProperty(obj, DescriptorHelpers.Camelify(name), value)) return;
			if (TryAssignProperty(obj, DescriptorHelpers.UpperFirstLetter(DescriptorHelpers.Camelify(name)), value)) return;

			throw new ScriptRuntimeException("Invalid property {0}", name);
		}

		/// <summary>
		/// Assigns properties from tables to an object.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <param name="data">The table.</param>
		/// <exception cref="System.ArgumentNullException">Object is null</exception>
		/// <exception cref="System.ArgumentException">The object is of an incompatible type.</exception>
		/// <exception cref="ScriptRuntimeException">A field does not correspond to any property and that property is not one of the expected missing ones.</exception>
		public void AssignObject(object obj, Table data)
		{
			if (obj == null)
				throw new ArgumentNullException("Object is null");

			if (!m_Type.IsInstanceOfType(obj))
				throw new ArgumentException(string.Format("Invalid type of object : got '{0}', expected {1}", obj.GetType().FullName, m_Type.FullName));

			foreach (var pair in data.Pairs)
			{
				if (pair.Key.Type != DataType.String)
				{
					throw new ScriptRuntimeException("Invalid property of type {0}", pair.Key.Type.ToErrorTypeString());
				}

				AssignProperty(obj, pair.Key.String, pair.Value);
			}
		}
	}
}
