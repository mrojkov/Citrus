using System.Collections.Generic;

namespace Lime.Graphics.Platform
{
	internal class LruCache<TKey, TValue>
	{
		private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> lookup;
		private LinkedList<KeyValuePair<TKey, TValue>> list;
		private Stack<LinkedListNode<KeyValuePair<TKey, TValue>>> freeNodes = new Stack<LinkedListNode<KeyValuePair<TKey, TValue>>>();

		public int Count => list.Count;

		public LruCache()
			: this(EqualityComparer<TKey>.Default)
		{
		}

		public LruCache(IEqualityComparer<TKey> comparer)
		{
			lookup = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
			list = new LinkedList<KeyValuePair<TKey, TValue>>();
		}

		public bool Contains(TKey key) => lookup.ContainsKey(key);

		public bool TryGetValue(TKey key, out TValue value)
		{
			if (lookup.TryGetValue(key, out var node)) {
				list.Remove(node);
				list.AddFirst(node);
				value = node.Value.Value;
				return true;
			}
			value = default;
			return false;
		}

		public void Add(TKey key, TValue value)
		{
			var node = CreateNode(new KeyValuePair<TKey, TValue>(key, value));
			lookup.Add(key, node);
			list.AddFirst(node);
		}

		public bool Remove(TKey key)
		{
			if (lookup.TryGetValue(key, out var node)) {
				Remove(node);
				return true;
			}
			return false;
		}
		public TValue Peek() => list.Last.Value.Value;

		public TValue Evict()
		{
			var lru = list.Last;
			Remove(lru);
			return lru.Value.Value;
		}

		private void Remove(LinkedListNode<KeyValuePair<TKey, TValue>> node)
		{
			lookup.Remove(node.Value.Key);
			list.Remove(node);
			freeNodes.Push(node);
		}

		private LinkedListNode<KeyValuePair<TKey, TValue>> CreateNode(KeyValuePair<TKey, TValue> kv)
		{
			LinkedListNode<KeyValuePair<TKey, TValue>> node;
			if (freeNodes.Count > 0) {
				node = freeNodes.Pop();
				node.Value = kv;
			} else {
				node = new LinkedListNode<KeyValuePair<TKey, TValue>>(kv);
			}
			return node;
		}
	}
}
