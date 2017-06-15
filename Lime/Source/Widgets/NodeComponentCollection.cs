using System;

namespace Lime
{
	public class NodeComponent : Component
	{
		public Node Owner { get; set; }

		public virtual void Awake() { }
		public virtual NodeComponent Clone() { return (NodeComponent)MemberwiseClone(); }
	}

	public class NodeComponentCollection : ComponentCollection<NodeComponent>
	{
		private Node owner;

		public NodeComponentCollection(Node owner)
		{
			this.owner = owner;
		}

		public NodeComponentCollection Clone(Node newOwner)
		{
			// Hack. Remove this when non-generic Add() is designed
			var result = new NodeComponentCollection(newOwner) {
				buckets = new Bucket[buckets.Length]
			};
			for (var i = 0; i < buckets.Length; i++) {
				var bucket = buckets[i];
				var clone = new Bucket() {
					Key = bucket.Key,
					Component = bucket.Component.Clone(),
				};
				clone.Component.Owner = newOwner;
				result.buckets[i] = clone;
			}
			return result;
		}

		public override void Add(NodeComponent component)
		{
			if (component.Owner != null) {
				throw new InvalidOperationException("The component is already in a collection.");
			}
			base.Add(component);
			component.Owner = owner;
			owner.PropagateDirtyFlags(Node.DirtyFlags.Components);
		}

		public override bool Remove<T>()
		{
			var c = Get<T>();
			if (c != null) {
				base.Remove<T>();
				owner.PropagateDirtyFlags(Node.DirtyFlags.Components);
				c.Owner = null;
				return true;
			}
			return false;
		}
	}
}
