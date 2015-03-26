using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop.Converters
{
	internal static class ScriptToClrConversions
	{
		internal const int WEIGHT_MAX_VALUE = 100;
		internal const int WEIGHT_CUSTOM_CONVERTER_MATCH = 100;
		internal const int WEIGHT_EXACT_MATCH = 100;
		internal const int WEIGHT_NIL_TO_NULLABLE = 100;
		internal const int WEIGHT_NIL_TO_REFTYPE = 100;
		internal const int WEIGHT_VOID_WITH_DEFAULT = 50;
		internal const int WEIGHT_VOID_WITHOUT_DEFAULT = 25;
		internal const int WEIGHT_NIL_WITH_DEFAULT = 25;
		internal const int WEIGHT_BOOL_TO_STRING = 5;
		internal const int WEIGHT_NUMBER_TO_STRING = 50;
		internal const int WEIGHT_USERDATA_TO_STRING = 5;
		internal const int WEIGHT_TABLE_CONVERSION = 90;
		internal const int WEIGHT_NUMBER_DOWNCAST = 99;
		internal const int WEIGHT_NO_MATCH = 0;
		internal const int WEIGHT_NO_EXTRA_PARAMS_BONUS = 100;
		internal const int WEIGHT_EXTRA_PARAMS_MALUS = 1;
		internal const int WEIGHT_BYREF_BONUSMALUS = -10;

		/// <summary>
		/// Converts a DynValue to a CLR object [simple conversion]
		/// </summary>
		internal static object DynValueToObject(DynValue value)
		{
			var converter = Script.GlobalOptions.CustomConverters.GetScriptToClrCustomConversion(value.Type, typeof(System.Object));
			if (converter != null)
			{
				var v = converter(value);
				if (v != null)
					return v;
			}

			switch (value.Type)
			{
				case DataType.Void:
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
					if (value.UserData.Object != null)
						return value.UserData.Object;
					else if (value.UserData.Descriptor != null)
						return value.UserData.Descriptor.Type;
					else
						return null;
				case DataType.ClrFunction:
					return value.Callback;
				default:
					throw ScriptRuntimeException.ConvertObjectFailed(value.Type);
			}
		}


		/// <summary>
		/// Converts a DynValue to a CLR object of a specific type
		/// </summary>
		internal static object DynValueToObjectOfType(DynValue value, Type desiredType, object defaultValue, bool isOptional)
		{
			if (desiredType.IsByRef)
				desiredType = desiredType.GetElementType();

			var converter = Script.GlobalOptions.CustomConverters.GetScriptToClrCustomConversion(value.Type, desiredType);
			if (converter != null)
			{
				var v = converter(value);
				if (v != null) return v;
			}

			if (desiredType == typeof(DynValue))
				return value;

			if (desiredType == typeof(object))
				return DynValueToObject(value);

			StringConversions.StringSubtype stringSubType = StringConversions.GetStringSubtype(desiredType);
			string str = null;

			Type nt = Nullable.GetUnderlyingType(desiredType);
			Type nullableType = null;

			if (nt != null)
			{
				nullableType = desiredType;
				desiredType = nt;
			}

			switch (value.Type)
			{
				case DataType.Void:
					if (isOptional)
						return defaultValue;
					else if ((!desiredType.IsValueType) || (nullableType != null))
						return null;
					break;
				case DataType.Nil:
					if (desiredType.IsValueType)
					{
						if (nullableType != null)
							return null;

						if (isOptional)
							return defaultValue;
					}
					else
					{
						return null;
					}
					break;
				case DataType.Boolean:
					if (desiredType == typeof(bool))
						return value.Boolean;
					if (stringSubType != StringConversions.StringSubtype.None)
						str = value.Boolean.ToString();
					break;
				case DataType.Number:
					if (NumericConversions.NumericTypes.Contains(desiredType))
						return NumericConversions.DoubleToType(desiredType, value.Number);
					if (stringSubType != StringConversions.StringSubtype.None)
						str = value.Number.ToString();
					break;
				case DataType.String:
					if (stringSubType != StringConversions.StringSubtype.None)
						str = value.String;
					break;
				case DataType.Function:
					if (desiredType == typeof(Closure)) return value.Function;
					else if (desiredType == typeof(ScriptFunctionDelegate)) return value.Function.GetDelegate();
					break;
				case DataType.ClrFunction:
					if (desiredType == typeof(CallbackFunction)) return value.Callback;
					else if (desiredType == typeof(Func<ScriptExecutionContext, CallbackArguments, DynValue>)) return value.Callback.ClrCallback;
					break;
				case DataType.UserData:
					if (value.UserData.Object != null)
					{
						if (desiredType.IsInstanceOfType(value.UserData.Object))
							return value.UserData.Object;
						if (stringSubType != StringConversions.StringSubtype.None)
							str = value.UserData.Object.ToString();
					}
					break;
				case DataType.Table:
					if (desiredType == typeof(Table) || desiredType.IsAssignableFrom(typeof(Table)))
						return value.Table;
					else
					{
						object o = TableConversions.ConvertTableToType(value.Table, desiredType);
						if (o != null)
							return o;
					}
					break;
				case DataType.Tuple:
					break;
			}

			if (stringSubType != StringConversions.StringSubtype.None && str != null)
				return StringConversions.ConvertString(stringSubType, str, desiredType, value.Type);

			throw ScriptRuntimeException.ConvertObjectFailed(value.Type, desiredType);
		}

		/// <summary>
		/// Gets a relative weight of how much the conversion is matching the given types.
		/// Implementation must follow that of DynValueToObjectOfType.. it's not very DRY in that sense.
		/// However here we are in perf-sensitive path.. TODO : double-check the gain and see if a DRY impl is better.
		/// </summary>
		internal static int DynValueToObjectOfTypeWeight(DynValue value, Type desiredType, bool isOptional)
		{
			if (desiredType.IsByRef)
				desiredType = desiredType.GetElementType();

			var customConverter = Script.GlobalOptions.CustomConverters.GetScriptToClrCustomConversion(value.Type, desiredType);
			if (customConverter != null)
				return WEIGHT_CUSTOM_CONVERTER_MATCH;

			if (desiredType == typeof(DynValue))
				return WEIGHT_EXACT_MATCH;

			if (desiredType == typeof(object))
				return WEIGHT_EXACT_MATCH;

			StringConversions.StringSubtype stringSubType = StringConversions.GetStringSubtype(desiredType);
			
			Type nt = Nullable.GetUnderlyingType(desiredType);
			Type nullableType = null;

			if (nt != null)
			{
				nullableType = desiredType;
				desiredType = nt;
			}

			switch (value.Type)
			{
				case DataType.Void:
					if (isOptional)
						return WEIGHT_VOID_WITH_DEFAULT;
					else if ((!desiredType.IsValueType) || (nullableType != null))
						return WEIGHT_VOID_WITHOUT_DEFAULT;
					break;
				case DataType.Nil:
					if (desiredType.IsValueType)
					{
						if (nullableType != null)
							return WEIGHT_NIL_TO_NULLABLE;

						if (isOptional)
							return WEIGHT_NIL_WITH_DEFAULT;
					}
					else
					{
						return WEIGHT_NIL_TO_REFTYPE;
					}
					break;
				case DataType.Boolean:
					if (desiredType == typeof(bool))
						return WEIGHT_EXACT_MATCH;
					if (stringSubType != StringConversions.StringSubtype.None)
						return WEIGHT_BOOL_TO_STRING;
					break;
				case DataType.Number:
					if (NumericConversions.NumericTypes.Contains(desiredType))
						return GetNumericTypeWeight(desiredType);
					if (stringSubType != StringConversions.StringSubtype.None)
						return WEIGHT_NUMBER_TO_STRING;
					break;
				case DataType.String:
					if (stringSubType != StringConversions.StringSubtype.None)
						return WEIGHT_EXACT_MATCH;
					break;
				case DataType.Function:
					if (desiredType == typeof(Closure)) return WEIGHT_EXACT_MATCH;
					else if (desiredType == typeof(ScriptFunctionDelegate)) return WEIGHT_EXACT_MATCH;
					break;
				case DataType.ClrFunction:
					if (desiredType == typeof(CallbackFunction)) return WEIGHT_EXACT_MATCH;
					else if (desiredType == typeof(Func<ScriptExecutionContext, CallbackArguments, DynValue>)) return WEIGHT_EXACT_MATCH;
					break;
				case DataType.UserData:
					if (value.UserData.Object != null)
					{
						if (desiredType.IsInstanceOfType(value.UserData.Object))
							return WEIGHT_EXACT_MATCH;
						if (stringSubType != StringConversions.StringSubtype.None)
							return WEIGHT_USERDATA_TO_STRING;
					}
					break;
				case DataType.Table:
					if (desiredType == typeof(Table) || desiredType.IsAssignableFrom(typeof(Table)))
						return WEIGHT_EXACT_MATCH;
					else if (TableConversions.CanConvertTableToType(value.Table, desiredType))
						return WEIGHT_TABLE_CONVERSION;
					break;
				case DataType.Tuple:
					break;
			}

			return WEIGHT_NO_MATCH;
		}

		private static int GetNumericTypeWeight(Type desiredType)
		{
			if (desiredType == typeof(double) || desiredType == typeof(decimal))
				return WEIGHT_EXACT_MATCH;
			else
				return WEIGHT_NUMBER_DOWNCAST;
		}




	}
}
