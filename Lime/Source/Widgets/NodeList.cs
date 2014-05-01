using System;
using System.Collections;
using System.Collections.Generic;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public class NodeList : IList<Node>
	{
		struct EmptyEnumerator : IEnumerator<Node>
		{
			object IEnumerator.Current { get { return null; } }

			public bool MoveNext() { return false; }

			public void Reset() { }

			public Node Current { get { return null; } }

			public void Dispose() { }
		}

		struct Enumerator : IEnumerator<Node>
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

		private readonly Node owner;
		private List<Node> list;

		public NodeList(Node owner)
		{
			this.list = null;
			this.owner = owner;
		}

		internal NodeList DeepCloneFast(Node clone)
		{
			var result = new NodeList(clone);
			if (Count > 0) {
				result.list = new List<Node>(Count);
				foreach (var node in this) {
					result.Add(node.DeepCloneFast());
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

		public int Count { get { return list != null ? list.Count : 0; } }

		public IEnumerator<Node> GetEnumerator()
		{
			if (Count == 0) {
				return new EmptyEnumerator();
			} else {
				return new Enumerator(list[0]);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			if (Count == 0) {
				return new EmptyEnumerator();
			} else {
				return new Enumerator(list[0]);
			}
		}

		bool ICollection<Node>.IsReadOnly {
			get { return false; }
		}

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
		}

		public bool Contains(Node node)
		{
			return IndexOf(node) >= 0;
		}

		public void Push(Node node)
		{
			Insert(0, node);
		}

		public void Add(Node node)
		{
			RuntimeChecksBeforeInsertion(node); 
			CreateListIfNeeded();
			node.Parent = owner;
			if (Count > 0) {
				list[Count - 1].NextSibling = node;
			}
			list.Add(node);
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

		public Node FirstOrNull()
		{
			return list == null || list.Count == 0 ? null : list[0];
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
		}

		private void RuntimeChecksBeforeInsertion(Node node)
		{
			if (node.Parent != null) {
				throw new Lime.Exception("Can't adopt a node twice. Call node.Unlink() first");
			}
			if (node.AsWidget != null && owner.AsWidget == null) {
				throw new Lime.Exception("A widget can be adopted only by other widget");
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

		public void Clear()
		{
			if (list == null) {
				return;
			}
			foreach (var node in list) {
				node.Parent = null;
				node.NextSibling = null;
			}
			list.Clear();
		}

		public Node TryFind(string id)
		{
			foreach (var node in this) {
				if (node.Id == id) {
					return node;
				}
			}
			return null;
		}

		public void RemoveAt(int index)
		{
			if (list == null) {
				throw new IndexOutOfRangeException();
			}
			list[index].Parent = null;
			list[index].NextSibling = null;
			list.RemoveAt(index);
			if (index > 0) {
				list[index - 1].NextSibling = index < Count ? list[index] : null;
			} 
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
				list[index].Parent = null;
				list[index].NextSibling = null;
				list[index] = value;
				if (index > 0) {
					list[index - 1].NextSibling = value;
				}
				if (index + 1 < Count) {
					value.NextSibling = list[index + 1];
				}
			}
		}
	}
}
