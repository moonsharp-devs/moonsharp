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
	/// Class providing easier marshalling of CLR fields
	/// </summary>
	public class StandardUserDataFieldDescriptor
	{
		/// <summary>
		/// Gets the FieldInfo got by reflection
		/// </summary>
		public FieldInfo FieldInfo { get; private set; }
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


		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataPropertyDescriptor"/> class.
		/// </summary>
		/// <param name="fi">The FieldInfo.</param>
		/// <param name="accessMode">The <see cref="InteropAccessMode" /> </param>
		internal StandardUserDataFieldDescriptor(FieldInfo fi, InteropAccessMode accessMode)
		{
			if (Script.GlobalOptions.Platform.IsRunningOnAOT())
				accessMode = InteropAccessMode.Reflection;

			this.FieldInfo = fi;
			this.AccessMode = accessMode;
			this.Name = fi.Name;
			this.IsStatic = this.FieldInfo.IsStatic;

			if (AccessMode == InteropAccessMode.Preoptimized)
			{
				this.OptimizeGetter();
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
				result = FieldInfo.GetValue(obj);

			return ClrToScriptConversions.ObjectToDynValue(script, result);
		}

		internal void OptimizeGetter()
		{
			using (PerformanceStatistics.StartGlobalStopwatch(PerformanceCounter.AdaptersCompilation))
			{
				if (IsStatic)
				{
					var paramExp = Expression.Parameter(typeof(object), "dummy");
					var propAccess = Expression.Field(null, FieldInfo);
					var castPropAccess = Expression.Convert(propAccess, typeof(object));
					var lambda = Expression.Lambda<Func<object, object>>(castPropAccess, paramExp);
					Interlocked.Exchange(ref m_OptimizedGetter, lambda.Compile());
				}
				else
				{
					var paramExp = Expression.Parameter(typeof(object), "obj");
					var castParamExp = Expression.Convert(paramExp, this.FieldInfo.DeclaringType);
					var propAccess = Expression.Field(castParamExp, FieldInfo);
					var castPropAccess = Expression.Convert(propAccess, typeof(object));
					var lambda = Expression.Lambda<Func<object, object>>(castPropAccess, paramExp);
					Interlocked.Exchange(ref m_OptimizedGetter, lambda.Compile());
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
			object value = ScriptToClrConversions.DynValueToObjectOfType(v, this.FieldInfo.FieldType, null, false);

			try
			{
				if (value is double)
					value = NumericConversions.DoubleToType(FieldInfo.FieldType, (double)value);

				FieldInfo.SetValue(IsStatic ? null : obj, value);
			}
			catch (ArgumentException)
			{
				// non-optimized setters fall here
				throw ScriptRuntimeException.UserDataArgumentTypeMismatch(v.Type, FieldInfo.FieldType);
			}
			catch (InvalidCastException)
			{
				// optimized setters fall here
				throw ScriptRuntimeException.UserDataArgumentTypeMismatch(v.Type, FieldInfo.FieldType);
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
