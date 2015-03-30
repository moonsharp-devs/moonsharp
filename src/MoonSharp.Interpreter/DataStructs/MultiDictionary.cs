using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.DataStructs
{
	/// <summary>
	/// A Dictionary where multiple values can be associated to the same key
	/// </summary>
	/// <typeparam name="K"></typeparam>
	/// <typeparam name="V"></typeparam>
	internal class MultiDictionary<K, V>
	{
		Dictionary<K, List<V>> m_Map = new Dictionary<K, List<V>>();
		V[] m_DefaultRet = new V[0];

		public void Add(K key, V value)
		{
			List<V> list;
			if (m_Map.TryGetValue(key, out list))
			{
				list.Add(value);
			}
			else
			{
				list = new List<V>();
				list.Add(value);
				m_Map.Add(key, list);
			}
		}

		public IEnumerable<V> Find(K key)
		{
			List<V> list;
			if (m_Map.TryGetValue(key, out list))
				return list;
			else
				return m_DefaultRet;
		}

		public bool ContainsKey(K key)
		{
			return m_Map.ContainsKey(key);
		}

		public IEnumerable<K> Keys
		{
			get { return m_Map.Keys; }
		}

		public void Clear()
		{
			m_Map.Clear();
		}

		public void Remove(K key)
		{
			m_Map.Remove(key);
		}

	}
}
