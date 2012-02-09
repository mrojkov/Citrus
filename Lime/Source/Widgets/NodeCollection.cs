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
		static List<Node> emptyList = new List<Node>();
		static Node[] emptyArray = new Node[0];

		List<Node> nodesList = emptyList;
		Node[] nodesArray;
		internal Node Owner;

		public Node[] AsArray
		{
			get
			{
				if (nodesArray == null) {
					if (nodesList.Count > 0) {
						nodesArray = new Node[nodesList.Count];
						nodesList.CopyTo(nodesArray);
					} else {
						nodesArray = emptyArray;
					}
				}
				return nodesArray;
			}
		}

		public int IndexOf(Node node)
		{
			int count = Count;
			for (int i = 0; i < count; i++)
				if (nodesList[i] == node)
					return i;
			return -1;
		}
		
		public Node this[int index] {
			get { return nodesList[index]; }
		}

		void ICollection<Node>.CopyTo(Node[] n, int index)
		{
			nodesList.CopyTo(n, index);
		}

		public int Count { get { return nodesList.Count; } }

		public IEnumerator<Node> GetEnumerator()
		{
			return nodesList.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return nodesList.GetEnumerator();
		}

		bool ICollection<Node>.IsReadOnly {
			get { return false; }
		}

		public void Sort(Comparison<Node> comparison)
		{
			nodesList.Sort(comparison);
			if (nodesArray != null) {
				nodesList.CopyTo(nodesArray);
			}
		}

		public bool Contains(Node node)
		{
			return nodesList.Contains(node);
		}

		public void Add(Node node)
		{
			nodesArray = null;
			if (nodesList == emptyList) {
				nodesList = new List<Node>();
			}
			node.Parent = Owner;
			nodesList.Add(node);
		}

		public void Insert(int index, Node node)
		{
			nodesArray = null;
			if (nodesList == emptyList) {
				nodesList = new List<Node>();
			}
			node.Parent = Owner;
			nodesList.Insert(index, node);
		}

		public bool Remove(Node node)
		{
			bool result = false;
			if (nodesList.Remove(node)) {
				node.Parent = null;
				result = true;
				nodesArray = null;
			}
			if (nodesList.Count == 0) {
				nodesList = emptyList;
			}
			return result;
		}

		public void Clear()
		{
			nodesArray = null;
			nodesList = emptyList;
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
