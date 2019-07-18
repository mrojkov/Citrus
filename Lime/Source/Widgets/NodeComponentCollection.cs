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

	public class NodeBehavior : BehaviorComponent
	{
		private bool registered;
		private LegacyEarlyBehaviorContainer earlyBehaviorContainer;
		private LegacyLateBehaviorContainer lateBehaviorContainer;

		public virtual int Order => 0;

		internal virtual void Register()
		{
			if (!registered) {
				OnRegister();
				registered = true;
			}
		}

		protected virtual void OnRegister()
		{
			if (behaviorEarlyUpdateChecker.Value.IsMethodOverriden(GetType())) {
				earlyBehaviorContainer = Owner.Components.GetOrAdd<LegacyEarlyBehaviorContainer>();
				earlyBehaviorContainer.Add(this);
			}
			if (behaviorLateUpdateChecker.Value.IsMethodOverriden(GetType())) {
				lateBehaviorContainer = Owner.Components.GetOrAdd<LegacyLateBehaviorContainer>();
				lateBehaviorContainer.Add(this);
			}
		}

		protected internal override void Start()
		{
			Register();
		}

		protected internal override void Stop()
		{
			RemoveFromContainer(earlyBehaviorContainer);
			RemoveFromContainer(lateBehaviorContainer);
			earlyBehaviorContainer = null;
			lateBehaviorContainer = null;
			registered = false;
		}

		public override NodeComponent Clone()
		{
			var clone = (NodeBehavior)base.Clone();
			clone.earlyBehaviorContainer = null;
			clone.lateBehaviorContainer = null;
			clone.registered = false;
			return clone;
		}

		private void RemoveFromContainer(LegacyBehaviorContainer container)
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

		private static ThreadLocal<OverridenBehaviorMethodChecker> behaviorEarlyUpdateChecker =
			new ThreadLocal<OverridenBehaviorMethodChecker>(() => new OverridenBehaviorMethodChecker(nameof(NodeBehavior.Update)));

		private static ThreadLocal<OverridenBehaviorMethodChecker> behaviorLateUpdateChecker =
			new ThreadLocal<OverridenBehaviorMethodChecker>(() => new OverridenBehaviorMethodChecker(nameof(NodeBehavior.LateUpdate)));

		private class OverridenBehaviorMethodChecker
		{
			private readonly string methodName;
			private readonly Dictionary<Type, bool> checkedTypes = new Dictionary<Type, bool>();

			public OverridenBehaviorMethodChecker(string methodName)
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
	[UpdateStage(typeof(EarlyUpdateStage))]
	[KeepActiveWhenNodeFrozen]
	public class LegacyEarlyBehaviorContainer : LegacyBehaviorContainer
	{
		protected override void UpdateBehavior(NodeBehavior b, float delta)
		{
			b.Update(delta);
		}
	}

	[NodeComponentDontSerialize]
	[UpdateBeforeBehavior(typeof(UpdatableNodeBehavior))]
	[UpdateStage(typeof(LateUpdateStage))]
	[KeepActiveWhenNodeFrozen]
	public class LegacyLateBehaviorContainer : LegacyBehaviorContainer
	{
		protected override void UpdateBehavior(NodeBehavior b, float delta)
		{
			b.LateUpdate(delta);
		}
	}

	public abstract class LegacyBehaviorContainer : BehaviorComponent
	{
		private List<NodeBehavior> behaviors = new List<NodeBehavior>();
		private LegacyBehaviorActivator activator;

		public bool IsEmpty => behaviors.Count == 0;

		private bool active;

		internal void SetActive(bool active)
		{
			this.active = active;
		}

		internal void Add(NodeBehavior b)
		{
			var index = 0;
			while (index < behaviors.Count && (behaviors[index] == null || behaviors[index].Order > b.Order)) {
				index++;
			}
			behaviors.Insert(index, b);
		}

		internal void Remove(NodeBehavior b)
		{
			var index = behaviors.IndexOf(b);
			if (index >= 0) {
				behaviors[index] = null;
			}
		}

		protected internal override void Start()
		{
			activator = Owner.Parent?.Components.GetOrAdd<LegacyBehaviorActivator>();
			if (activator != null) {
				activator?.AddContainer(this);
			} else {
				active = true;
			}
		}

		protected internal override void Stop()
		{
			activator?.RemoveContainer(this);
		}

		protected internal override void Update(float delta)
		{
			if (!active) {
				return;
			}
			for (var i = behaviors.Count - 1; i >= 0; i--) {
				var b = behaviors[i];
				if (b != null) {
					UpdateBehavior(b, delta);
				} else {
					behaviors.RemoveAt(i);
				}
			}
		}

		protected abstract void UpdateBehavior(NodeBehavior b, float delta);

		public override NodeComponent Clone()
		{
			// TODO: This component should not be cloned
			var clone = (LegacyBehaviorContainer)base.Clone();
			clone.behaviors = new List<NodeBehavior>();
			return clone;
		}
	}

	[NodeComponentDontSerialize]
	public class LegacyBehaviorActivator : BehaviorComponent
	{
		private bool enabled;
		private List<LegacyBehaviorContainer> containers = new List<LegacyBehaviorContainer>();

		public void AddContainer(LegacyBehaviorContainer container)
		{
			containers.Add(container);
			container.SetActive(!Owner.GloballyFrozen);
		}

		public void RemoveContainer(LegacyBehaviorContainer container)
		{
			containers.Remove(container);
			container.SetActive(false);
		}

		protected internal override void OnEnabled()
		{
			SetActive(true);
		}

		protected internal override void OnDisabled()
		{
			SetActive(false);
		}

		private void SetActive(bool active)
		{
			foreach (var i in containers) {
				i.SetActive(active);
			}
		}

		public override NodeComponent Clone()
		{
			var clone = (LegacyBehaviorActivator)base.Clone();
			clone.containers = new List<LegacyBehaviorContainer>();
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
