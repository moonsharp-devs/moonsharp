using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Execution.DataTypes;

namespace MoonSharp.Interpreter.Execution
{
	public class Table
	{
		LinkedList<TablePair> m_Values;
		LinkedListIndex<RValue, TablePair> m_ValueMap;
		LinkedListIndex<string, TablePair> m_StringMap;
		LinkedListIndex<int, TablePair> m_ArrayMap;

		int m_CachedLength = -1;

		public Table()
		{
			m_Values = new LinkedList<TablePair>();
			m_StringMap = new LinkedListIndex<string, TablePair>(m_Values);
			m_ArrayMap = new LinkedListIndex<int, TablePair>(m_Values);
			m_ValueMap = new LinkedListIndex<RValue, TablePair>(m_Values);
		}

		private int GetIntegralKey(double d)
		{
			int v = ((int)d);

			if (d >= 1.0 && d == v)
				return v;

			return -1;
		}

		public RValue this[RValue key]
		{
			get 
			{
				if (key.Type == DataType.Number)
				{
					int idx = GetIntegralKey(key.Number);
					if (idx > 0)
					{
						return GetValueOrNil(m_ArrayMap.Find(idx));
					}
				}
				else if (key.Type == DataType.String)
				{
					return GetValueOrNil(m_StringMap.Find(key.String));
				}

				return GetValueOrNil(m_ValueMap.Find(key));
			}
			set 
			{
				if (key.Type == DataType.String)
				{
					this[key.String] = value;
					return;
				}

				if (key.Type == DataType.Number)
				{
					int idx = GetIntegralKey(key.Number);

					if (idx > 0)
					{
						this[idx] = value;
						return;
					}
				}
				
				if (value.Type == DataType.Nil)
				{
					m_ValueMap.Remove(key);
					m_CachedLength = -1;
				}
				else
				{
					if (m_ValueMap.Set(key, new TablePair(key, value)))
						m_CachedLength = -1;
				}
			}
		}

		private RValue GetValueOrNil(LinkedListNode<TablePair> linkedListNode)
		{
			if (linkedListNode != null)
				return linkedListNode.Value.Value;

			return RValue.Nil;
		}

		public RValue this[string key]
		{
			get
			{
				return GetValueOrNil(m_StringMap.Find(key));
			}
			set
			{
				if (value.Type == DataType.Nil)
				{
					m_StringMap.Remove(key);
				}
				else
				{
					m_StringMap.Set(key, new TablePair(new RValue(key), value));
				}
			}
		}

		public RValue this[int key]
		{
			get
			{
				return GetValueOrNil(m_ArrayMap.Find(key));
			}
			set
			{
				if (value.Type == DataType.Nil)
				{
					m_ArrayMap.Remove(key);
				}
				else
				{
					m_ArrayMap.Set(key, new TablePair(new RValue(key), value));
				}
			}
		}

		public IEnumerable<TablePair> DebugPairs()
		{
			return m_Values;
		}

		public TablePair NextKey(RValue v)
		{
			if (v.Type == DataType.Nil)
			{
				LinkedListNode<TablePair> node = m_Values.First;

				if (node == null)
					return TablePair.Nil;
				else
					return node.Value;
			}

			if (v.Type == DataType.String)
			{
				return GetNextOf(m_StringMap.Find(v.String));
			}

			if (v.Type == DataType.Number)
			{
				int idx = GetIntegralKey(v.Number);

				if (idx > 0)
				{
					return GetNextOf(m_ArrayMap.Find(idx));
				}
			}

			return GetNextOf(m_ValueMap.Find(v));
		}

		private TablePair GetNextOf(LinkedListNode<TablePair> linkedListNode)
		{
			if (linkedListNode == null)
				return TablePair.Nil;

			return linkedListNode.Next.Value;
		}
		
		public bool HasStringSymbol(string symbol)
		{
			return m_StringMap.ContainsKey(symbol);
		}

		public double Length
		{
			get 
			{
				if (m_CachedLength < 0)
				{
					m_CachedLength = 0;

					for (int i = 1; m_ArrayMap.ContainsKey(i); i++)
						m_CachedLength = i;
				}

				return m_CachedLength; 
			}
		}


	}
}
