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
		LinkedListIndex<DynValue, TablePair> m_ValueMap;
		LinkedListIndex<string, TablePair> m_StringMap;
		LinkedListIndex<int, TablePair> m_ArrayMap;

		int m_CachedLength = -1;

		public Table()
		{
			m_Values = new LinkedList<TablePair>();
			m_StringMap = new LinkedListIndex<string, TablePair>(m_Values);
			m_ArrayMap = new LinkedListIndex<int, TablePair>(m_Values);
			m_ValueMap = new LinkedListIndex<DynValue, TablePair>(m_Values);
		}

		private int GetIntegralKey(double d)
		{
			int v = ((int)d);

			if (d >= 1.0 && d == v)
				return v;

			return -1;
		}

		public DynValue this[DynValue key]
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
				if (key.IsNilOrNan())
				{
					if (key.IsNil())
						throw new ScriptRuntimeException(null, "table index is nil");
					else
						throw new ScriptRuntimeException(null, "table index is NaN");
				}

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
				
				if (m_ValueMap.Set(key, new TablePair(key, value)))
					CollectDeadKeys();
			}
		}

		private DynValue GetValueOrNil(LinkedListNode<TablePair> linkedListNode)
		{
			if (linkedListNode != null)
				return linkedListNode.Value.Value;

			return DynValue.Nil;
		}

		public DynValue this[string key]
		{
			get
			{
				return GetValueOrNil(m_StringMap.Find(key));
			}
			set
			{
				if (m_StringMap.Set(key, new TablePair(DynValue.NewString(key), value)))
					CollectDeadKeys();
			}
		}


		public DynValue RawGet(string key)
		{
			var linkedListNode = m_StringMap.Find(key);

			if (linkedListNode != null)
				return linkedListNode.Value.Value;

			return null;
		}

		public DynValue this[int key]
		{
			get
			{
				return GetValueOrNil(m_ArrayMap.Find(key));
			}
			set
			{
				if (m_ArrayMap.Set(key, new TablePair(DynValue.NewNumber(key), value)))
				{
					CollectDeadKeys();
					m_CachedLength = -1;
				}
			}
		}

		private void CollectDeadKeys()
		{
			for (LinkedListNode<TablePair> node = m_Values.First; node != null; node = node.Next)
			{
				if (node.Value.Value.Type == DataType.Nil)
				{
					if (node.Value.Key.Type == DataType.Number)
					{
						int idx = GetIntegralKey(node.Value.Key.Number);
						if (idx > 0)
						{
							m_ArrayMap.Remove(idx);
							continue;
						}
					}

					if (node.Value.Key.Type == DataType.String)
					{
						m_StringMap.Remove(node.Value.Key.String);
					}
					else
					{
						m_ValueMap.Remove(node.Value.Key);
					}
				}
			}
		}

		public IEnumerable<TablePair> DebugPairs()
		{
			return m_Values;
		}

		public TablePair NextKey(DynValue v)
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
			if (linkedListNode == null || linkedListNode.Next == null)
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
