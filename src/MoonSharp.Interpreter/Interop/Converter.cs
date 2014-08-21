using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter.Interop
{
	public static class Converter
	{
		static readonly Type[] NumericTypes = 
		{
			typeof(sbyte), 
			typeof(byte), 
			//typeof(char),  // should we ?
			typeof(short), 
			typeof(ushort), 
			typeof(int), 
			typeof(uint), 
			typeof(long), 
			typeof(ulong), 
			typeof(float), 
			typeof(decimal), 
			typeof(double)
		};

		public static bool CheckCallbackSignature(MethodInfo mi)
		{
			ParameterInfo[] pi = mi.GetParameters();

			return (pi.Length == 2 && pi[0].ParameterType == typeof(ScriptExecutionContext)
				&& pi[1].ParameterType == typeof(CallbackArguments) && mi.ReturnType == typeof(DynValue));
		}

		public static DataType? TryGetSimpleScriptTypeForClrType(Type t, out bool nullable)
		{
			Type tv = Nullable.GetUnderlyingType(t);

			nullable = false;

			if (tv != null)
			{
				nullable = true;
				t = tv;
			}

			if (NumericTypes.Contains(t))
				return DataType.Number;

			if (t == typeof(bool))
				return DataType.Boolean;

			if (t == typeof(string) || t == typeof(StringBuilder) || t == typeof(char))
				return DataType.String;

			return null;
		}




		public static DynValue CreateValueFromSupportedObject(this Script script, object obj)
		{
			if (obj == null)
				return DynValue.Nil;

			if (NumericTypes.Contains(obj.GetType()))
				return DynValue.NewNumber((double)obj);

			if (obj is bool)
				return DynValue.NewBoolean((bool)obj);

			if (obj is string || obj is StringBuilder || obj is char)
				return DynValue.NewString(obj.ToString());

			if (obj is Table)
				return DynValue.NewTable((Table)obj);

			if (obj is CallbackFunction)
				return DynValue.NewCallback((CallbackFunction)obj);

			if (obj is Delegate)
			{
				Delegate d = (Delegate)obj;
				MethodInfo mi = d.Method;

				if (CheckCallbackSignature(mi))
					return DynValue.NewCallback((Func<ScriptExecutionContext, CallbackArguments, DynValue>)d); 
			}

			return null;
		}




	}
}
