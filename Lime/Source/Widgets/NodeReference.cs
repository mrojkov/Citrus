using System;
using Yuzu;

namespace Lime
{
	/// <summary>
	/// A node reference is used for referencing to a node within a serialized scene by the node Id.
	/// </summary>
	public struct NodeReference<T> where T: Node
	{
		[YuzuMember]
		// Public field only because of Yuzu serialization.
		public string Id;

		public T Node { get; private set; }

		public NodeReference(string id)
		{
			Id = id;
			Node = null;
		}

		public NodeReference(T node)
		{
			Id = node?.Id;
			Node = node;
		}

		/// <summary>
		/// Create a copy with the resolved Node reference. This method should be called from Node.LookupReferences().
		/// </summary>
		public NodeReference<T> Resolve(Node context)
		{
			return new NodeReference<T> {
				Id = Id,
				Node = Id != null ? context.TryFind<T>(Id) : null
			};
		}
	}
}
