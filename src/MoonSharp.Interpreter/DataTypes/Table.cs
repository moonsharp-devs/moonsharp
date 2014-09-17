using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.DataTypes;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter
{
	public class Table : IScriptPrivateResource
	{
		readonly LinkedList<TablePair> m_Values;
		readonly LinkedListIndex<DynValue, TablePair> m_ValueMap;
		readonly LinkedListIndex<string, TablePair> m_StringMap;
		readonly LinkedListIndex<int, TablePair> m_ArrayMap;
		readonly Script m_Owner;

		int m_InitArray = 0;
		int m_CachedLength = -1;

		public Table(Script owner)
		{
			m_Values = new LinkedList<TablePair>();
			m_StringMap = new LinkedListIndex<string, TablePair>(m_Values);
			m_ArrayMap = new LinkedListIndex<int, TablePair>(m_Values);
			m_ValueMap = new LinkedListIndex<DynValue, TablePair>(m_Values);
			m_Owner = owner;
		}

		public Script OwnerScript
		{
			get { return m_Owner; }
		}

		private int GetIntegralKey(double d)
		{
			int v = ((int)d);

			if (d >= 1.0 && d == v)
				return v;

			return -1;
		}

		public object this[object key]
		{
			get
			{
				if (key is string)
					return Get((string)key).ToObject();
				else if (key is int)
					return Get((int)key).ToObject();
				
				DynValue dynkey = DynValue.FromObject(this.OwnerScript, key);
				return Get(dynkey).ToObject();
			}

			set
			{
				DynValue dynval = DynValue.FromObject(this.OwnerScript, value);

				if (key is string)
					Set((string)key, dynval);
				else if (key is int)
					Set((int)key, dynval);
				else
					Set(DynValue.FromObject(this.OwnerScript, key), dynval);
			}
		}



		public void Set(DynValue key, DynValue value)
		{
			if (key.IsNilOrNan())
			{
				if (key.IsNil())
					throw ScriptRuntimeException.TableIndexIsNil();
				else
					throw ScriptRuntimeException.TableIndexIsNaN();
			}

			if (key.Type == DataType.String)
			{
				Set(key.String, value);
				return;
			}

			if (key.Type == DataType.Number)
			{
				int idx = GetIntegralKey(key.Number);

				if (idx > 0)
				{
					Set(idx, value);
					return;
				}
			}

			CheckValueOwner(key);
			CheckValueOwner(value);

			if (m_ValueMap.Set(key, new TablePair(key, value)))
				CollectDeadKeys();
		}

		public DynValue Get(DynValue key)
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

		private DynValue GetValueOrNil(LinkedListNode<TablePair> linkedListNode)
		{
			if (linkedListNode != null)
				return linkedListNode.Value.Value;

			return DynValue.Nil;
		}

		public void Set(string key, DynValue value)
		{
			CheckValueOwner(value);

			if (m_StringMap.Set(key, new TablePair(DynValue.NewString(key), value)))
				CollectDeadKeys();
		}

		public DynValue Get(string key)
		{
			return GetValueOrNil(m_StringMap.Find(key));
		}


		public DynValue RawGet(string key)
		{
			var linkedListNode = m_StringMap.Find(key);

			if (linkedListNode != null)
				return linkedListNode.Value.Value;

			return null;
		}

		public void Set(int key, DynValue value)
		{
			CheckValueOwner(value);

			if (m_ArrayMap.Set(key, new TablePair(DynValue.NewNumber(key), value)))
			{
				CollectDeadKeys();
				m_CachedLength = -1;
			}
			else if (value.IsNil())
				m_CachedLength = -1;
		}

		public DynValue Get(int key)
		{
			return GetValueOrNil(m_ArrayMap.Find(key));
		}

		private void CheckValueOwner(DynValue value)
		{
			// +++ value.AssertOwner(m_Owner);
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


		public int Length
		{
			get 
			{
				if (m_CachedLength < 0)
				{
					m_CachedLength = 0;

					for (int i = 1; m_ArrayMap.ContainsKey(i) && !m_ArrayMap.Find(i).Value.Value.IsNil(); i++)
						m_CachedLength = i;
				}

				return m_CachedLength; 
			}
		}

		internal void InitNextArrayKeys(DynValue val, bool lastpos)
		{
			if (val.Type == DataType.Tuple && lastpos)
			{
				foreach (DynValue v in val.Tuple)
					InitNextArrayKeys(v, true);
			}
			else
			{
				Set(++m_InitArray, val.ToScalar());
			}
		}

		/// <summary>
		/// Gets the meta-table associated with this instance.
		/// </summary>
		public Table MetaTable { get; set; }



		/// <summary>
		/// Enumerates the key value pairs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<KeyValuePair<DynValue, DynValue>> Pairs
		{
			get
			{
				return m_Values.Select(n => new KeyValuePair<DynValue, DynValue>(n.Key, n.Value));
			}
		}

		/// <summary>
		/// Enumerates the keys.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<DynValue> Keys
		{
			get
			{
				return m_Values.Select(n => n.Key);
			}
		}

		/// <summary>
		/// Enumerates the values
		/// </summary>
		/// <returns></returns>
		public IEnumerable<DynValue> Values
		{
			get
			{
				return m_Values.Select(n => n.Value);
			}
		}



	}
}
