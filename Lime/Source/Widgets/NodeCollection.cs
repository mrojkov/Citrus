using System;
using System.Collections;
using System.Collections.Generic;
using Lime;
using ProtoBuf;

namespace Lime
{
	[ProtoContract]
	public sealed class NodeCollection : ICollection<Node>
	{
		static readonly List<Node> emptyList = new List<Node>();
		static readonly Node[] emptyArray = new Node[0];

		List<Node> nodeList = emptyList;
		Node[] nodeArray;
		readonly Node owner;

		public NodeCollection() { /* ctor for ProtoBuf only */ }

		public NodeCollection(Node owner)
		{
			this.owner = owner;
		}

		public NodeCollection(Node owner, int capacity)
		{
			if (capacity > 0) {
				nodeList = new List<Node>(capacity);
			}
			this.owner = owner;
		}

		internal static NodeCollection DeepCloneFast(Node owner, NodeCollection source)
		{
			var result = new NodeCollection(owner, source.Count);
			foreach (var node in source.AsArray) {
				result.Add(node.DeepCloneFast());
			}
			return result;
		}

		public Node[] AsArray
		{
			get
			{
				if (nodeArray == null) {
					nodeArray = nodeList.Count > 0 ? nodeList.ToArray() : emptyArray;
				}
				return nodeArray;
			}
		}

		public int IndexOf(Node node)
		{
			int i = 0;
			foreach (var current in AsArray) {
				if (current == node)
					return i;
				i++;
			}
			return -1;
		}

		public IEnumerable<T> All<T>() where T : Node
		{
			foreach (var node in AsArray) {
				if (node is T) {
					yield return node as T;
				}
			}
		}

		public Node this[int index] {
			get { return AsArray[index]; }
		}

		void ICollection<Node>.CopyTo(Node[] n, int index)
		{
			nodeList.CopyTo(n, index);
		}

		public bool Empty { get { return AsArray == emptyArray; } }

		public int Count { get { return AsArray.Length; } }

		public IEnumerator<Node> GetEnumerator()
		{
			return nodeList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return nodeList.GetEnumerator();
		}

		bool ICollection<Node>.IsReadOnly {
			get { return false; }
		}

		public void Sort(Comparison<Node> comparison)
		{
			nodeList.Sort(comparison);
			if (nodeArray != null) {
				nodeList.CopyTo(nodeArray);
			}
		}

		public bool Contains(Node node)
		{
			return nodeList.Contains(node);
		}

		public void Push(Node node)
		{
			Insert(0, node);
		}

		public void Add(Node node)
		{
			if (node.Parent != null) {
				throw new Lime.Exception("Can't adopt node twice. Try Unlink() first");
			}
			nodeArray = null;
			if (nodeList == emptyList) {
				nodeList = new List<Node>();
			}
			node.Parent = owner;
			nodeList.Add(node);
		}

		public void Insert(int index, Node node)
		{
			if (node.Parent != null) {
				throw new Lime.Exception("Can't adopt node twice. Try Unlink() first");
			}
			nodeArray = null;
			if (nodeList == emptyList) {
				nodeList = new List<Node>();
			}
			node.Parent = owner;
			nodeList.Insert(index, node);
		}

		public bool Remove(Node node)
		{
			bool result = false;
			if (nodeList.Remove(node)) {
				node.Parent = null;
				result = true;
				nodeArray = null;
			}
			if (nodeList.Count == 0) {
				nodeList = emptyList;
			}
			return result;
		}

		public void Clear()
		{
			foreach (var node in nodeList) {
				node.Parent = null;
			}
			nodeArray = null;
			nodeList = emptyList;
		}

		public Node Get(string id)
		{
			foreach (Node child in this) {
				if (child.Id == id)
					return child;
			}
			return null;
		}

		public T Get<T>(string id) where T : Node
		{
			return Get(id) as T;
		}
	}
}
