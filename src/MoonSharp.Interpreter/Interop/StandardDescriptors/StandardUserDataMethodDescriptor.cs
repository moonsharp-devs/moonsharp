using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop
{
	public class StandardUserDataMethodDescriptor
	{
		public MethodBase MethodInfo { get; private set; }
		public InteropAccessMode AccessMode { get; private set; }
		public bool IsStatic { get; private set; }
		public string Name { get; private set; }
		public bool IsConstructor { get; private set; }

		private Type[] m_Arguments;
		private object[] m_Defaults;
		private Func<object, object[], object> m_OptimizedFunc = null;
		private Action<object, object[]> m_OptimizedAction = null;
		private bool m_IsAction = false;

		public StandardUserDataMethodDescriptor(MethodBase mi, InteropAccessMode accessMode = InteropAccessMode.Default)
		{
			if (accessMode == InteropAccessMode.Default)
				accessMode = UserData.DefaultAccessMode;

			if (accessMode == InteropAccessMode.HideMembers)
				throw new ArgumentException("Invalid accessMode");

			this.MethodInfo = mi;
			this.AccessMode = accessMode;
			this.Name = mi.Name;

			IsConstructor = (mi is ConstructorInfo);

			this.IsStatic = mi.IsStatic || IsConstructor; // we consider the constructor to be a static method as far interop is concerned.

			if (mi is ConstructorInfo)
			{
				m_IsAction = false;
			}
			else
			{
				m_IsAction = ((MethodInfo)mi).ReturnType == typeof(void);
			}

			m_Arguments = mi.GetParameters().Select(pi => pi.ParameterType).ToArray();
			m_Defaults = mi.GetParameters().Select(pi => pi.DefaultValue).ToArray();

			if (AccessMode == InteropAccessMode.Preoptimized)
				Optimize();
		}

		public Func<ScriptExecutionContext, CallbackArguments, DynValue> GetCallback(Script script, object obj = null)
		{
			return (c, a) => Callback(script, obj, c, a);
		}

		public CallbackFunction GetCallbackFunction(Script script, object obj = null)
		{
			return new CallbackFunction(GetCallback(script, obj), this.Name);
		}

		public DynValue GetCallbackAsDynValue(Script script, object obj = null)
		{
			return DynValue.NewCallback(this.GetCallbackFunction(script, obj));
		}

		public static DynValue CreateCallbackDynValue(Script script, MethodInfo mi, object obj = null)
		{
			var desc = new StandardUserDataMethodDescriptor(mi);
			return desc.GetCallbackAsDynValue(script, obj);
		}

		DynValue Callback(Script script, object obj, ScriptExecutionContext context, CallbackArguments args)
		{
			if (AccessMode == InteropAccessMode.LazyOptimized &&
				m_OptimizedFunc == null && m_OptimizedAction == null)
				Optimize();

			object[] pars = new object[m_Arguments.Length];

			int j = args.IsMethodCall ? 1 : 0;

			for (int i = 0; i < pars.Length; i++)
			{
				if (m_Arguments[i] == typeof(Script))
				{
					pars[i] = script;
				}
				else if (m_Arguments[i] == typeof(ScriptExecutionContext))
				{
					pars[i] = context;
				}
				else if (m_Arguments[i] == typeof(CallbackArguments))
				{
					pars[i] = args.SkipMethodCall();
				}
				else
				{
					var arg = args.RawGet(j, false) ?? DynValue.Void;
					pars[i] = ConversionHelper.MoonSharpValueToObjectOfType(arg, m_Arguments[i], m_Defaults[i]);
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

			return ConversionHelper.ClrObjectToComplexMoonSharpValue(script, retv);
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

				Expression[] args = new Expression[m_Arguments.Length];

				for (int i = 0; i < m_Arguments.Length; i++)
				{
					var x = Expression.ArrayIndex(ep, Expression.Constant(i));
					args[i] = Expression.Convert(x, m_Arguments[i]);
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
	}
}
