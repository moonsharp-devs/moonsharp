#if USE_DYNAMIC_STACKS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.DataStructs
{
	public class FastStack<T> : List<T>
	{
		public FastStack(int maxCapacity)
			: base(maxCapacity)
		{
		}


		public T Push(T item)
		{
			this.Add(item);
			return item;
		}

		public void Expand(int size)
		{
			for(int i = 0; i < size; i++)
				this.Add(default(T));
		}

		public void Zero(int index)
		{
			this[index] = default(T);
		}

		public T Peek(int idxofs = 0)
		{
			T item = this[this.Count - 1 - idxofs];
			return item;
		}
		public void CropAtCount(int p)
		{
			RemoveLast(Count - p);
		}

		public void RemoveLast( int cnt = 1)
		{
			if (cnt == 1)
			{
				this.RemoveAt(this.Count - 1);
			}
			else
			{
				this.RemoveRange(this.Count - cnt, cnt);
			}
		}

		public T Pop()
		{
			T retval = this[this.Count - 1];
			this.RemoveAt(this.Count - 1);
			return retval;
		}
	}
}


#endif