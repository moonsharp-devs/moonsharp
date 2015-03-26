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

		Func<object, object> m_OptimizedGetter = null;
		Action<object, object> m_OptimizedSetter = null;


		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataPropertyDescriptor"/> class.
		/// </summary>
		/// <param name="pi">The PropertyInfo.</param>
		/// <param name="accessMode">The <see cref="InteropAccessMode" /> </param>
		internal StandardUserDataPropertyDescriptor(PropertyInfo pi, InteropAccessMode accessMode)
		{
			if (Script.GlobalOptions.Platform.IsRunningOnAOT())
				accessMode = InteropAccessMode.Reflection;

			this.PropertyInfo = pi;
			this.AccessMode = accessMode;
			this.Name = pi.Name;
			this.IsStatic = (this.PropertyInfo.GetGetMethod() ?? this.PropertyInfo.GetSetMethod()).IsStatic;

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
			if (AccessMode == InteropAccessMode.LazyOptimized && m_OptimizedGetter == null)
				OptimizeGetter();

			object result = null;

			if (m_OptimizedGetter != null)
				result = m_OptimizedGetter(obj);
			else
				result = PropertyInfo.GetGetMethod().Invoke(IsStatic ? null : obj, null); // convoluted workaround for --full-aot Mono execution

			return ClrToScriptConversions.ObjectToDynValue(script, result);
		}

		internal void OptimizeGetter()
		{
			using (PerformanceStatistics.StartGlobalStopwatch(PerformanceCounter.AdaptersCompilation))
			{
				if (PropertyInfo.CanRead)
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
				if (PropertyInfo.CanWrite)
				{
					MethodInfo setterMethod = PropertyInfo.GetSetMethod();

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
					PropertyInfo.SetValue(IsStatic ? null : obj, value, null);
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
