using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class NodeManagerRootNodeCollection : IList<Node>
	{
		private NodeManager owner;
		private List<Node> items = new List<Node>();

		public int Count => items.Count;

		public Node this[int index] => items[index];

		Node IList<Node>.this[int index]
		{
			get => this[index];
			set => throw new NotSupportedException();
		}

		bool ICollection<Node>.IsReadOnly => false;

		internal NodeManagerRootNodeCollection(NodeManager owner)
		{
			this.owner = owner;
		}

		public void Add(Node item)
		{
			Insert(Count, item);
		}

		public void Insert(int index, Node item)
		{
			if (item == null) {
				throw new ArgumentNullException(nameof(item));
			}
			if (item.Manager != null) {
				throw new ArgumentException(nameof(item));
			}
			if (item.Parent != null) {
				throw new ArgumentException(nameof(item));
			}
			items.Insert(index, item);
			owner.RegisterNode(item);
		}

		public void Clear()
		{
			while (Count > 0) {
				RemoveAt(Count - 1);
			}
		}

		public bool Remove(Node item)
		{
			var index = IndexOf(item);
			if (index >= 0) {
				RemoveAt(index);
				return true;
			}
			return false;
		}

		public void RemoveAt(int index)
		{
			var item = items[index];
			items.RemoveAt(index);
			owner.UnregisterNode(item);
		}

		public int IndexOf(Node item)
		{
			return Contains(item) ? items.IndexOf(item) : -1;
		}

		public bool Contains(Node item)
		{
			return item != null && item.Manager == owner && item.Parent == null;
		}

		public void CopyTo(Node[] array)
		{
			CopyTo(array, 0);
		}

		public void CopyTo(Node[] array, int arrayIndex)
		{
			CopyTo(0, array, arrayIndex, Count);
		}

		public void CopyTo(int index, Node[] array, int arrayIndex, int count)
		{
			items.CopyTo(index, array, arrayIndex, count);
		}

		public Enumerator GetEnumerator() => new Enumerator(items);

		IEnumerator<Node> IEnumerable<Node>.GetEnumerator() => GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public struct Enumerator : IEnumerator<Node>
		{
			private List<Node>.Enumerator sourceEnumerator;

			public Node Current => sourceEnumerator.Current;

			object IEnumerator.Current => Current;

			internal Enumerator(List<Node> items)
			{
				sourceEnumerator = items.GetEnumerator();
			}

			public bool MoveNext() => sourceEnumerator.MoveNext();

			public void Dispose() => sourceEnumerator.Dispose();

			void IEnumerator.Reset() => Toolbox.ResetEnumerator(ref sourceEnumerator);
		}
	}
}
