using System;
using System.Collections;
using System.Collections.Generic;
using Lime;
using ProtoBuf;

namespace Lime
{
	/// <summary>
	/// Список объектов сцены. Поведение аналогично стандартному списку List
	/// </summary>
	[ProtoContract]
	public class NodeList : IList<Node>
	{
		/// <summary>
		/// Перечислитель объектов сцены
		/// </summary>
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

		private readonly Node owner;
		private List<Node> list;

		/// <summary>
		/// Конструктор только для ProtoBuf
		/// </summary>
		public NodeList() { /* ctor for ProtoBuf only */ }

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="owner">Объект, которому будет принадлежать этот список</param>
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

		public Enumerator GetEnumerator()
		{
			return new Enumerator(FirstOrNull());
		}

		IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
		{
			return new Enumerator(FirstOrNull());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(FirstOrNull());
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

		/// <summary>
		/// Добавляет объект в начало списка
		/// </summary>
		public void Push(Node node)
		{
			Insert(0, node);
		}

		/// <summary>
		/// Добавляет объект в конец списка
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
			if (node.GlobalValuesValid) {
				node.InvalidateGlobalValues();
			}
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
			if (node.GlobalValuesValid) {
				node.InvalidateGlobalValues();
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
				node.InvalidateGlobalValues();
			}
			list.Clear();
		}

		public Node TryFind(string id)
		{
			for (var node = FirstOrNull(); node != null; node = node.NextSibling) {
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
			var node = list[index];
			node.Parent = null;
			node.NextSibling = null;
			node.InvalidateGlobalValues();
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
				var oldNode = list[index];
				oldNode.Parent = null;
				oldNode.NextSibling = null;
				oldNode.InvalidateGlobalValues();
				list[index] = value;
				if (index > 0) {
					list[index - 1].NextSibling = value;
				}
				if (index + 1 < Count) {
					value.NextSibling = list[index + 1];
				}
				if (value.GlobalValuesValid) {
					value.InvalidateGlobalValues();
				}
			}
		}
	}
}
