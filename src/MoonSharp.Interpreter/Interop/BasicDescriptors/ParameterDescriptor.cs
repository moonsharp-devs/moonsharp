using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.Interop.BasicDescriptors
{
	/// <summary>
	/// Descriptor of parameters used in <see cref="IOverloadableMemberDescriptor"/> implementations.
	/// </summary>
	public sealed class ParameterDescriptor
	{
		/// <summary>
		/// Gets the name of the parameter
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Gets the type of the parameter
		/// </summary>
		public Type Type { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this instance has a default value.
		/// </summary>
		public bool HasDefaultValue { get; private set; }
		/// <summary>
		/// Gets the default value
		/// </summary>
		public object DefaultValue { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this instance is an out parameter
		/// </summary>
		public bool IsOut { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this instance has been restricted.
		/// </summary>
		public bool HasBeenRestricted { get { return m_OriginalType != null; } }
		/// <summary>
		/// Gets the original type of the parameter before any restriction has been applied.
		/// </summary>
		public Type OriginalType { get { return m_OriginalType ?? Type; } }


		/// <summary>
		/// If the type got restricted, the original type before the restriction.
		/// </summary>
		private Type m_OriginalType = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="ParameterDescriptor"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="type">The type.</param>
		/// <param name="hasDefaultValue">if set to <c>true</c> the parameter has default value.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <param name="isOut">if set to <c>true</c>, is an out param.</param>
		public ParameterDescriptor(string name, Type type, bool hasDefaultValue, object defaultValue, bool isOut)
		{
			Name = name;
			Type = type;
			HasDefaultValue = hasDefaultValue;
			DefaultValue = defaultValue;
			IsOut = isOut;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ParameterDescriptor"/> class.
		/// </summary>
		/// <param name="pi">A ParameterInfo taken from reflection.</param>
		public ParameterDescriptor(ParameterInfo pi)
		{
			Name = pi.Name;
			Type = pi.ParameterType;
			HasDefaultValue = !(pi.DefaultValue.IsDbNull());
			DefaultValue = pi.DefaultValue;
			IsOut = pi.IsOut;
		}


		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format("{0} {1}{2}", Type.Name, Name, HasDefaultValue ? " = ..." : "");
		}

		/// <summary>
		/// Restricts the type of this parameter to a tighter constraint.
		/// Restrictions must be applied before the <see cref="IOverloadableMemberDescriptor"/> containing this
		/// parameter is used in any way.
		/// </summary>
		/// <param name="type">The new type.</param>
		/// <exception cref="System.InvalidOperationException">
		/// Cannot restrict a ref/out param
		/// or
		/// Specified operation is not a restriction
		/// </exception>
		public void RestrictType(Type type)
		{
			if (IsOut || Type.IsByRef)
				throw new InvalidOperationException("Cannot restrict a ref/out param");

			if (!Type.IsAssignableFrom(type))
				throw new InvalidOperationException("Specified operation is not a restriction");

			m_OriginalType = Type;
			Type = type;
		}

	}
}
