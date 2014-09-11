using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	public class UserDataPropertyDescriptor
	{
		public PropertyInfo PropertyInfo { get; private set; }
		public UserDataDescriptor UserDataDescriptor { get; private set; }
		public bool IsStatic { get; private set; }
		public string Name { get; private set; }

		Func<object, object> m_OptimizedGetter = null;
		Action<object, object> m_OptimizedSetter = null;
		

		internal UserDataPropertyDescriptor(PropertyInfo pi, UserDataDescriptor userDataDescriptor)
		{
			this.PropertyInfo = pi;
			this.UserDataDescriptor = userDataDescriptor;
			this.Name = pi.Name;
			this.IsStatic = (this.PropertyInfo.GetGetMethod() ?? this.PropertyInfo.GetSetMethod()).IsStatic;

			if (userDataDescriptor.OptimizationMode == UserDataOptimizationMode.Precomputed)
			{
				this.OptimizeGetter();
				this.OptimizeSetter();
			}
		}


		public object GetValue(object obj)
		{
			if (UserDataDescriptor.OptimizationMode == UserDataOptimizationMode.Lazy && m_OptimizedGetter == null)
				OptimizeGetter();

			if (m_OptimizedGetter != null)
				return m_OptimizedGetter(obj);

			return PropertyInfo.GetValue(IsStatic ? null : obj, null);
		}

		private void OptimizeGetter()
		{
			if (PropertyInfo.CanRead)
			{
				if (IsStatic)
				{
					var paramExp = Expression.Parameter(typeof(object), "dummy");
					var propAccess = Expression.Property(null, PropertyInfo);
					var castPropAccess = Expression.Convert(propAccess, typeof(object));
					var lambda = Expression.Lambda<Func<object, object>>(castPropAccess, paramExp);
					m_OptimizedGetter = lambda.Compile();
				}
				else
				{
					var paramExp = Expression.Parameter(typeof(object), "obj");
					var castParamExp = Expression.Convert(paramExp, this.UserDataDescriptor.Type);
					var propAccess = Expression.Property(castParamExp, PropertyInfo);
					var castPropAccess = Expression.Convert(propAccess, typeof(object));
					var lambda = Expression.Lambda<Func<object, object>>(castPropAccess, paramExp);
					m_OptimizedGetter = lambda.Compile();
				}
			}
		}

		private void OptimizeSetter()
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
					m_OptimizedSetter = lambda.Compile();
				}
				else
				{
					var paramExp = Expression.Parameter(typeof(object), "obj");
					var paramValExp = Expression.Parameter(typeof(object), "val");
					var castParamExp = Expression.Convert(paramExp, this.UserDataDescriptor.Type);
					var castParamValExp = Expression.Convert(paramValExp, this.PropertyInfo.PropertyType);
					var callExpression = Expression.Call(castParamExp, setterMethod, castParamValExp);
					var lambda = Expression.Lambda<Action<object, object>>(callExpression, paramExp, paramValExp);
					m_OptimizedSetter = lambda.Compile();
				}
			}
		}

		public void SetValue(object obj, object value, DataType originalType)
		{
			try
			{
				if (value is double)
					value = Converter.DoubleToType(PropertyInfo.PropertyType, (double)value);

				if (UserDataDescriptor.OptimizationMode == UserDataOptimizationMode.Lazy && m_OptimizedSetter == null)
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
