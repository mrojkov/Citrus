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

	public class NodeBehavior : BehaviourComponent
	{
		private bool registered;
		private LegacyBehaviourContainer behaviourContainer;
		private LegacyLateBehaviourContainer lateBehaviourContainer;

		public virtual int Order => 0;

		public void Register()
		{
			if (!registered) {
				if (behaviourUpdateChecker.Value.IsMethodOverriden(GetType())) {
					behaviourContainer = Owner.Components.GetOrAdd<LegacyBehaviourContainer>();
					behaviourContainer.Add(this);
				}
				if (behaviourLateUpdateChecker.Value.IsMethodOverriden(GetType())) {
					lateBehaviourContainer = Owner.Components.GetOrAdd<LegacyLateBehaviourContainer>();
					lateBehaviourContainer.Add(this);
				}
				registered = true;
			}
		}

		protected internal override void Start()
		{
			Register();
		}

		protected internal override void Stop()
		{
			RemoveFromContainer(behaviourContainer);
			RemoveFromContainer(lateBehaviourContainer);
			behaviourContainer = null;
			lateBehaviourContainer = null;
			registered = false;
		}

		public override NodeComponent Clone()
		{
			var clone = (NodeBehavior)base.Clone();
			clone.behaviourContainer = null;
			clone.lateBehaviourContainer = null;
			clone.registered = false;
			return clone;
		}

		private void RemoveFromContainer(LegacyBehaviourContainerBase container)
		{
			if (container != null) {
				container.Remove(this);
				if (container.IsEmpty) {
					container.Owner?.Components.Remove(container);
				}
			}
		}

		public new virtual void Update(float delta)
		{
		}

		public virtual void LateUpdate(float delta)
		{
		}

		private static ThreadLocal<OverridenBehaviourMethodChecker> behaviourUpdateChecker =
			new ThreadLocal<OverridenBehaviourMethodChecker>(() => new OverridenBehaviourMethodChecker(nameof(NodeBehavior.Update)));

		private static ThreadLocal<OverridenBehaviourMethodChecker> behaviourLateUpdateChecker =
			new ThreadLocal<OverridenBehaviourMethodChecker>(() => new OverridenBehaviourMethodChecker(nameof(NodeBehavior.LateUpdate)));

		private class OverridenBehaviourMethodChecker
		{
			private readonly string methodName;
			private readonly Dictionary<Type, bool> checkedTypes = new Dictionary<Type, bool>();

			public OverridenBehaviourMethodChecker(string methodName)
			{
				this.methodName = methodName;
			}

			public bool IsMethodOverriden(Type type)
			{
				bool result;
				if (!checkedTypes.TryGetValue(type, out result)) {
					result = type.GetMethod(methodName).DeclaringType != typeof(NodeBehavior);
					checkedTypes.Add(type, result);
				}
				return result;
			}
		}
	}

	[NodeComponentDontSerialize]
	[UpdateBeforeBehaviour(typeof(AnimationComponent))]
	public class LegacyBehaviourContainer : LegacyBehaviourContainerBase
	{
		protected internal override void Update(float delta)
		{
			for (var i = behaviours.Count - 1; i >= 0; i--) {
				var b = behaviours[i];
				if (b != null) {
					b.Update(delta);
				} else {
					behaviours.RemoveAt(i);
				}
			}
		}
	}

	[NodeComponentDontSerialize]
	[UpdateAfterBehaviour(typeof(AnimationComponent))]
	[UpdateBeforeBehaviour(typeof(UpdatableNodeBehaviour))]
	public class LegacyLateBehaviourContainer : LegacyBehaviourContainerBase
	{
		protected internal override void Update(float delta)
		{
			for (var i = behaviours.Count - 1; i >= 0; i--) {
				var b = behaviours[i];
				if (b != null) {
					b.LateUpdate(delta);
				} else {
					behaviours.RemoveAt(i);
				}
			}
		}
	}

	public class LegacyBehaviourContainerBase : BehaviourComponent
	{
		protected readonly List<NodeBehavior> behaviours = new List<NodeBehavior>();

		public bool IsEmpty => behaviours.Count == 0;

		internal void Add(NodeBehavior b)
		{
			var index = 0;
			while (index < behaviours.Count && (behaviours[index] == null || behaviours[index].Order < b.Order)) {
				index++;
			}
			behaviours.Insert(index, b);
		}

		internal void Remove(NodeBehavior b)
		{
			var index = behaviours.IndexOf(b);
			if (index >= 0) {
				behaviours[index] = null;
			}
		}

		public override NodeComponent Clone()
		{
			// TODO: This component shold not be cloned
			var clone = (LegacyBehaviourContainerBase)base.Clone();
			clone.behaviours.Clear();
			return clone;
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
			if (component != null && component.Owner == owner) {
				base.Remove(component);
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

		public override bool Contains(NodeComponent component)
		{
			return component != null && component.Owner == owner;
		}

		[global::Yuzu.YuzuSerializeItemIf]
		private bool SerializeItemIf(int index, Object component)
		{
			return !component.GetType().IsDefined(typeof(NodeComponentDontSerializeAttribute), true);
		}
	}
}
