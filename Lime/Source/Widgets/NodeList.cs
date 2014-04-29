using System;
using System.Collections;
using System.Collections.Generic;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract(IgnoreListHandling = true)]
	public struct NodeList : IList<Node>
	{
		private readonly Node owner;

		/// <summary>
		/// This member can be used only for fast serialization and children traverse.
		/// Avoid to use it in the game code.
		/// </summary>
		[ProtoMember(1)]
		public Node[] AsArray;

		private static Node[] EmptyNodeArray = new Node[0];

		public NodeList(Node owner)
		{
			AsArray = EmptyNodeArray;
			this.owner = owner;
		}

		internal NodeList DeepCloneFast(Node clone)
		{
			var result = new NodeList(clone);
			if (Count > 0) {
				result.AsArray = new Node[Count];
				int i = 0;
				foreach (var node in AsArray) {
					var n = node.DeepCloneFast();
					n.Parent = clone;
					result.AsArray[i++] = n;
				}
			}
			return result;
		}

		[ProtoAfterDeserialization]
		public void AfterDeserialization()
		{
			foreach (var node in AsArray) {
				node.Parent = owner;
			}
		}

		public int IndexOf(Node node)
		{
			return Array.IndexOf(AsArray, node);
		}

		public void CopyTo(Node[] array, int index)
		{
			AsArray.CopyTo(array, index);
		}

		public int Count { get { return AsArray.Length; } }

		public IEnumerator<Node> GetEnumerator()
		{
			foreach (var node in AsArray) {
				yield return node;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return AsArray.GetEnumerator();
		}

		bool ICollection<Node>.IsReadOnly {
			get { return false; }
		}

		public void Sort(Comparison<Node> comparison)
		{
			Array.Sort(AsArray, comparison);
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
			node.Parent = owner;
			Array.Resize(ref AsArray, Count + 1);
			AsArray[Count - 1] = node;
		}

		public void AddRange(IEnumerable<Node> collection)
		{
			int count = 0;
			foreach (var node in collection) {
				RuntimeChecksBeforeInsertion(node);
				count++;
			}
			Array.Resize(ref AsArray, Count + count);
			int i = Count - count;
			foreach (var node in collection) {
				node.Parent = owner;
				AsArray[i++] = node;
			}
		}

		public void Insert(int index, Node node)
		{
			RuntimeChecksBeforeInsertion(node);
			Array.Resize(ref AsArray, Count + 1);
			Array.Copy(AsArray, index, AsArray, index + 1, Count - index - 1);
			AsArray[index] = node;
			node.Parent = owner;
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
			foreach (var node in AsArray) {
				node.Parent = null;
			}
			AsArray = EmptyNodeArray;
		}

		public Node TryFind(string id)
		{
			foreach (Node node in AsArray) {
				if (node.Id == id) {
					return node;
				}
			}
			return null;
		}

		public void RemoveAt(int index)
		{
			var node = AsArray[index];
			node.Parent = null;
			Node last = null;
			int count = AsArray.Length;
			if (count > 0) {
				last = AsArray[count - 1];
			}
			Array.Resize(ref AsArray, count - 1);
			count -= 1;
			if (count - index - 1 > 0) {
				Array.Copy(AsArray, index + 1, AsArray, index, count - index - 1);
			}
			if (index < count) {
				AsArray[count - 1] = last;
			}
		}

		public Node this[int index]
		{
			get { return AsArray[index]; }
			set
			{
				RuntimeChecksBeforeInsertion(value);
				value.Parent = owner;
				AsArray[index].Parent = null;
				AsArray[index] = value;
			}
		}
	}
}
