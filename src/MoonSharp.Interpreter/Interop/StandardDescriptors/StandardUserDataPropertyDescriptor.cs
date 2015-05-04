using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Class providing easier marshalling of CLR properties
	/// </summary>
	public class StandardUserDataPropertyDescriptor
	{
		/// <summary>
		/// Gets the PropertyInfo got by reflection
		/// </summary>
		public PropertyInfo PropertyInfo { get; private set; }
		/// <summary>
		/// Gets the <see cref="InteropAccessMode" />
		/// </summary>
		public InteropAccessMode AccessMode { get; private set; }
		/// <summary>
		/// Gets a value indicating whether the described property is static.
		/// </summary>
		public bool IsStatic { get; private set; }
		/// <summary>
		/// Gets the name of the property
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Gets a value indicating whether this instance can be read from
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance can be read from; otherwise, <c>false</c>.
		/// </value>
		public bool CanRead { get { return m_Getter != null; } }
		/// <summary>
		/// Gets a value indicating whether this instance can be written to.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance can be written to; otherwise, <c>false</c>.
		/// </value>
		public bool CanWrite { get { return m_Setter != null; } }


		private MethodInfo m_Getter, m_Setter;
		Func<object, object> m_OptimizedGetter = null;
		Action<object, object> m_OptimizedSetter = null;


		/// <summary>
		/// Tries to create a new StandardUserDataPropertyDescriptor, returning <c>null</c> in case the property is not 
		/// visible to script code.
		/// </summary>
		/// <param name="pi">The PropertyInfo.</param>
		/// <param name="accessMode">The <see cref="InteropAccessMode" /></param>
		/// <returns>A new StandardUserDataPropertyDescriptor or null.</returns>
		public static StandardUserDataPropertyDescriptor TryCreateIfVisible(PropertyInfo pi, InteropAccessMode accessMode)
		{
			MethodInfo getter = pi.GetGetMethod(true);
			MethodInfo setter = pi.GetSetMethod(true);

			bool? pvisible = pi.GetVisibilityFromAttributes();
			bool? gvisible = getter.GetVisibilityFromAttributes();
			bool? svisible = setter.GetVisibilityFromAttributes();

			if (pvisible.HasValue)
			{
				return StandardUserDataPropertyDescriptor.TryCreate(pi, accessMode,
					(gvisible ?? pvisible.Value) ? getter : null,
					(svisible ?? pvisible.Value) ? setter : null);
			}
			else 
			{
				return StandardUserDataPropertyDescriptor.TryCreate(pi, accessMode,
					(gvisible ?? getter.IsPublic) ? getter : null,
					(svisible ?? setter.IsPublic) ? setter : null);
			}
		}

		private static StandardUserDataPropertyDescriptor TryCreate(PropertyInfo pi, InteropAccessMode accessMode, MethodInfo getter, MethodInfo setter)
		{
			if (getter == null && setter == null)
				return null;
			else
				return new StandardUserDataPropertyDescriptor(pi, accessMode, getter, setter);
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataPropertyDescriptor"/> class.
		/// NOTE: This constructor gives get/set visibility based exclusively on the CLR visibility of the 
		/// getter and setter methods.
		/// </summary>
		/// <param name="pi">The pi.</param>
		/// <param name="accessMode">The access mode.</param>
		public StandardUserDataPropertyDescriptor(PropertyInfo pi, InteropAccessMode accessMode)
			: this(pi, accessMode, pi.GetGetMethod(), pi.GetSetMethod())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataPropertyDescriptor" /> class.
		/// </summary>
		/// <param name="pi">The PropertyInfo.</param>
		/// <param name="accessMode">The <see cref="InteropAccessMode" /></param>
		/// <param name="getter">The getter method. Use null to make the property writeonly.</param>
		/// <param name="setter">The setter method. Use null to make the property readonly.</param>
		public StandardUserDataPropertyDescriptor(PropertyInfo pi, InteropAccessMode accessMode, MethodInfo getter, MethodInfo setter)
		{
			if (getter == null && setter == null)
				throw new ArgumentNullException("getter and setter cannot both be null");

			if (Script.GlobalOptions.Platform.IsRunningOnAOT())
				accessMode = InteropAccessMode.Reflection;

			this.PropertyInfo = pi;
			this.AccessMode = accessMode;
			this.Name = pi.Name;

			m_Getter = getter;
			m_Setter = setter;

			this.IsStatic = (m_Getter ?? m_Setter).IsStatic;

			if (AccessMode == InteropAccessMode.Preoptimized)
			{
				this.OptimizeGetter();
				this.OptimizeSetter();
			}
		}


		/// <summary>
		/// Gets the value of the property
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public DynValue GetValue(Script script, object obj)
		{
			if (m_Getter == null)
				throw new ScriptRuntimeException("userdata property '{0}.{1}' cannot be read from.", this.PropertyInfo.DeclaringType.Name, this.Name);

			if (AccessMode == InteropAccessMode.LazyOptimized && m_OptimizedGetter == null)
				OptimizeGetter();

			object result = null;

			if (m_OptimizedGetter != null)
				result = m_OptimizedGetter(obj);
			else
				result = m_Getter.Invoke(IsStatic ? null : obj, null); // convoluted workaround for --full-aot Mono execution

			return ClrToScriptConversions.ObjectToDynValue(script, result);
		}

		internal void OptimizeGetter()
		{
			using (PerformanceStatistics.StartGlobalStopwatch(PerformanceCounter.AdaptersCompilation))
			{
				if (m_Getter != null)
				{
					if (IsStatic)
					{
						var paramExp = Expression.Parameter(typeof(object), "dummy");
						var propAccess = Expression.Property(null, PropertyInfo);
						var castPropAccess = Expression.Convert(propAccess, typeof(object));
						var lambda = Expression.Lambda<Func<object, object>>(castPropAccess, paramExp);
						Interlocked.Exchange(ref m_OptimizedGetter, lambda.Compile());
					}
					else
					{
						var paramExp = Expression.Parameter(typeof(object), "obj");
						var castParamExp = Expression.Convert(paramExp, this.PropertyInfo.DeclaringType);
						var propAccess = Expression.Property(castParamExp, PropertyInfo);
						var castPropAccess = Expression.Convert(propAccess, typeof(object));
						var lambda = Expression.Lambda<Func<object, object>>(castPropAccess, paramExp);
						Interlocked.Exchange(ref m_OptimizedGetter, lambda.Compile());
					}
				}
			}
		}

		internal void OptimizeSetter()
		{
			using (PerformanceStatistics.StartGlobalStopwatch(PerformanceCounter.AdaptersCompilation))
			{
				if (m_Setter != null && !(PropertyInfo.DeclaringType.IsValueType))
				{
					MethodInfo setterMethod = PropertyInfo.GetSetMethod(true);

					if (IsStatic)
					{
						var paramExp = Expression.Parameter(typeof(object), "dummy");
						var paramValExp = Expression.Parameter(typeof(object), "val");
						var castParamValExp = Expression.Convert(paramValExp, this.PropertyInfo.PropertyType);
						var callExpression = Expression.Call(setterMethod, castParamValExp);
						var lambda = Expression.Lambda<Action<object, object>>(callExpression, paramExp, paramValExp);
						Interlocked.Exchange(ref m_OptimizedSetter, lambda.Compile());
					}
					else
					{
						var paramExp = Expression.Parameter(typeof(object), "obj");
						var paramValExp = Expression.Parameter(typeof(object), "val");
						var castParamExp = Expression.Convert(paramExp, this.PropertyInfo.DeclaringType);
						var castParamValExp = Expression.Convert(paramValExp, this.PropertyInfo.PropertyType);
						var callExpression = Expression.Call(castParamExp, setterMethod, castParamValExp);
						var lambda = Expression.Lambda<Action<object, object>>(callExpression, paramExp, paramValExp);
						Interlocked.Exchange(ref m_OptimizedSetter, lambda.Compile());
					}
				}
			}
		}

		/// <summary>
		/// Sets the value of the property
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <param name="v">The value to set.</param>
		public void SetValue(Script script, object obj, DynValue v)
		{
			if (m_Setter == null)
				throw new ScriptRuntimeException("userdata property '{0}.{1}' cannot be written to.", this.PropertyInfo.DeclaringType.Name, this.Name);

			object value = ScriptToClrConversions.DynValueToObjectOfType(v, this.PropertyInfo.PropertyType, null, false);

			try
			{
				if (value is double)
					value = NumericConversions.DoubleToType(PropertyInfo.PropertyType, (double)value);

				if (AccessMode == InteropAccessMode.LazyOptimized && m_OptimizedSetter == null)
					OptimizeSetter();

				if (m_OptimizedSetter != null)
				{
					m_OptimizedSetter(obj, value);
				}
				else
				{
					m_Setter.Invoke(IsStatic ? null : obj, new object[] { value }); // convoluted workaround for --full-aot Mono execution
				}
			}
			catch (ArgumentException)
			{
				// non-optimized setters fall here
				throw ScriptRuntimeException.UserDataArgumentTypeMismatch(v.Type, PropertyInfo.PropertyType);
			}
			catch (InvalidCastException)
			{
				// optimized setters fall here
				throw ScriptRuntimeException.UserDataArgumentTypeMismatch(v.Type, PropertyInfo.PropertyType);
			}
		}

		/// <summary>
		/// Gets the getter of the property as a DynValue containing a callback
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public DynValue GetGetterCallbackAsDynValue(Script script, object obj)
		{
			return DynValue.NewCallback((p1, p2) => GetValue(script, obj));
		}
	}
}
