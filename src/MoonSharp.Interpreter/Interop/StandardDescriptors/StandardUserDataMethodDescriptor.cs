using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Interop.Converters;
using MoonSharp.Interpreter.Interop.StandardDescriptors;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Class providing easier marshalling of CLR functions
	/// </summary>
	public class StandardUserDataMethodDescriptor : IComparable<StandardUserDataMethodDescriptor>
	{
		/// <summary>
		/// Gets the method information (can be a MethodInfo or ConstructorInfo)
		/// </summary>
		public MethodBase MethodInfo { get; private set; }
		/// <summary>
		/// Gets the access mode used for interop
		/// </summary>
		public InteropAccessMode AccessMode { get; private set; }
		/// <summary>
		/// Gets a value indicating whether the described method is static.
		/// </summary>
		public bool IsStatic { get; private set; }
		/// <summary>
		/// Gets the name of the described method
		/// </summary>
		public string Name { get; private set; }
		/// <summary>
		/// Gets a value indicating whether the described method is a constructor
		/// </summary>
		public bool IsConstructor { get; private set; }
		/// <summary>
		/// Gets a sort discriminant to give consistent overload resolution matching in case of perfectly equal scores
		/// </summary>
		public string SortDiscriminant { get; private set; }
		/// <summary>
		/// Gets the type of the arguments of the underlying CLR function
		/// </summary>
		public ParameterInfo[] Parameters { get; private set; }


		private Func<object, object[], object> m_OptimizedFunc = null;
		private Action<object, object[]> m_OptimizedAction = null;
		private bool m_IsAction = false;


		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataMethodDescriptor"/> class.
		/// </summary>
		/// <param name="methodBase">The MethodBase (MethodInfo or ConstructorInfo) got through reflection.</param>
		/// <param name="accessMode">The interop access mode.</param>
		/// <exception cref="System.ArgumentException">Invalid accessMode</exception>
		public StandardUserDataMethodDescriptor(MethodBase methodBase, InteropAccessMode accessMode = InteropAccessMode.Default)
		{
			CheckMethodIsCompatible(methodBase, true);

			if (Script.GlobalOptions.Platform.IsRunningOnAOT())
				accessMode = InteropAccessMode.Reflection;

			if (accessMode == InteropAccessMode.Default)
				accessMode = UserData.DefaultAccessMode;

			if (accessMode == InteropAccessMode.HideMembers)
				throw new ArgumentException("Invalid accessMode");

			this.MethodInfo = methodBase;
			this.AccessMode = accessMode;
			this.Name = methodBase.Name;

			IsConstructor = (methodBase is ConstructorInfo);

			this.IsStatic = methodBase.IsStatic || IsConstructor; // we consider the constructor to be a static method as far interop is concerned.

			if (methodBase is ConstructorInfo)
				m_IsAction = false;
			else
				m_IsAction = ((MethodInfo)methodBase).ReturnType == typeof(void);

			Parameters = methodBase.GetParameters();

			SortDiscriminant = string.Join(":", Parameters.Select(pi => pi.ParameterType.FullName).ToArray());

			if (AccessMode == InteropAccessMode.Preoptimized)
				Optimize();
		}

		/// <summary>
		/// Checks if the method is compatible with a standard descriptor
		/// </summary>
		/// <param name="methodBase">The MethodBase.</param>
		/// <param name="throwException">if set to <c>true</c> an exception with the proper error message is thrown if not compatible.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">
		/// Method cannot contain unresolved generic parameters
		/// or
		/// Method cannot contain by-ref parameters
		/// </exception>
		public static bool CheckMethodIsCompatible(MethodBase methodBase, bool throwException)
		{
			if (methodBase.ContainsGenericParameters)
			{
				if (throwException) throw new ArgumentException("Method cannot contain unresolved generic parameters");
				return false;
			}

			if (methodBase.GetParameters().Any(pi => pi.ParameterType.IsByRef))
			{
				if (throwException) throw new ArgumentException("Method cannot contain by-ref parameters");
				return false;
			}

			return true;
		}



		/// <summary>
		/// Gets a callback function as a delegate
		/// </summary>
		/// <param name="script">The script for which the callback must be generated.</param>
		/// <param name="obj">The object (null for static).</param>
		/// <returns></returns>
		public Func<ScriptExecutionContext, CallbackArguments, DynValue> GetCallback(Script script, object obj = null)
		{
			return (c, a) => Callback(script, obj, c, a);
		}

		/// <summary>
		/// Gets the callback function.
		/// </summary>
		/// <param name="script">The script for which the callback must be generated.</param>
		/// <param name="obj">The object (null for static).</param>
		/// <returns></returns>
		public CallbackFunction GetCallbackFunction(Script script, object obj = null)
		{
			return new CallbackFunction(GetCallback(script, obj), this.Name);
		}

		/// <summary>
		/// Gets the callback function as a DynValue.
		/// </summary>
		/// <param name="script">The script for which the callback must be generated.</param>
		/// <param name="obj">The object (null for static).</param>
		/// <returns></returns>
		public DynValue GetCallbackAsDynValue(Script script, object obj = null)
		{
			return DynValue.NewCallback(this.GetCallbackFunction(script, obj));
		}

		/// <summary>
		/// Creates a callback DynValue starting from a MethodInfo.
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="mi">The mi.</param>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public static DynValue CreateCallbackDynValue(Script script, MethodInfo mi, object obj = null)
		{
			var desc = new StandardUserDataMethodDescriptor(mi);
			return desc.GetCallbackAsDynValue(script, obj);
		}

		/// <summary>
		/// The internal callback which actually executes the method
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <param name="context">The context.</param>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		internal DynValue Callback(Script script, object obj, ScriptExecutionContext context, CallbackArguments args)
		{
			if (AccessMode == InteropAccessMode.LazyOptimized &&
				m_OptimizedFunc == null && m_OptimizedAction == null)
				Optimize();

			object[] pars = new object[Parameters.Length];

			int j = args.IsMethodCall ? 1 : 0;

			for (int i = 0; i < pars.Length; i++)
			{
				if (Parameters[i].ParameterType == typeof(Script))
				{
					pars[i] = script;
				}
				else if (Parameters[i].ParameterType == typeof(ScriptExecutionContext))
				{
					pars[i] = context;
				}
				else if (Parameters[i].ParameterType == typeof(CallbackArguments))
				{
					pars[i] = args.SkipMethodCall();
				}
				else
				{
					var arg = args.RawGet(j, false) ?? DynValue.Void;
					pars[i] = ScriptToClrConversions.DynValueToObjectOfType(arg, Parameters[i].ParameterType,
						Parameters[i].DefaultValue, Parameters[i].DefaultValue != System.DBNull.Value);
					j++;
				}
			}


			object retv = null;

			if (m_OptimizedFunc != null)
			{
				retv = m_OptimizedFunc(obj, pars);
			}
			else if (m_OptimizedAction != null)
			{
				m_OptimizedAction(obj, pars);
				retv = DynValue.Void;
			}
			else if (m_IsAction)
			{
				MethodInfo.Invoke(obj, pars);
				retv = DynValue.Void;
			}
			else
			{
				if (IsConstructor)
					retv = ((ConstructorInfo)MethodInfo).Invoke(pars);
				else
					retv = MethodInfo.Invoke(obj, pars);
			}

			return ClrToScriptConversions.ObjectToDynValue(script, retv);
		}

		internal void Optimize()
		{
			MethodInfo methodInfo = this.MethodInfo as MethodInfo;

			if (methodInfo == null)
				return;

			using (PerformanceStatistics.StartGlobalStopwatch(PerformanceCounter.AdaptersCompilation))
			{
				var ep = Expression.Parameter(typeof(object[]), "pars");
				var objinst = Expression.Parameter(typeof(object), "instance");
				var inst = Expression.Convert(objinst, MethodInfo.DeclaringType);

				Expression[] args = new Expression[Parameters.Length];

				for (int i = 0; i < Parameters.Length; i++)
				{
					var x = Expression.ArrayIndex(ep, Expression.Constant(i));
					args[i] = Expression.Convert(x, Parameters[i].ParameterType);
				}

				Expression fn;

				if (IsStatic)
				{
					fn = Expression.Call(methodInfo, args);
				}
				else
				{
					fn = Expression.Call(inst, methodInfo, args);
				}


				if (this.m_IsAction)
				{
					var lambda = Expression.Lambda<Action<object, object[]>>(fn, objinst, ep);
					Interlocked.Exchange(ref m_OptimizedAction, lambda.Compile());
				}
				else
				{
					var fnc = Expression.Convert(fn, typeof(object));
					var lambda = Expression.Lambda<Func<object, object[], object>>(fnc, objinst, ep);
					Interlocked.Exchange(ref m_OptimizedFunc, lambda.Compile());
				}
			}
		}

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
		/// </returns>
		public int CompareTo(StandardUserDataMethodDescriptor other)
		{
			return this.SortDiscriminant.CompareTo(other.SortDiscriminant);
		}
	}
}
