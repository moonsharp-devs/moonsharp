using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MoonSharp.Interpreter.DataStructs
{
	public class MultiDictionary<K, V>
	{
		Dictionary<K, List<V>> m_Map = new Dictionary<K, List<V>>();

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
				return new V[0];
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
