using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.DataStructs
{
	public class Slice<T> : IEnumerable<T>
	{
		IList<T> m_SourceList;
		int m_From, m_Length;
		bool m_Reversed;

		public Slice(IList<T> list, int from, int length, bool reversed)
		{
			m_SourceList = list;
			m_From = from;
			m_Length = length;
			m_Reversed = reversed;
		}

		public T this[int index]
		{
			get 
			{
				return m_SourceList[CalcRealIndex(index)];
			}
			set
			{
				m_SourceList[CalcRealIndex(index)] = value;
			}
		}

		public int From
		{
			get { return m_From; }
		}

		public int Count
		{
			get { return m_Length; }
		}

		public bool Reversed
		{
			get { return m_Reversed; }
		}

		private int CalcRealIndex(int index)
		{
			if (index < 0 || index >= m_Length)
				throw new ArgumentOutOfRangeException("index");

			if (m_Reversed)
			{
				return m_From + m_Length - index - 1;
			}
			else
			{
				return m_From + index;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < m_Length; i++)
				yield return m_SourceList[CalcRealIndex(i)];
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			for (int i = 0; i < m_Length; i++)
				yield return m_SourceList[CalcRealIndex(i)];
		}

		public T[] ToArray()
		{
			T[] array = new T[m_Length];

			for (int i = 0; i < m_Length; i++)
				array[i] = m_SourceList[CalcRealIndex(i)];

			return array;
		}

		public List<T> ToList()
		{
			List<T> list = new List<T>(m_Length);

			for (int i = 0; i < m_Length; i++)
				list.Add(m_SourceList[CalcRealIndex(i)]);

			return list;
		}


	}
}
