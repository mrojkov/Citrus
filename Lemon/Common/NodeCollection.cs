using System;
using System.Collections;
using System.Collections.Generic;
using Lime;
using ProtoBuf;
	
namespace Lemon
{
	[ProtoContract]
    public class NodeCollection : ICollection<Node>
	{
		private readonly List<Node> nodes = new List<Node> ();
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
			node.Parent = Owner;
			nodes.Add (node);
		}

		public void Insert (int index, Node node)
		{
			node.Parent = Owner;
			nodes.Insert (index, node);
		}
		
		public bool Remove (Node node)
		{
			if (nodes.Remove (node)) {
				node.Parent = null;
				return true;
			}
			return false;
		}

		public void Clear ()
		{
			nodes.Clear ();
		}
			
		public T Find<T> (string id) where T : Node
		{
			T result = Find (id) as T;
			if (result == null)
				throw new Lime.Exception (
					String.Format ("Node '{0}' (of type: {1}) not found in '{2}' (type: {3})", 
					id, typeof(T), Owner.Id, Owner.GetType ()));
			return result;
		}

		public Node Find (string id)
		{
			if (id.Contains ("/")) {
				Node child = Owner;
				string[] names = id.Split ('/');
				foreach (string name in names) {
					child = child.Nodes.Find (name);
					if (child == null)
						break;
				}
				return child;
			} else
				return FindHelper (id);
		}

		Node FindHelper (string id)
		{
			Queue<Node> queue = new Queue<Node> ();
			queue.Enqueue (Owner);
			while (queue.Count > 0) {
				Node node = queue.Dequeue ();
				foreach (Node child in node.Nodes) {
					if (child.Id == id)
						return child;
				}
				foreach (Node child in node.Nodes)
					queue.Enqueue (child);
			}
			return null;
		}
	}
}
