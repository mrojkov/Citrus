using System;
using Yuzu;

namespace Lime
{
	public class NodeComponent
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
			var result = new NodeComponentCollection(newOwner);
			foreach (var node in this) {
				result.Add(node.Clone());
			}
			return result;
		}

		public override void Add<T>(T component)
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
