using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class Table
	{
		Dictionary<RValue, RValue> m_Symbols = new Dictionary<RValue, RValue>();
		Dictionary<string, RValue> m_StringSymbols = new Dictionary<string, RValue>();
		int m_LastConsecutiveInteger = 0;
		int m_MaxInteger = 0;


		private void RebuildArrayIndex()
		{
			var numberKeys = m_Symbols.Keys
				.Where(t => t.Type == DataType.Number)
				.Select(t => t.Number)
				.OrderBy(t => t);

			m_LastConsecutiveInteger = m_MaxInteger = 0;

			foreach (double key in numberKeys)
			{
				UpdateIntegersOnAdd(key);
			}
		}

		private void UpdateIntegersOnAdd(double key)
		{
			int ikey = (int)key;

			if (ikey != key || ikey <= 0)
				return;

			if (ikey == m_LastConsecutiveInteger + 1)
				m_LastConsecutiveInteger = ikey;

			if (m_MaxInteger > ikey)

			m_MaxInteger = Math.Max(m_MaxInteger, ikey);
		}


		public RValue this[RValue key]
		{
			get { return GetSymbol(key); }
			set 
			{
				if (value.Type == DataType.Nil)
				{
					Remove(key);
				}
				else
				{
					Set(key, value);
				}
			}
		}

		public IEnumerable<KeyValuePair<RValue, RValue>> Pairs()
		{
			return m_Symbols;
		}

		private void Remove(RValue key)
		{
			if (key.Type == DataType.String)
			{
				m_StringSymbols.Remove(key.String);
			}
			else
			{
				m_Symbols.Remove(key);

				if (key.Type == DataType.Number)
					RebuildArrayIndex(); // TODO: Optimize
			}
		}

		public void Set(RValue key, RValue value)
		{
			if (key.Type == DataType.String)
			{
				m_StringSymbols[key.String] = value.CloneAsWritable();
			}
			else
			{
				m_Symbols[key.Clone()] = value.CloneAsWritable();

				if (key.Type == DataType.Number)
					RebuildArrayIndex(); // TODO: Optimize
			}
		}

		public bool HasStringSymbol(string symbol)
		{
			return m_StringSymbols.ContainsKey(symbol);
		}


		public RValue this[string key]
		{
			get 
			{
				RValue v;

				if (m_StringSymbols.TryGetValue(key, out v))
					return v;

				return RValue.Nil;
			}
			set
			{
				if (value.Type == DataType.Nil)
				{
					m_StringSymbols.Remove(key);
				}
				else
				{
					m_StringSymbols[key] = value;
				}
			}
		}






		public RValue GetSymbol(RValue key)
		{
			if (key.Type == DataType.String)
			{
				if (m_StringSymbols.ContainsKey(key.String))
					return m_StringSymbols[key.String];
			}
			else
			{
				if (m_Symbols.ContainsKey(key))
					return m_Symbols[key];
			}

			return RValue.Nil;
		}

		public double Length
		{
			get { return m_LastConsecutiveInteger; }
		}


	}
}
