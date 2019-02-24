using System;
using System.Collections.Generic;

namespace MoonSharp.Interpreter.Interop.Converters
{
	/// <summary>
	/// Static functions to handle conversions of numeric types
	/// </summary>
	internal static class NumericConversions
	{
		static NumericConversions()
		{
			NumericTypesOrdered = new Type[] 
			{
				typeof(double),
				typeof(decimal), 
				typeof(float), 
				typeof(long), 
				typeof(int), 
				typeof(short), 
				typeof(sbyte), 
				typeof(ulong), 
				typeof(uint), 
				typeof(ushort), 
				typeof(byte), 
			};
			NumericTypes = new HashSet<Type>(NumericTypesOrdered);
		}

		/// <summary>
		/// HashSet of numeric types
		/// </summary>
		internal static readonly HashSet<Type> NumericTypes;
		/// <summary>
		/// Array of numeric types in order used for some conversions
		/// </summary>
		internal static readonly Type[] NumericTypesOrdered;

		/// <summary>
		/// Converts a double to another type
		/// </summary>
		internal static object DoubleToType(Type type, double d)
		{
			type = Nullable.GetUnderlyingType(type) ?? type;

            		try
            		{
                		if (type == typeof(double)) return d;
                		if (type == typeof(sbyte)) return Convert.ToSByte(d);
                		if (type == typeof(byte)) return Convert.ToByte(d);
                		if (type == typeof(short)) return Convert.ToInt16(d);
                		if (type == typeof(ushort)) return Convert.ToUInt16(d);
                		if (type == typeof(int)) return Convert.ToInt32(d);
                		if (type == typeof(uint)) return Convert.ToUInt32(d);
                		if (type == typeof(long)) return Convert.ToInt64(d);
                		if (type == typeof(ulong)) return Convert.ToUInt64(d);
                		if (type == typeof(float)) return Convert.ToSingle(d);
                		if (type == typeof(decimal)) return Convert.ToDecimal(d);
            		}
            		catch (Exception)
            		{
            		    
            		}

			return d;
		}

		/// <summary>
		/// Converts a type to double
		/// </summary>
		internal static double TypeToDouble(Type type, object d)
		{
            		if (type != typeof(double) &&
                		type != typeof(sbyte) &&
                		type != typeof(byte) &&
                		type != typeof(short) &&
                		type != typeof(ushort) &&
                		type != typeof(int) &&
                		type != typeof(uint) &&
                		type != typeof(long) &&
                		type != typeof(ulong) &&
                		type != typeof(float) &&
                		type != typeof(decimal))
            		{
                		return (double)d;
            		}

			return Convert.ToDouble(d);
		}



	}
}
