using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.Execution.VM
{
	public class FastStack<T> 
	{
		T[] m_Storage;
		int m_HeadIdx = 0;

		public FastStack(int maxCapacity)
		{
			m_Storage = new T[maxCapacity];
		}

		public T this[int index]
		{
			get { return m_Storage[index]; }
			set { m_Storage[index] = value; }
		}

		public T Push(T item)
		{
			m_Storage[m_HeadIdx++] = item;
			return item;
		}

		public void Zero(int from, int to)
		{
			Array.Clear(m_Storage, from, to - from + 1);
		}

		public void Zero(int index)
		{
			m_Storage[index] = default(T);
		}

		public T Peek(int idxofs = 0)
		{
			T item = m_Storage[m_HeadIdx - 1 - idxofs];
			return item;
		}

		public void RemoveLast( int cnt = 1)
		{
			if (cnt == 1)
			{
				--m_HeadIdx;
				m_Storage[m_HeadIdx] = default(T);
			}
			else
			{
				int oldhead = m_HeadIdx;
				m_HeadIdx -= cnt;
				Zero(m_HeadIdx + 1, oldhead);
			}
		}

		public T Pop()
		{
			--m_HeadIdx;
			T retval = m_Storage[m_HeadIdx];
			m_Storage[m_HeadIdx] = default(T);
			return retval;
		}

		public void Clear()
		{
			Array.Clear(m_Storage, 0, m_Storage.Length);
			m_HeadIdx = 0;
		}

		public int Count
		{
			get { return m_HeadIdx; }
		}


	}
}
