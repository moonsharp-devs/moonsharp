using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Interop
{
	/// <summary>
	/// A collection of custom converters between MoonSharp types and CLR types.
	/// If a converter function is not specified or returns null, the standard conversion path applies.
	/// </summary>
	public class CustomConvertersCollection 
	{
		private Dictionary<Type, Func<DynValue, object>>[] m_Script2Clr = new Dictionary<Type, Func<DynValue, object>>[(int)LuaTypeExtensions.MaxConvertibleTypes + 1];
		private Dictionary<Type, Func<object, DynValue>> m_Clr2Script = new Dictionary<Type, Func<object, DynValue>>();

		internal CustomConvertersCollection()
		{
			for (int i = 0; i < m_Script2Clr.Length; i++)
				m_Script2Clr[i] = new Dictionary<Type, Func<DynValue, object>>();
		}


		/// <summary>
		/// Sets a custom converter from a script data type to a CLR data type. Set null to remove a previous custom converter.
		/// </summary>
		/// <param name="scriptDataType">The script data type</param>
		/// <param name="clrDataType">The CLR data type.</param>
		/// <param name="converter">The converter, or null.</param>
		public void SetScriptToClrCustomConversion(DataType scriptDataType, Type clrDataType, Func<DynValue, object> converter = null)
		{
			if ((int)scriptDataType > m_Script2Clr.Length)
				throw new ArgumentException("scriptDataType");

			Dictionary<Type, Func<DynValue, object>> map = m_Script2Clr[(int)scriptDataType];

			if (converter == null)
			{
				if (map.ContainsKey(clrDataType))
					map.Remove(clrDataType);
			}
			else
			{
				map[clrDataType] = converter;
			}
		}

		/// <summary>
		/// Gets a custom converter from a script data type to a CLR data type, or null
		/// </summary>
		/// <param name="scriptDataType">The script data type</param>
		/// <param name="clrDataType">The CLR data type.</param>
		/// <returns>The converter function, or null if not found</returns>
		public Func<DynValue, object> GetScriptToClrCustomConversion(DataType scriptDataType, Type clrDataType)
		{
			if ((int)scriptDataType > m_Script2Clr.Length)
				return null;

			Dictionary<Type, Func<DynValue, object>> map = m_Script2Clr[(int)scriptDataType];
			return map.GetOrDefault(clrDataType);
		}

		/// <summary>
		/// Sets a custom converter from a CLR data type. Set null to remove a previous custom converter.
		/// </summary>
		/// <param name="clrDataType">The CLR data type.</param>
		/// <param name="converter">The converter, or null.</param>
		public void SetClrToScriptCustomConversion(Type clrDataType, Func<object, DynValue> converter = null)
		{
			if (converter == null)
			{
				if (m_Clr2Script.ContainsKey(clrDataType))
					m_Clr2Script.Remove(clrDataType);
			}
			else
			{
				m_Clr2Script[clrDataType] = converter;
			}
		}

		/// <summary>
		/// Sets a custom converter from a CLR data type. Set null to remove a previous custom converter.
		/// </summary>
		/// <typeparam name="T">The CLR data type.</typeparam>
		/// <param name="converter">The converter, or null.</param>
		public void SetClrToScriptCustomConversion<T>(Func<T, DynValue> converter = null)
		{
			SetClrToScriptCustomConversion(typeof(T), o => converter((T)o));
		}


		/// <summary>
		/// Gets a custom converter from a CLR data type, or null
		/// </summary>
		/// <param name="clrDataType">Type of the color data.</param>
		/// <returns>The converter function, or null if not found</returns>
		public Func<object, DynValue> GetClrToScriptCustomConversion(Type clrDataType)
		{
			return m_Clr2Script.GetOrDefault(clrDataType);
		}


		/// <summary>
		/// Removes all converters.
		/// </summary>
		public void Clear()
		{
			m_Clr2Script.Clear();

			for (int i = 0; i < m_Script2Clr.Length; i++)
				m_Script2Clr[i].Clear();
		}

	}
}
