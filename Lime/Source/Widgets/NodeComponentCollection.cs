using System;
using System.Collections.Generic;
using System.Threading;

namespace Lime
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class NodeComponentDontSerializeAttribute : Attribute
	{ }

	public sealed class AllowedComponentOwnerTypes : Attribute
	{
		public Type[] Types;

		public AllowedComponentOwnerTypes(params Type[] types)
		{
			Types = types;
		}
	}

	public class NodeComponent : Component, IDisposable, IAnimable
	{
		IAnimable IAnimable.Owner { get => Owner; set => Owner = (Node)value; }
		private Node owner;
		public Node Owner
		{
			get
			{
				return owner;
			}

			set
			{
				if (value != owner) {
					var oldOwner = owner;
					(oldOwner as IAnimable)?.UnbindAnimators();
					owner = value;
					(owner as IAnimable)?.UnbindAnimators();
					OnOwnerChanged(oldOwner);
				}
			}
		}

		protected internal virtual void OnBeforeClone() { }
		protected internal virtual void OnAfterClone() { }
		protected virtual void OnOwnerChanged(Node oldOwner) { }

		public virtual NodeComponent Clone()
		{
			var clone = (NodeComponent)MemberwiseClone();
			clone.owner = null;
			return clone;
		}

		public virtual void Dispose() { }
	}

	//public class NodeBehavior : NodeComponent
	//{
	//	public virtual int Order => 0;

	//	public virtual void Update(float delta) { }
	//	public virtual void LateUpdate(float delta) { }
	//}

	public class NodeBehavior : BehaviourComponent
	{
		private UpdateSubBehaviour ub;
		private LateUpdateSubBehaviour lub;

		public virtual int Order => 0;

		protected internal override void Start()
		{
			Owner.Components.GetOrAdd<UpdateSubBehaviour>().Behaviors.Add(this);
			Owner.Components.GetOrAdd<LateUpdateSubBehaviour>().Behaviors.Add(this);
		}

		protected internal override void Stop()
		{
			Owner.Components.GetOrAdd<UpdateSubBehaviour>().Behaviors.Remove(this);
			Owner.Components.GetOrAdd<LateUpdateSubBehaviour>().Behaviors.Remove(this);
		}

		public new virtual void Update(float delta)
		{
		}

		public virtual void LateUpdate(float delta)
		{
		}

		[UpdateAfterBehaviour(typeof(UpdateBehaviour))]
		[UpdateBeforeBehaviour(typeof(TasksBehaviour))]
		private class UpdateSubBehaviour : BehaviourComponent
		{
			public List<NodeBehavior> Behaviors = new List<NodeBehavior>();

			protected internal override void Update(float delta)
			{
				foreach (var b in Behaviors) {
					b.Update(delta);
				}
			}
		}

		[UpdateAfterBehaviour(typeof(AnimationBehaviour))]
		[UpdateBeforeBehaviour(typeof(LateTasksBehaviour))]
		private class LateUpdateSubBehaviour : BehaviourComponent
		{
			public List<NodeBehavior> Behaviors = new List<NodeBehavior>();

			protected internal override void Update(float delta)
			{
				foreach (var b in Behaviors) {
					b.LateUpdate(delta);
				}
			}
		}
	}

	public class NodeComponentCollection : ComponentCollection<NodeComponent>
	{
		private readonly Node owner;

		public NodeComponentCollection(Node owner)
		{
			this.owner = owner;
		}

		public override void Add(NodeComponent component)
		{
			if (component.Owner != null) {
				throw new InvalidOperationException("The component is already in a collection.");
			}
			base.Add(component);
			component.Owner = owner;
			owner?.Manager?.RegisterComponent(component);
		}

		public override bool Remove(NodeComponent component)
		{
			if (base.Remove(component)) {
				component.Owner = null;
				owner?.Manager?.UnregisterComponent(component);
				return true;
			}
			return false;
		}

		public override void Clear()
		{
			for (int i = 0; i < buckets.Length; i++) {
				if (buckets[i].Key > 0) {
					buckets[i].Component.Owner = null;
					owner?.Manager?.UnregisterComponent(buckets[i].Component);
				}
			}
			base.Clear();
		}

		[global::Yuzu.YuzuSerializeItemIf]
		private bool SerializeItemIf(int index, Object component)
		{
			return !component.GetType().IsDefined(typeof(NodeComponentDontSerializeAttribute), true);
		}
	}
}
