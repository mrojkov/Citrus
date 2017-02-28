using System;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// A node reference. Used for serialization, cloning and picking a node within Tangerine.
	/// </summary>
	public struct NodeReference<T> where T: Node
	{
		private T node;

		[YuzuMember]
		public string Id;

		public NodeReference(string id)
		{
			Id = id;
			node = null;
		}

		/// <summary>
		/// Lookups the node by its id. This method should be called from Node.LookupReferences().
		/// </summary>
		public void LookupNode(Node container)
		{
			Node = Id != null ? container.TryFind<T>(Id) : null;
		}

		public T Node
		{
			get { return node; }
			set
			{
				node = value;
				Id = node?.Id;
			}
		}
	}
}
