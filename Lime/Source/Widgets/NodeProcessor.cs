using System;

namespace Lime
{
	public class NodeProcessor
	{
		public NodeManager Manager { get; internal set; }

		protected internal virtual void Start() { }

		protected internal virtual void Stop() { }

		protected internal virtual void Update(float delta) { }
	}

	public abstract class NodeComponentProcessor : NodeProcessor
	{
		public Type TargetComponentType { get; }

		protected NodeComponentProcessor(Type targetComponentType)
		{
			TargetComponentType = targetComponentType;
		}

		protected internal virtual void InternalAdd(NodeComponent component) { }

		protected internal virtual void InternalRemove(NodeComponent component, Node owner) { }

		protected internal virtual void InternalOnOwnerFrozenChanged(NodeComponent component) { }
	}

	public class NodeComponentProcessor<TComponent> : NodeComponentProcessor where TComponent : NodeComponent
	{
		protected NodeComponentProcessor() : base(typeof(TComponent)) { }

		protected internal sealed override void InternalAdd(NodeComponent component)
		{
			Add((TComponent)component);
		}

		protected internal sealed override void InternalRemove(NodeComponent component, Node owner)
		{
			Remove((TComponent)component, owner);
		}

		protected internal sealed override void InternalOnOwnerFrozenChanged(NodeComponent component)
		{
			OnOwnerFrozenChanged((TComponent)component);
		}

		protected virtual void Add(TComponent component) { }

		protected virtual void Remove(TComponent component, Node owner) { }

		protected virtual void OnOwnerFrozenChanged(TComponent component) { }
	}
}
