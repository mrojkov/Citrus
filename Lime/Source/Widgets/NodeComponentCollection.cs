using System;

namespace Lime
{
	public class NodeComponent : Component
	{
		public Node Owner { get; set; }

		public virtual void Awake() { }
		public virtual NodeComponent Clone()
		{
			var clone = (NodeComponent)MemberwiseClone();
			clone.Owner = null;
			return clone;
		}
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
			var result = new NodeComponentCollection(newOwner);
			foreach (var c in this) {
				result.Add(c.Clone());
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
		}

		public override bool Remove(NodeComponent component)
		{
			if (base.Remove(component)) {
				component.Owner = null;
				return true;
			}
			return false;
		}

		public override void Clear()
		{
			for (int i = 0; i < buckets.Length; i++) {
				if (buckets[i].Key > 0) {
					buckets[i].Component.Owner = null;
				}
			}
			base.Clear();
		}
	}
}
