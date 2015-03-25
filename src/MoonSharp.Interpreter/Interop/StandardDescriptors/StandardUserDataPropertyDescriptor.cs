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
	internal class StandardUserDataPropertyDescriptor
	{
		internal PropertyInfo PropertyInfo { get; private set; }
		internal InteropAccessMode AccessMode { get; private set; }
		internal bool IsStatic { get; private set; }
		internal string Name { get; private set; }

		Func<object, object> m_OptimizedGetter = null;
		Action<object, object> m_OptimizedSetter = null;


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


		internal object GetValue(object obj)
		{
			if (AccessMode == InteropAccessMode.LazyOptimized && m_OptimizedGetter == null)
				OptimizeGetter();

			if (m_OptimizedGetter != null)
			{
				return m_OptimizedGetter(obj);
			}

			// convoluted workaround for --full-aot Mono execution
			object result = PropertyInfo.GetGetMethod().Invoke(IsStatic ? null : obj, null);
			return result;
				
				//PropertyInfo.GetValue(IsStatic ? null : obj, null);
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

		internal void SetValue(object obj, object value, DataType originalType)
		{
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
				throw ScriptRuntimeException.UserDataArgumentTypeMismatch(originalType, PropertyInfo.PropertyType);
			}
			catch (InvalidCastException)
			{
				// optimized setters fall here
				throw ScriptRuntimeException.UserDataArgumentTypeMismatch(originalType, PropertyInfo.PropertyType);
			}
		}
	}
}
