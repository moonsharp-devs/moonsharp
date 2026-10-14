using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter.Compatibility;
using MoonSharp.Interpreter.Interop.BasicDescriptors;
using MoonSharp.Interpreter.Interop.Converters;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// Class providing marshalling of generic CLR functions that can be called from Lua by passing the type as the first parameter
	/// </summary>
	public class GenericMethodMemberDescriptor : FunctionMemberDescriptorBase
	{
		MethodInfo m_MethodInfo;
		internal int GenericParameterCount { get; private set; }

		public GenericMethodMemberDescriptor(MethodInfo methodInfo)
		{
			m_MethodInfo = methodInfo;

			// Build parameters: first parameters are the generic argument types, then the actual method parameters
			Type[] genericArguments = methodInfo.GetGenericArguments();
			GenericParameterCount = genericArguments.Length;
			ParameterInfo[] actualParameter = methodInfo.GetParameters();
			ParameterDescriptor[] parameters = new ParameterDescriptor[genericArguments.Length + actualParameter.Length];

			for (int i = 0; i < genericArguments.Length; i++)
				parameters[i] = new ParameterDescriptor($"type{i}", typeof(Type));
			for (int i = 0; i < actualParameter.Length; i++)
			{
				ParameterInfo pi = actualParameter[i];
				parameters[i + genericArguments.Length] = !pi.ParameterType.ContainsGenericParameters
					? new ParameterDescriptor(pi)
					: new ParameterDescriptor(
						pi.Name,
						typeof(object),
						!Framework.Do.IsDbNull(pi.DefaultValue),
						pi.DefaultValue,
						pi.IsOut,
						pi.ParameterType.IsByRef,
						pi.ParameterType.IsArray && pi.GetCustomAttributes(typeof(ParamArrayAttribute), true).Any());
			}

			Initialize(methodInfo.Name, methodInfo.IsStatic, parameters, false);
		}

		public override DynValue Execute(Script script, object obj, ScriptExecutionContext context, CallbackArguments args)
		{
			this.CheckAccess(MemberDescriptorAccess.CanExecute, obj);

			// Build generic type list
			Type[] types = new Type[m_MethodInfo.GetGenericArguments().Length];
			int genericArgsIndex = args.IsMethodCall ? 1 : 0;
			for (int i = 0; i < types.Length; i++)
			{
				DynValue typeArg = args.RawGet(genericArgsIndex + i, false);
				if (typeArg == null || typeArg.Type != DataType.UserData)
					throw new ScriptRuntimeException("Generic method {0} requires a Type as parameter {1}", m_MethodInfo.Name, i);

				types[i] = typeArg.UserData.Descriptor?.Type;
				if (types[i] == null)
					throw new ScriptRuntimeException("Generic method {0} requires a valid Type as parameter {1}", m_MethodInfo.Name, i);
			}

			MethodInfo constructedMethod;
			try
			{
				constructedMethod = m_MethodInfo.MakeGenericMethod(types);
			}
			catch (Exception ex)
			{
				throw new ScriptRuntimeException("Failed to construct generic method {0} with types {1}: {2}", m_MethodInfo.Name, string.Join(", ", types.Select(t => t.Name)), ex.Message);
			}

			// Build argument list, skipping the type argument
			List<int> outParams = null;
			ParameterInfo[] methodParams = constructedMethod.GetParameters();
			object[] pars = new object[methodParams.Length];
			int argsIndex = genericArgsIndex + types.Length;
			for (int i = 0; i < pars.Length; i++)
			{
				if (methodParams[i].ParameterType.IsByRef)
				{
					if (outParams == null)
						outParams = new List<int>();
					outParams.Add(i);
				}

				// fill special types (copied from base.BuildArgumentList)
				if (methodParams[i].ParameterType == typeof(Script))
					pars[i] = script;
				else if (methodParams[i].ParameterType == typeof(ScriptExecutionContext))
					pars[i] = context;
				else if (methodParams[i].ParameterType == typeof(CallbackArguments))
					pars[i] = args.SkipMethodCall();
				else if (methodParams[i].IsOut)
					pars[i] = null;
				else if (i == methodParams.Length - 1 && methodParams[i].ParameterType.IsArray && methodParams[i].GetCustomAttributes(typeof(ParamArrayAttribute), true).Any())
				{
					Type varArgsArrayType = methodParams[i].ParameterType;
					Type varArgsElementType = varArgsArrayType.GetElementType();
					List<DynValue> extraArgs = new List<DynValue>();

					while (true)
					{
						DynValue arg = args.RawGet(argsIndex, false);
						argsIndex += 1;
						if (arg != null)
							extraArgs.Add(arg);
						else
							break;
					}

					if (extraArgs.Count == 1)
					{
						DynValue arg = extraArgs[0];

						if (arg.Type == DataType.UserData && arg.UserData.Object != null)
						{
							if (Framework.Do.IsAssignableFrom(varArgsArrayType, arg.UserData.Object.GetType()))
							{
								pars[i] = arg.UserData.Object;
								continue;
							}
						}
					}

					Array vararg = Array.CreateInstance(varArgsElementType, extraArgs.Count);

					for (int ii = 0; ii < extraArgs.Count; ii++)
					{
						vararg.SetValue(ScriptToClrConversions.DynValueToObjectOfType(extraArgs[ii], varArgsElementType,
						null, false), ii);
					}

					pars[i] = vararg;
				}
				else
				{
					DynValue arg = args.RawGet(argsIndex++, false);
					if (arg != null)
						pars[i] = ScriptToClrConversions.DynValueToObjectOfType(arg, methodParams[i].ParameterType, null, false);
					else if (methodParams[i].HasDefaultValue)
						pars[i] = methodParams[i].DefaultValue;
					else
						pars[i] = null;
				}
			}

			try
			{
				object retv = constructedMethod.Invoke(obj, pars);
				return BuildReturnValue(script, outParams, pars, m_MethodInfo.ReturnType == typeof(void) ? DynValue.Void : retv);
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException ?? ex;
			}
		}
	}
}
