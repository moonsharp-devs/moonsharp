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

		public static DynValue TryClrObjectToSimpleMoonSharpValue(this Script script, object obj)
		{
			if (obj == null)
				return DynValue.Nil;

			Type t = obj.GetType();

			if (NumericTypes.Contains(t))
				return DynValue.NewNumber(TypeToDouble(t, obj));

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


		public static DynValue ClrObjectToComplexMoonSharpValue(this Script script, object obj)
		{
			DynValue v = TryClrObjectToSimpleMoonSharpValue(script, obj);

			if (v != null) return v;

			v = script.UserDataRepository.CreateUserData(obj);

			if (v != null) return v;

			if (obj is Type)
			{
				v = script.UserDataRepository.CreateStaticUserData(obj as Type);
			}

			if (obj is System.Collections.IEnumerable)
			{
				var enumer = (System.Collections.IEnumerable)obj;
				return EnumerableIterator.ConvertIterator(script, enumer.GetEnumerator());
			}

			if (obj is System.Collections.IEnumerator)
			{
				var enumer = (System.Collections.IEnumerator)obj;
				return EnumerableIterator.ConvertIterator(script, enumer);
			}

			throw ScriptRuntimeException.ConvertObjectFailed(obj);
		}

		internal static object MoonSharpValueToClrObject(DynValue value)
		{
			switch (value.Type)
			{
				case DataType.Nil:
					return null;
				case DataType.Boolean:
					return value.Boolean;
				case DataType.Number:
					return value.Number;
				case DataType.String:
					return value.String;
				case DataType.Function:
					return value.Function;
				case DataType.Table:
					return value.Table;
				case DataType.Tuple:
					return value.Tuple;
				case DataType.UserData:
					return value.UserData.Object;
				case DataType.ClrFunction:
					return value.Callback;
				default:
					throw ScriptRuntimeException.ConvertObjectFailed(value.Type);
			}
		}

		public static object DoubleToType(Type type, double d)
		{
			type = Nullable.GetUnderlyingType(type) ?? type;

			if (type == typeof(double)) return d;
			if (type == typeof(sbyte)) return (sbyte)d;
			if (type == typeof(byte)) return (byte)d;
			if (type == typeof(short)) return (short)d;
			if (type == typeof(ushort)) return (ushort)d;
			if (type == typeof(int)) return (int)d;
			if (type == typeof(uint)) return (uint)d;
			if (type == typeof(long)) return (long)d;
			if (type == typeof(ulong)) return (ulong)d;
			if (type == typeof(float)) return (float)d;
			if (type == typeof(decimal)) return (decimal)d;
			return d;
		}

		public static double TypeToDouble(Type type, object d)
		{
			if (type == typeof(double)) return (double)d;
			if (type == typeof(sbyte)) return (double)(sbyte)d;
			if (type == typeof(byte)) return (double)(byte)d;
			if (type == typeof(short)) return (double)(short)d;
			if (type == typeof(ushort)) return (double)(ushort)d;
			if (type == typeof(int)) return (double)(int)d;
			if (type == typeof(uint)) return (double)(uint)d;
			if (type == typeof(long)) return (double)(long)d;
			if (type == typeof(ulong)) return (double)(ulong)d;
			if (type == typeof(float)) return (double)(float)d;
			if (type == typeof(decimal)) return (double)(decimal)d;
			return (double)d;
		}
	}
}
