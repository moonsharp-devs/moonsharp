using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop
{
	internal class UserDataMethodDescriptor
	{
		internal MethodInfo MethodInfo { get; private set; }
		internal UserDataDescriptor UserDataDescriptor { get; private set; }
		internal bool IsStatic { get; private set; }
		internal string Name { get; private set; }

		private Type[] m_Arguments;
		private object[] m_Defaults;
		private Func<object, object[], object> m_OptimizedFunc = null;
		private Action<object, object[]> m_OptimizedAction = null;

		internal UserDataMethodDescriptor(MethodInfo mi, UserDataDescriptor userDataDescriptor)
		{
			this.MethodInfo = mi;
			this.UserDataDescriptor = userDataDescriptor;
			this.Name = mi.Name;
			this.IsStatic = mi.IsStatic;

			m_Arguments = mi.GetParameters().Select(pi => pi.ParameterType).ToArray();
			m_Defaults = mi.GetParameters().Select(pi => pi.DefaultValue).ToArray();

			if (userDataDescriptor.AccessMode == UserDataAccessMode.Preoptimized)
				Optimize();
		}

		internal Func<ScriptExecutionContext, CallbackArguments, DynValue> GetCallback(Script script, object obj)
		{
			return (c, a) => Callback(script, obj, c, a);
		}

		DynValue Callback(Script script, object obj, ScriptExecutionContext context, CallbackArguments args)
		{
			if (UserDataDescriptor.AccessMode == UserDataAccessMode.LazyOptimized &&
				m_OptimizedFunc == null && m_OptimizedAction == null)
				Optimize();

			object[] pars = new object[m_Arguments.Length];

			for (int i = 0; i < pars.Length; i++)
			{
				pars[i] = ConversionHelper.MoonSharpValueToObjectOfType(args[i], m_Arguments[i], m_Defaults[i]);
			}


			object retv = null;

			if (m_OptimizedFunc != null)
				retv = m_OptimizedFunc(obj, pars);
			else if (m_OptimizedAction != null)
				m_OptimizedAction(obj, pars);
			else
				retv = MethodInfo.Invoke(obj, pars);

			return ConversionHelper.ClrObjectToComplexMoonSharpValue(script, retv);
		}

		private void Optimize()
		{
			var ep = Expression.Parameter(typeof(object[]), "pars");
			var objinst = Expression.Parameter(typeof(object), "instance");
			var inst = Expression.Convert(objinst, UserDataDescriptor.Type);

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
				m_OptimizedAction = lambda.Compile();
			}
			else
			{
				var fnc = Expression.Convert(fn, typeof(object));
				var lambda = Expression.Lambda<Func<object, object[], object>>(fnc, objinst, ep);
				m_OptimizedFunc = lambda.Compile();
			}
		}
	}
}
