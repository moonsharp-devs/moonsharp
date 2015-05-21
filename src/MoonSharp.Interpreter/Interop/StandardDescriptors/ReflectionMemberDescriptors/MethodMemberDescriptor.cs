using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Diagnostics;
using MoonSharp.Interpreter.Execution;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Class providing easier marshalling of CLR functions
	/// </summary>
	public class MethodMemberDescriptor : FunctionMemberDescriptorBase, IOptimizableDescriptor
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
		/// Gets a value indicating whether the described method is a constructor
		/// </summary>
		public bool IsConstructor { get; private set; }


		private Func<object, object[], object> m_OptimizedFunc = null;
		private Action<object, object[]> m_OptimizedAction = null;
		private bool m_IsAction = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="MethodMemberDescriptor"/> class.
		/// </summary>
		/// <param name="methodBase">The MethodBase (MethodInfo or ConstructorInfo) got through reflection.</param>
		/// <param name="accessMode">The interop access mode.</param>
		/// <exception cref="System.ArgumentException">Invalid accessMode</exception>
		public MethodMemberDescriptor(MethodBase methodBase, InteropAccessMode accessMode = InteropAccessMode.Default)
		{
			CheckMethodIsCompatible(methodBase, true);

			IsConstructor = (methodBase is ConstructorInfo);
			this.MethodInfo = methodBase;

			bool isStatic = methodBase.IsStatic || IsConstructor;

			if (IsConstructor)
				m_IsAction = false;
			else
				m_IsAction = ((MethodInfo)methodBase).ReturnType == typeof(void);

			ParameterInfo[] reflectionParams = methodBase.GetParameters();
			ParameterDescriptor[] parameters = reflectionParams.Select(pi => new ParameterDescriptor(pi)).ToArray();

			bool isExtensionMethod = (methodBase.IsStatic && parameters.Length > 0 && methodBase.GetCustomAttributes(typeof(ExtensionAttribute), false).Any());

			base.Initialize(methodBase.Name, isStatic, parameters, isExtensionMethod);

			// adjust access mode
			if (Script.GlobalOptions.Platform.IsRunningOnAOT())
				accessMode = InteropAccessMode.Reflection;

			if (accessMode == InteropAccessMode.Default)
				accessMode = UserData.DefaultAccessMode;

			if (accessMode == InteropAccessMode.HideMembers)
				throw new ArgumentException("Invalid accessMode");

			if (parameters.Any(p => p.Type.IsByRef))
				accessMode = InteropAccessMode.Reflection;

			this.AccessMode = accessMode;

			if (AccessMode == InteropAccessMode.Preoptimized)
				((IOptimizableDescriptor)this).Optimize();
		}

		/// <summary>
		/// Tries to create a new MethodMemberDescriptor, returning 
		/// <c>null</c> in case the method is not
		/// visible to script code.
		/// </summary>
		/// <param name="methodBase">The MethodBase.</param>
		/// <param name="accessMode">The <see cref="InteropAccessMode" /></param>
		/// <param name="forceVisibility">if set to <c>true</c> forces visibility.</param>
		/// <returns>
		/// A new MethodMemberDescriptor or null.
		/// </returns>
		public static MethodMemberDescriptor TryCreateIfVisible(MethodBase methodBase, InteropAccessMode accessMode, bool forceVisibility = false)
		{
			if (!CheckMethodIsCompatible(methodBase, false))
				return null;

			if (forceVisibility || (methodBase.GetVisibilityFromAttributes() ?? methodBase.IsPublic))
				return new MethodMemberDescriptor(methodBase, accessMode);

			return null;
		}

		/// <summary>
		/// Checks if the method is compatible with a standard descriptor
		/// </summary>
		/// <param name="methodBase">The MethodBase.</param>
		/// <param name="throwException">if set to <c>true</c> an exception with the proper error message is thrown if not compatible.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentException">
		/// Thrown if throwException is <c>true</c> and one of this applies:
		/// The method contains unresolved generic parameters, or has an unresolved generic return type
		/// or
		/// The method contains pointer parameters, or has a pointer return type
		/// </exception>
		public static bool CheckMethodIsCompatible(MethodBase methodBase, bool throwException)
		{
			if (methodBase.ContainsGenericParameters)
			{
				if (throwException) throw new ArgumentException("Method cannot contain unresolved generic parameters");
				return false;
			}

			if (methodBase.GetParameters().Any(p => p.ParameterType.IsPointer))
			{
				if (throwException) throw new ArgumentException("Method cannot contain pointer parameters");
				return false;
			}

			MethodInfo mi = methodBase as MethodInfo;

			if (mi != null)
			{
				if (mi.ReturnType.IsPointer)
				{
					if (throwException) throw new ArgumentException("Method cannot have a pointer return type");
					return false;
				}

				if (mi.ReturnType.IsGenericTypeDefinition)
				{
					if (throwException) throw new ArgumentException("Method cannot have an unresolved generic return type");
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// The internal callback which actually executes the method
		/// </summary>
		/// <param name="script">The script.</param>
		/// <param name="obj">The object.</param>
		/// <param name="context">The context.</param>
		/// <param name="args">The arguments.</param>
		/// <returns></returns>
		public override DynValue Execute(Script script, object obj, ScriptExecutionContext context, CallbackArguments args)
		{
			this.CheckAccess(MemberDescriptorAccess.CanExecute, obj);

			if (AccessMode == InteropAccessMode.LazyOptimized &&
				m_OptimizedFunc == null && m_OptimizedAction == null)
				((IOptimizableDescriptor)this).Optimize();

			List<int> outParams = null;
			object[] pars = base.BuildArgumentList(script, obj, context, args, out outParams);
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

			return BuildReturnValue(script, outParams, pars, retv);
		}

		/// <summary>
		/// Called by standard descriptors when background optimization or preoptimization needs to be performed.
		/// </summary>
		/// <exception cref="InternalErrorException">Out/Ref params cannot be precompiled.</exception>
		void IOptimizableDescriptor.Optimize()
		{
			ParameterDescriptor[] parameters = Parameters;

			if (AccessMode == InteropAccessMode.Reflection)
				return;

			MethodInfo methodInfo = this.MethodInfo as MethodInfo;

			if (methodInfo == null)
				return;

			using (PerformanceStatistics.StartGlobalStopwatch(PerformanceCounter.AdaptersCompilation))
			{
				var ep = Expression.Parameter(typeof(object[]), "pars");
				var objinst = Expression.Parameter(typeof(object), "instance");
				var inst = Expression.Convert(objinst, MethodInfo.DeclaringType);

				Expression[] args = new Expression[parameters.Length];

				for (int i = 0; i < parameters.Length; i++)
				{
					if (parameters[i].OriginalType.IsByRef)
					{
						throw new InternalErrorException("Out/Ref params cannot be precompiled.");
					}
					else
					{
						var x = Expression.ArrayIndex(ep, Expression.Constant(i));
						args[i] = Expression.Convert(x, parameters[i].OriginalType);
					}
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
