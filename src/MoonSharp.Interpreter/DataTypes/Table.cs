using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter.DataStructs;
using MoonSharp.Interpreter.Execution;

namespace MoonSharp.Interpreter
{
	/// <summary>
	/// A class representing a Lua table.
	/// </summary>
	public class Table : RefIdObject, IScriptPrivateResource
	{
		readonly LinkedList<TablePair> m_Values;
		readonly LinkedListIndex<DynValue, TablePair> m_ValueMap;
		readonly LinkedListIndex<string, TablePair> m_StringMap;
		readonly LinkedListIndex<int, TablePair> m_ArrayMap;
		readonly Script m_Owner;

		int m_InitArray = 0;
		int m_CachedLength = -1;

		/// <summary>
		/// Initializes a new instance of the <see cref="Table"/> class.
		/// </summary>
		/// <param name="owner">The owner script.</param>
		public Table(Script owner)
		{
			m_Values = new LinkedList<TablePair>();
			m_StringMap = new LinkedListIndex<string, TablePair>(m_Values);
			m_ArrayMap = new LinkedListIndex<int, TablePair>(m_Values);
			m_ValueMap = new LinkedListIndex<DynValue, TablePair>(m_Values);
			m_Owner = owner;
		}


		/// <summary>
		/// Gets the script owning this resource.
		/// </summary>
		public Script OwnerScript
		{
			get { return m_Owner; }
		}

		/// <summary>
		/// Removes all items from the Table.
		/// </summary>
		public void Clear()
		{
			m_Values.Clear();
			m_StringMap.Clear();
			m_ArrayMap.Clear();
			m_ValueMap.Clear();
		}

		/// <summary>
		/// Gets the integral key from a double.
		/// </summary>
		private int GetIntegralKey(double d)
		{
			int v = ((int)d);

			if (d >= 1.0 && d == v)
				return v;

			return -1;
		}

		/// <summary>
		/// Gets or sets the 
		/// <see cref="System.Object" /> with the specified key(s).
		/// This will marshall CLR and MoonSharp objects in the best possible way.
		/// Multiple keys can be used to access subtables.
		/// </summary>
		/// <value>
		/// The <see cref="System.Object" />.
		/// </value>
		/// <param name="key">The key.</param>
		/// <param name="subkeys">Optional subkeys to access subtables</param>
		/// <returns></returns>
		public object this[object key, params object[] subkeys]
		{
			get
			{
				Table t = ResolveMultipleKeys(ref key, subkeys);
				return t.GetAsObject(key);
			}

			set
			{ 
				Table t = ResolveMultipleKeys(ref key, subkeys);
				t.SetAsObject(key, value);
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="System.Object"/> with the specified key(s).
		/// This will marshall CLR and MoonSharp objects in the best possible way.
		/// </summary>
		/// <value>
		/// The <see cref="System.Object"/>.
		/// </value>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public object this[object key]
		{
			get
			{
				return this.GetAsObject(key);
			}

			set
			{
				this.SetAsObject(key, value);
			}
		}

		private Table ResolveMultipleKeys(ref object key, object[] subkeys)
		{
			if (subkeys.Length == 0)
				return this;

			Table t = this;
			int i = -1;

			do
			{
				DynValue vt = t.GetWithObjectKey(key);

				if (vt.Type != DataType.Table)
					throw new ScriptRuntimeException("Key '{0}' did not point to a table");

				t = vt.Table;
				key = subkeys[++i];
			}
			while (i < subkeys.Length - 1);

			return t;
		}

		/// <summary>
		/// Gets the dynvalue associated with the specified key (expressed as a System.Object)
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public DynValue GetWithObjectKey(object key)
		{
			if (key is string)
				return Get((string)key);
			else if (key is int)
				return Get((int)key);

			DynValue dynkey = DynValue.FromObject(this.OwnerScript, key);
			return Get(dynkey);
		}


		/// <summary>
		/// Gets the dynvalue associated with the specified key (expressed as a System.Object) as a System.Object.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public object GetAsObject(object key)
		{
			if (key is string)
				return Get((string)key).ToObject();
			else if (key is int)
				return Get((int)key).ToObject();

			DynValue dynkey = DynValue.FromObject(this.OwnerScript, key);
			return Get(dynkey).ToObject();
		}

		/// <summary>
		/// Sets the dynvalue associated with the specified key. Both expressed as System.Object.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void SetAsObject(object key, object value)
		{
			DynValue dynval = DynValue.FromObject(this.OwnerScript, value);

			if (key is string)
				Set((string)key, dynval);
			else if (key is int)
				Set((int)key, dynval);
			else
				Set(DynValue.FromObject(this.OwnerScript, key), dynval);
		}


		/// <summary>
		/// Sets the value associated to the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
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

			this.CheckScriptOwnership(key);
			this.CheckScriptOwnership(value);

			PerformTableSet(m_ValueMap, key, key, value, false);
		}

		private void PerformTableSet<T>(LinkedListIndex<T, TablePair> listIndex, T key, DynValue keyDynValue, DynValue value, bool isNumber)
		{
			TablePair prev = listIndex.Set(key, new TablePair(keyDynValue, value));

			if (prev.Value == null || prev.Value.IsNil())
			{
				CollectDeadKeys();

				if (isNumber)
					m_CachedLength = -1;
			}

			if (isNumber && value.IsNil())
				m_CachedLength = -1;
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
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

		/// <summary>
		///  Sets the value associated to the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void Set(string key, DynValue value)
		{
			this.CheckScriptOwnership(value);
			PerformTableSet(m_StringMap, key, DynValue.NewString(key), value, false);
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public DynValue Get(string key)
		{
			return GetValueOrNil(m_StringMap.Find(key));
		}


		/// <summary>
		/// Gets the value associated with the specified key, without bringing to Nil the non-existant values.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public DynValue RawGet(string key)
		{
			var linkedListNode = m_StringMap.Find(key);

			if (linkedListNode != null)
				return linkedListNode.Value.Value;

			return null;
		}

		/// <summary>
		/// Sets the value associated to the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void Set(int key, DynValue value)
		{
			this.CheckScriptOwnership(value);
			PerformTableSet(m_ArrayMap, key, DynValue.NewNumber(key), value, true);
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public DynValue Get(int key)
		{
			return GetValueOrNil(m_ArrayMap.Find(key));
		}

		/// <summary>
		/// Collects the dead keys. This frees up memory but invalidates pending iterators.
		/// It's called automatically internally when the semantics of Lua tables allow, but can be forced
		/// externally if it's known that no iterators are pending.
		/// </summary>
		public void CollectDeadKeys()
		{
			for (LinkedListNode<TablePair> node = m_Values.First; node != null; node = node.Next)
			{
				if (node.Value.Value.IsNil())
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


		/// <summary>
		/// Returns the next pair from a value
		/// </summary>
		public TablePair? NextKey(DynValue v)
		{
			if (v.IsNil())
			{
				LinkedListNode<TablePair> node = m_Values.First;

				if (node == null)
					return TablePair.Nil;
				else
				{
					if (node.Value.Value.IsNil())
						return NextKey(node.Value.Key);
					else
						return node.Value;
				}
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

		private TablePair? GetNextOf(LinkedListNode<TablePair> linkedListNode)
		{
			while (true)
			{
				if (linkedListNode == null)
					return null;

				if (linkedListNode.Next == null)
					return TablePair.Nil;

				linkedListNode = linkedListNode.Next;

				if (!linkedListNode.Value.Value.IsNil())
					return linkedListNode.Value;
			}
		}


		/// <summary>
		/// Gets the length of the "array part".
		/// </summary>
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
		public Table MetaTable 
		{ 
			get { return m_MetaTable; }
			set { this.CheckScriptOwnership(m_MetaTable); m_MetaTable = value; } 
		}
		private Table m_MetaTable;



		/// <summary>
		/// Enumerates the key/value pairs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<TablePair> Pairs
		{
			get
			{
				return m_Values.Select(n => new TablePair(n.Key, n.Value));
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
