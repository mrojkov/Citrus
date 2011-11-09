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
		static List<Node> emptyList = new List<Node> ();
		List<Node> nodes = emptyList;
		internal Node Owner;

		public int IndexOf (Node node)
		{
			int count = Count;
			for (int i = 0; i < count; i++)
				if (nodes [i] == node)
					return i;
			return -1;
		}

		public Node this [int index] {
			get { return nodes [index]; }
		}
		
		void ICollection<Node>.CopyTo (Node[] n, int index)
		{
			nodes.CopyTo (n, index);
		}

		public int Count { get { return nodes.Count; } }
	
		public IEnumerator<Node> GetEnumerator ()
		{
			return nodes.GetEnumerator ();
		}
	
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return nodes.GetEnumerator ();
		}
		
		bool ICollection<Node>.IsReadOnly {
			get { return false; }
		}
		
		public bool Contains (Node node)
		{
			return nodes.Contains (node);
		}
	
		public void Add (Node node)
		{
			if (nodes == emptyList) {
				nodes = new List<Node> ();
			}
			node.Parent = Owner;
			nodes.Add (node);
		}

		public void Insert (int index, Node node)
		{
			if (nodes == emptyList) {
				nodes = new List<Node> ();
			}
			node.Parent = Owner;
			nodes.Insert (index, node);
		}
		
		public bool Remove (Node node)
		{
			bool result = false;
			if (nodes.Remove (node)) {
				node.Parent = null;
				result = true;
			}
			if (nodes.Count == 0) {
				nodes = emptyList;
			}
			return result;
		}

		public void Clear ()
		{
			nodes = emptyList;
		}

		public Node Get (string id)
		{
			foreach (Node child in this) {
				if (child.Id == id)
					return child;
			}
			return null;
		}

		public T Get<T> (string id) where T : Node
		{
			return Get (id) as T;
		}
	}
}
