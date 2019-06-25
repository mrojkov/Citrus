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

		protected internal virtual void Add(NodeComponent component) { }

		protected internal virtual void Remove(NodeComponent component) { }

		protected internal virtual void Freeze(NodeComponent component) => Remove(component);

		protected internal virtual void Unfreeze(NodeComponent component) => Add(component);
	}

	public class NodeComponentProcessor<TComponent> : NodeComponentProcessor where TComponent : NodeComponent
	{
		protected NodeComponentProcessor() : base(typeof(TComponent)) { }

		protected internal sealed override void Add(NodeComponent component)
		{
			Add((TComponent)component);
		}

		protected internal sealed override void Remove(NodeComponent component)
		{
			Remove((TComponent)component);
		}

		protected internal sealed override void Freeze(NodeComponent component)
		{
			Freeze((TComponent)component);
		}

		protected internal sealed override void Unfreeze(NodeComponent component)
		{
			Unfreeze((TComponent)component);
		}

		protected virtual void Add(TComponent component) { }

		protected virtual void Remove(TComponent component) { }

		protected virtual void Freeze(TComponent component) => Remove(component);

		protected virtual void Unfreeze(TComponent component) => Add(component);
	}
}
