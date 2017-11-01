using System;
using System.Collections;
using System.Collections.Generic;
using Lime;

namespace Lime
{
	public class NodeList : IList<Node>
	{
		private Node First { get; set; }

		public Node FirstOrNull() => First;

		private readonly Node owner;
		private List<Node> list;

		public int Count => list != null ? list.Count : 0;

		public NodeList(Node owner)
		{
			this.list = null;
			this.owner = owner;
		}

		/// <summary>
		/// Creates a copy of this list with new owner.
		/// Containing nodes will alse have their Parent set as new owner.
		/// </summary>
		internal NodeList Clone(Node newOwner)
		{
			var result = new NodeList(newOwner);
			if (Count > 0) {
				result.list = new List<Node>(Count);
				foreach (var node in this) {
					result.Add(node.Clone());
				}
			}
			return result;
		}

		public int IndexOf(Node node)
		{
			if (list == null) {
				return -1;
			}
			return list.IndexOf(node);
		}

		public void CopyTo(Node[] array, int index)
		{
			if (list == null) {
				return;
			}
			list.CopyTo(array, index);
		}

		/// <summary>
		/// Returns Enumerator for this list. This method is preferrable over IEnumerable.GetEnumerator()
		/// because it doesn't allocate new memory via boxing.
		/// </summary>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(First);
		}

		IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
		{
			return new Enumerator(First);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(First);
		}

		public bool IsReadOnly => false;

		public void Sort(Comparison<Node> comparison)
		{
			if (list == null) {
				return;
			}
			list.Sort(comparison);
			for (int i = 1; i < Count; i++) {
				list[i - 1].NextSibling = list[i];
			}
			if (Count > 0) {
				list[Count - 1].NextSibling = null;
			}
			RecacheFirst();
		}

		public bool Contains(Node node)
		{
			return IndexOf(node) >= 0;
		}

		/// <summary>
		/// Adds a Node to the start of this list.
		/// </summary>
		public void Push(Node node)
		{
			Insert(0, node);
		}

		/// <summary>
		/// Adds a Node to the end of this list.
		/// </summary>
		public void Add(Node node)
		{
			RuntimeChecksBeforeInsertion(node);
			CreateListIfNeeded();
			node.Parent = owner;
			if (Count > 0) {
				list[Count - 1].NextSibling = node;
			}
			list.Add(node);
			RecacheFirst();
			node.PropagateDirtyFlags();
		}

		private void CreateListIfNeeded()
		{
			if (list == null) {
				list = new List<Node>();
			}
		}

		public void AddRange(IEnumerable<Node> collection)
		{
			foreach (var node in collection) {
				Add(node);
			}
		}

		public void AddRange(params Node[] collection)
		{
			foreach (var node in collection) {
				Add(node);
			}
		}

		public void Insert(int index, Node node)
		{
			RuntimeChecksBeforeInsertion(node);
			CreateListIfNeeded();
			list.Insert(index, node);
			node.Parent = owner;
			if (index > 0) {
				list[index - 1].NextSibling = node;
			}
			if (index + 1 < Count) {
				list[index].NextSibling = list[index + 1];
			}
			RecacheFirst();
			node.PropagateDirtyFlags();
			Node.InvalidateNodeReferenceCache();
		}

		private void RuntimeChecksBeforeInsertion(Node node)
		{
			if (node.Parent != null) {
				throw new Lime.Exception("Can't adopt a node twice. Call node.Unlink() first");
			}
		}

		public bool Remove(Node node)
		{
			int index = IndexOf(node);
			if (index >= 0) {
				RemoveAt(index);
				return true;
			}
			return false;
		}

		public void RemoveAll(Predicate<Node> predicate)
		{
			if (list == null) {
				return;
			}
			int index = list.Count - 1;
			while (index >= 0) {
				var node = list[index];
				if (predicate(node)) {
					RemoveAt(index);
				}
				index--;
			}
		}

		public void Clear()
		{
			if (list == null) {
				return;
			}
			Node.InvalidateNodeReferenceCache();
			foreach (var node in list) {
				node.Parent = null;
				node.NextSibling = null;
				node.PropagateDirtyFlags();
			}
			list = null;
			RecacheFirst();
		}

		/// <summary>
		/// Searchs for node with provided Id in this list.
		/// Returns null if this list doesn't contain sought-for node.
		/// </summary>
		public Node TryFind(string id)
		{
			for (var node = First; node != null; node = node.NextSibling) {
				if (node.Id == id) {
					return node;
				}
			}
			return null;
		}

		private void CleanupNode(Node node)
		{
			node.Parent = null;
			node.NextSibling = null;
			node.PropagateDirtyFlags();
		}

		public void RemoveRange(int index, int count)
		{
			if (list == null || index + count > list.Count) {
				throw new IndexOutOfRangeException();
			}
			for (int i = index; i < index + count; i++) {
				CleanupNode(list[i]);
			}
			list.RemoveRange(index, count);
			if (index > 0 && count > 0) {
				list[index - 1].NextSibling = index < Count ? list[index] : null;
			}
			RecacheFirst();
			Node.InvalidateNodeReferenceCache();
		}

		public void RemoveAt(int index)
		{
			if (list == null) {
				throw new IndexOutOfRangeException();
			}
			CleanupNode(list[index]);
			list.RemoveAt(index);
			if (index > 0) {
				list[index - 1].NextSibling = index < Count ? list[index] : null;
			}
			if (list.Count == 0) {
				list = null;
			}
			RecacheFirst();
			Node.InvalidateNodeReferenceCache();
		}

		public Node this[int index]
		{
			get
			{
				if (list == null) {
					throw new IndexOutOfRangeException();
				}
				return list[index];
			}
			set
			{
				RuntimeChecksBeforeInsertion(value);
				CreateListIfNeeded();
				value.Parent = owner;
				var oldNode = list[index];
				oldNode.Parent = null;
				oldNode.NextSibling = null;
				oldNode.PropagateDirtyFlags();
				list[index] = value;
				if (index > 0) {
					list[index - 1].NextSibling = value;
				}
				if (index + 1 < Count) {
					value.NextSibling = list[index + 1];
				}
				RecacheFirst();
				value.PropagateDirtyFlags();
				Node.InvalidateNodeReferenceCache();
			}
		}

		public void Move(int indexFrom, int indexTo)
		{
			if (list == null) {
				throw new IndexOutOfRangeException();
			}
			if (indexFrom == indexTo) {
				return;
			}
			if (indexFrom > 0) {
				list[indexFrom - 1].NextSibling = (indexFrom + 1 < Count) ? list[indexFrom + 1] : null;
			}
			var tmp = list[indexFrom];
			if (indexFrom < indexTo) {
				for (int i = indexFrom; i < indexTo; i++) {
					list[i] = list[i + 1];
				}
			} else {
				for (int i = indexFrom; i > indexTo; i--) {
					list[i] = list[i - 1];
				}
			}
			list[indexTo] = tmp;
			if (indexTo > 0) {
				list[indexTo - 1].NextSibling = tmp;
			}
			tmp.NextSibling = (indexTo + 1 < Count) ? list[indexTo + 1] : null;
			RecacheFirst();
			Node.InvalidateNodeReferenceCache();
		}

		private void RecacheFirst()
		{
			First = (list == null || list.Count == 0) ? null : list[0];
		}

		public struct Enumerator : IEnumerator<Node>
		{
			private Node first;
			private Node current;

			public Enumerator(Node first)
			{
				this.first = first;
				current = null;
			}

			object IEnumerator.Current { get { return current; } }

			public bool MoveNext()
			{
				if (current == null) {
					current = first;
				} else {
					current = current.NextSibling;
				}
				return current != null;
			}

			public void Reset()
			{
				current = null;
			}

			public Node Current { get { return current; } }

			public void Dispose() { }
		}
	}
}
