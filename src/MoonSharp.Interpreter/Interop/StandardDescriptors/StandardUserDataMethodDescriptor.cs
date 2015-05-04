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
using MoonSharp.Interpreter.Interop.Converters;

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
		/// Gets the type which this extension method extends, null if this is not an extension method.
		/// </summary>
		public Type ExtensionMethodType { get; private set; }
		/// <summary>
		/// Gets a sort discriminant to give consistent overload resolution matching in case of perfectly equal scores
		/// </summary>
		public string SortDiscriminant { get; private set; }
		/// <summary>
		/// Gets the type of the arguments of the underlying CLR function
		/// </summary>
		public ParameterInfo[] Parameters { get; private set; }
		/// <summary>
		/// Gets a value indicating the type of the ParamArray parameter of a var-args function. If the function is not var-args,
		/// null is returned.
		/// </summary>
		public Type VarArgsArrayType { get; private set; }
		/// <summary>
		/// Gets a value indicating the type of the elements of the ParamArray parameter of a var-args function. If the function is not var-args,
		/// null is returned.
		/// </summary>
		public Type VarArgsElementType { get; private set; }
		/// <summary>
		/// If this is a placeholder for a valuetype default ctor, this property is equal to the value type to be constructed.
		/// </summary>
		public Type ValueTypeDefaultCtor { get; private set; }

		private Func<object, object[], object> m_OptimizedFunc = null;
		private Action<object, object[]> m_OptimizedAction = null;
		private bool m_IsAction = false;


		/// <summary>
		/// Tries to create a new StandardUserDataMethodDescriptor, returning <c>null</c> in case the method is not 
		/// visible to script code.
		/// </summary>
		/// <param name="methodBase">The MethodBase.</param>
		/// <param name="accessMode">The <see cref="InteropAccessMode" /></param>
		/// <returns>A new StandardUserDataMethodDescriptor or null.</returns>
		public static StandardUserDataMethodDescriptor TryCreateIfVisible(MethodBase methodBase, InteropAccessMode accessMode)
		{
			if (!CheckMethodIsCompatible(methodBase, false))
				return null;

			if (methodBase.GetVisibilityFromAttributes() ?? methodBase.IsPublic)
				return new StandardUserDataMethodDescriptor(methodBase, accessMode);

			return null;
		}

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="StandardUserDataMethodDescriptor" /> class
		/// representing the default empty ctor for a value type.
		/// </summary>
		/// <param name="valueType">Type of the value.</param>
		/// <param name="accessMode">The interop access mode.</param>
		/// <exception cref="System.ArgumentException">valueType is not a value type</exception>
		public StandardUserDataMethodDescriptor(Type valueType, InteropAccessMode accessMode = InteropAccessMode.Default)
		{
			if (!valueType.IsValueType)
				throw new ArgumentException("valueType is not a value type");

			this.Name = "__new";
			this.MethodInfo = null;
			IsConstructor = true;
			this.IsStatic = true;
			Parameters = new ParameterInfo[0];

			// adjust access mode
			this.AccessMode = InteropAccessMode.Reflection;

			ValueTypeDefaultCtor = valueType;
		}



		/// <summary>
		/// Initializes a new instance of the <see cref="StandardUserDataMethodDescriptor"/> class.
		/// </summary>
		/// <param name="methodBase">The MethodBase (MethodInfo or ConstructorInfo) got through reflection.</param>
		/// <param name="accessMode">The interop access mode.</param>
		/// <exception cref="System.ArgumentException">Invalid accessMode</exception>
		public StandardUserDataMethodDescriptor(MethodBase methodBase, InteropAccessMode accessMode = InteropAccessMode.Default)
		{
			CheckMethodIsCompatible(methodBase, true);

			this.MethodInfo = methodBase;
			this.Name = methodBase.Name;

			IsConstructor = (methodBase is ConstructorInfo);

			this.IsStatic = methodBase.IsStatic || IsConstructor; // we consider the constructor to be a static method as far interop is concerned.

			if (methodBase is ConstructorInfo)
				m_IsAction = false;
			else
				m_IsAction = ((MethodInfo)methodBase).ReturnType == typeof(void);

			Parameters = methodBase.GetParameters();

			if (methodBase.IsStatic && Parameters.Length > 0 && methodBase.GetCustomAttributes(typeof(ExtensionAttribute), false).Any())
			{
				this.ExtensionMethodType = Parameters[0].ParameterType;
			}


			// adjust access mode
			if (Script.GlobalOptions.Platform.IsRunningOnAOT())
				accessMode = InteropAccessMode.Reflection;

			if (accessMode == InteropAccessMode.Default)
				accessMode = UserData.DefaultAccessMode;

			if (accessMode == InteropAccessMode.HideMembers)
				throw new ArgumentException("Invalid accessMode");

			if (Parameters.Any(p => p.ParameterType.IsByRef))
				accessMode = InteropAccessMode.Reflection;

			if (Parameters.Length > 0)
			{
				ParameterInfo plast = Parameters[Parameters.Length - 1];

				if (plast.ParameterType.IsArray && plast.GetCustomAttributes(typeof(ParamArrayAttribute), true).Any())
				{
					VarArgsArrayType = plast.ParameterType;
					VarArgsElementType = plast.ParameterType.GetElementType();
				}
			}


			SortDiscriminant = string.Join(":", Parameters.Select(pi => pi.ParameterType.FullName).ToArray());

			this.AccessMode = accessMode;

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
			if (ValueTypeDefaultCtor != null)
			{
				object vto = Activator.CreateInstance(ValueTypeDefaultCtor);
				return ClrToScriptConversions.ObjectToDynValue(script, vto);
			}

			if (AccessMode == InteropAccessMode.LazyOptimized &&
				m_OptimizedFunc == null && m_OptimizedAction == null)
				Optimize();

			object[] pars = new object[Parameters.Length];

			int j = args.IsMethodCall ? 1 : 0;

			List<int> outParams = null;

			for (int i = 0; i < pars.Length; i++)
			{
				// keep track of out and ref params
				if (Parameters[i].ParameterType.IsByRef)
				{
					if (outParams == null) outParams = new List<int>();
					outParams.Add(i);
				}

				// if an ext method, we have an obj -> fill the first param
				if (ExtensionMethodType != null && obj != null && i == 0)
				{
					pars[i] = obj;
					continue;
				}
				// else, fill types with a supported type
				else if (Parameters[i].ParameterType == typeof(Script))
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
				// else, ignore out params
				else if (Parameters[i].IsOut)
				{
					pars[i] = null;
				}
				else if (i == Parameters.Length - 1 && VarArgsArrayType != null)
				{
					List<DynValue> extraArgs = new List<DynValue>();

					while (true)
					{
						DynValue arg = args.RawGet(j, false);
						j += 1;
						if (arg != null)
							extraArgs.Add(arg);
						else
							break;
					}

					// here we have to worry we already have an array.. damn. We only support this for userdata.
					// remains to be analyzed what's the correct behavior here. For example, let's take a params object[]..
					// given a single table parameter, should it use it as an array or as an object itself ?
					if (extraArgs.Count == 1)
					{
						DynValue arg = extraArgs[0];

						if (arg.Type == DataType.UserData && arg.UserData.Object != null)
						{
							if (VarArgsArrayType.IsAssignableFrom(arg.UserData.Object.GetType()))
							{
								pars[i] = arg.UserData.Object;
								continue;
							}
						}
					}

					// ok let's create an array, and loop
					Array vararg = CreateVarArgArray(extraArgs.Count);

					for (int ii = 0; ii < extraArgs.Count; ii++)
					{
						vararg.SetValue(ScriptToClrConversions.DynValueToObjectOfType(extraArgs[ii], VarArgsElementType,
						null, false), ii);
					}

					pars[i] = vararg;

				}
				// else, convert it
				else
				{
					var arg = args.RawGet(j, false) ?? DynValue.Void;
					pars[i] = ScriptToClrConversions.DynValueToObjectOfType(arg, Parameters[i].ParameterType,
						Parameters[i].DefaultValue, !Parameters[i].DefaultValue.IsDbNull());
					j += 1;
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

			if (outParams == null)
			{
				return ClrToScriptConversions.ObjectToDynValue(script, retv);
			}
			else
			{
				DynValue[] rets = new DynValue[outParams.Count + 1];

				rets[0] = ClrToScriptConversions.ObjectToDynValue(script, retv);

				for (int i = 0; i < outParams.Count; i++)
					rets[i + 1] = ClrToScriptConversions.ObjectToDynValue(script, pars[outParams[i]]);

				return DynValue.NewTuple(rets);
			}
		}

		private Array CreateVarArgArray(int len)
		{
			return Array.CreateInstance(VarArgsElementType, len);
		}

		internal void Optimize()
		{
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

				Expression[] args = new Expression[Parameters.Length];

				for (int i = 0; i < Parameters.Length; i++)
				{
					if (Parameters[i].ParameterType.IsByRef)
					{
						throw new InternalErrorException("Out/Ref params cannot be precompiled.");
					}
					else
					{
						var x = Expression.ArrayIndex(ep, Expression.Constant(i));
						args[i] = Expression.Convert(x, Parameters[i].ParameterType);
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
