using System;
using System.Collections;
using System.Collections.Generic;

namespace Lime
{
	public class NodeManagerProcessorCollection : IList<NodeProcessor>
	{
		private NodeManager owner;
		private List<NodeProcessor> items = new List<NodeProcessor>();

		public int Count => items.Count;

		public NodeProcessor this[int index] => items[index];

		NodeProcessor IList<NodeProcessor>.this[int index]
		{
			get => this[index];
			set => throw new NotSupportedException();
		}

		bool ICollection<NodeProcessor>.IsReadOnly => false;

		internal NodeManagerProcessorCollection(NodeManager owner)
		{
			this.owner = owner;
		}

		public void Add(NodeProcessor item)
		{
			Insert(Count, item);
		}

		public void Insert(int index, NodeProcessor item)
		{
			if (item == null) {
				throw new ArgumentNullException(nameof(item));
			}
			if (item.Manager != null) {
				throw new ArgumentException(nameof(item));
			}
			items.Insert(index, item);
			owner.RegisterNodeProcessor(item);
		}

		public void Clear()
		{
			while (Count > 0) {
				RemoveAt(Count - 1);
			}
		}

		public bool Remove(NodeProcessor item)
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
			owner.UnregisterNodeProcessor(item);
		}

		public int IndexOf(NodeProcessor item)
		{
			return Contains(item) ? items.IndexOf(item) : -1;
		}

		public bool Contains(NodeProcessor item)
		{
			return item != null && item.Manager == owner;
		}

		public void CopyTo(NodeProcessor[] array)
		{
			CopyTo(array, 0);
		}

		public void CopyTo(NodeProcessor[] array, int arrayIndex)
		{
			CopyTo(0, array, arrayIndex, Count);
		}

		public void CopyTo(int index, NodeProcessor[] array, int arrayIndex, int count)
		{
			items.CopyTo(index, array, arrayIndex, count);
		}

		public Enumerator GetEnumerator() => new Enumerator(items);

		IEnumerator<NodeProcessor> IEnumerable<NodeProcessor>.GetEnumerator() => GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public struct Enumerator : IEnumerator<NodeProcessor>
		{
			private List<NodeProcessor>.Enumerator sourceEnumerator;

			public NodeProcessor Current => sourceEnumerator.Current;

			object IEnumerator.Current => Current;

			internal Enumerator(List<NodeProcessor> items)
			{
				sourceEnumerator = items.GetEnumerator();
			}

			public bool MoveNext() => sourceEnumerator.MoveNext();

			public void Dispose() => sourceEnumerator.Dispose();

			void IEnumerator.Reset() => Toolbox.ResetEnumerator(ref sourceEnumerator);
		}
	}
}
