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
	internal class UserDataMethodDescriptor
	{
		internal MethodInfo MethodInfo { get; private set; }
		internal InteropAccessMode AccessMode { get; private set; }
		internal bool IsStatic { get; private set; }
		internal string Name { get; private set; }

		private Type[] m_Arguments;
		private object[] m_Defaults;
		private Func<object, object[], object> m_OptimizedFunc = null;
		private Action<object, object[]> m_OptimizedAction = null;

		internal UserDataMethodDescriptor(MethodInfo mi, InteropAccessMode accessMode)
		{
			this.MethodInfo = mi;
			this.AccessMode = accessMode;
			this.Name = mi.Name;
			this.IsStatic = mi.IsStatic;

			m_Arguments = mi.GetParameters().Select(pi => pi.ParameterType).ToArray();
			m_Defaults = mi.GetParameters().Select(pi => pi.DefaultValue).ToArray(); 

			if (AccessMode == InteropAccessMode.Preoptimized)
				Optimize();
		}

		internal Func<ScriptExecutionContext, CallbackArguments, DynValue> GetCallback(Script script, object obj)
		{
			return (c, a) => Callback(script, obj, c, a);
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
				else
				{
					pars[i] = ConversionHelper.MoonSharpValueToObjectOfType(args[j], m_Arguments[i], m_Defaults[i]);
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
			}
			else
			{
				retv = MethodInfo.Invoke(obj, pars);
			}

			return ConversionHelper.ClrObjectToComplexMoonSharpValue(script, retv);
		}

		internal void Optimize()
		{
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
					fn = Expression.Call(MethodInfo, args);
				}
				else
				{
					fn = Expression.Call(inst, MethodInfo, args);
				}


				if (MethodInfo.ReturnType == typeof(void))
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
