using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop
{
	public class ValueTypeDefaultCtorDescriptor : IOverloadableMemberDescriptor
	{
		/// <summary>
		/// Gets a value indicating whether the described method is static.
		/// </summary>
		public bool IsStatic { get { return true; } }
		/// <summary>
		/// Gets the name of the described method
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// This property is equal to the value type to be constructed.
		/// </summary>
		public Type ValueTypeDefaultCtor { get; private set; }

		/// <summary>
		/// Gets the type of the arguments of the underlying CLR function
		/// </summary>
		public ParameterDescriptor[] Parameters { get; private set; }


		/// <summary>
		/// Gets the type which this extension method extends, null if this is not an extension method.
		/// </summary>
		public Type ExtensionMethodType
		{
			get { return null; }
		}

		/// <summary>
		/// Gets a value indicating the type of the ParamArray parameter of a var-args function. If the function is not var-args,
		/// null is returned.
		/// </summary>
		public Type VarArgsArrayType
		{
			get { return null; }
		}

		/// <summary>
		/// Gets a value indicating the type of the elements of the ParamArray parameter of a var-args function. If the function is not var-args,
		/// null is returned.
		/// </summary>
		public Type VarArgsElementType
		{
			get { return null; }
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="StandardUserDataMethodDescriptor" /> class
		/// representing the default empty ctor for a value type.
		/// </summary>
		/// <param name="valueType">Type of the value.</param>
		/// <param name="accessMode">The interop access mode.</param>
		/// <exception cref="System.ArgumentException">valueType is not a value type</exception>
		public ValueTypeDefaultCtorDescriptor(Type valueType)
		{
			if (!valueType.IsValueType)
				throw new ArgumentException("valueType is not a value type");

			this.Name = "__new";
			this.Parameters = new ParameterDescriptor[0];

			ValueTypeDefaultCtor = valueType;
		}


		public DynValue Execute(Script script, object obj, ScriptExecutionContext context, CallbackArguments args)
		{
			object vto = Activator.CreateInstance(ValueTypeDefaultCtor);
			return ClrToScriptConversions.ObjectToDynValue(script, vto);
		}


		public string SortDiscriminant
		{
			get { return "@.ctor"; }
		}


		public MemberDescriptorAccess MemberAccess
		{
			get { return MemberDescriptorAccess.CanRead | MemberDescriptorAccess.CanExecute; }
		}

		public DynValue GetValue(Script script, object obj)
		{
			object vto = Activator.CreateInstance(ValueTypeDefaultCtor);
			return ClrToScriptConversions.ObjectToDynValue(script, vto);
		}

		public void SetValue(Script script, object obj, DynValue value)
		{
			this.CheckAccess(MemberDescriptorAccess.CanWrite);
		}
	}
}
