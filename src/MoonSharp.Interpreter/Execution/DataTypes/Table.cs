using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution
{
	public class Table
	{
		Dictionary<RValue, RValue> m_Symbols;
		int m_LastConsecutiveInteger = 0;
		int m_MaxInteger = 0;

		public Table()
		{
			m_Symbols = new Dictionary<RValue, RValue>();
		}

		public Table(Dictionary<RValue, RValue> symbols, IEnumerable<RValue> positionals)
		{
			m_Symbols = symbols;

			RValue lastPositional = null;
			double positionalIndex = 1;

			foreach (RValue v in positionals)
			{
				m_Symbols[new RValue(positionalIndex)] = v.ToSimplestValue();
				lastPositional = v;
				positionalIndex += 1;
			}

			if ((lastPositional != null) && (lastPositional.Type == DataType.Tuple))
			{
				positionalIndex -= 1;

				foreach (RValue v in lastPositional.UnpackedTuple())
				{
					m_Symbols[new RValue(positionalIndex)] = v.ToSimplestValue();
					positionalIndex += 1;
				}
			}

			RebuildArrayIndex();
		}

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
			m_Symbols.Remove(key);

			if (key.Type == DataType.Number)
				RebuildArrayIndex(); // TODO: Optimize
		}

		public double Length
		{
			get { return m_LastConsecutiveInteger; }
		}

		public void Set(RValue key, RValue value)
		{
			m_Symbols[key.Clone()] = value.CloneAsWritable();

			if (key.Type == DataType.Number)
				RebuildArrayIndex(); // TODO: Optimize
		}

		public RValue GetSymbol(RValue key)
		{
			if (m_Symbols.ContainsKey(key))
				return m_Symbols[key];

			return RValue.Nil;
		}


	}
}
