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

		protected virtual void OnOwnerChanged(Node oldOwner) { }

		public virtual void Dispose() { }

		public static bool IsSerializable(Type type)
		{
			return !type.IsDefined(typeof(NodeComponentDontSerializeAttribute), true);
		}

		protected internal virtual void OnBeforeNodeSerialization() { }

		protected internal virtual void OnAfterNodeSerialization() { }
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

		protected internal override void Stop(Node owner)
		{
			RemoveFromContainer(earlyBehaviorContainer);
			RemoveFromContainer(lateBehaviorContainer);
			earlyBehaviorContainer = null;
			lateBehaviorContainer = null;
			registered = false;
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

		public virtual void OnTrigger(string property, object value, double animationTimeCorrection = 0)
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
	[UpdateFrozen]
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
	[UpdateFrozen]
	public class LegacyLateBehaviorContainer : LegacyBehaviorContainer
	{
		protected override void UpdateBehavior(NodeBehavior b, float delta)
		{
			b.LateUpdate(delta);
		}
	}

	public abstract class LegacyBehaviorContainer : BehaviorComponent
	{
		private bool attached;
		private List<NodeBehavior> behaviors = new List<NodeBehavior>();

		public bool IsEmpty => behaviors.Count == 0;

		internal void Add(NodeBehavior b)
		{
			var index = behaviors.Count;
			while (index > 0 && (behaviors[index - 1] == null || behaviors[index - 1].Order <= b.Order)) {
				index--;
			}
			behaviors.Insert(index, b);
			CheckActivity();
		}

		internal void Remove(NodeBehavior b)
		{
			var index = behaviors.IndexOf(b);
			if (index >= 0) {
				behaviors[index] = null;
			}
			CheckActivity();
		}

		protected internal override void Start()
		{
			attached = true;
			CheckActivity();
		}

		protected internal override void Stop(Node owner)
		{
			attached = false;
		}

		protected internal override void OnOwnerFrozenChanged()
		{
			CheckActivity();
		}

		private void CheckActivity()
		{
			if (attached) {
				if ((Owner.Parent?.GloballyFrozen ?? false) || behaviors.Count == 0) {
					Suspend();
				} else {
					Resume();
				}
			}
		}

		protected internal override void Update(float delta)
		{
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
				owner.Manager?.UnregisterComponent(component, owner);
				return true;
			}
			return false;
		}

		public override void Clear()
		{
			for (int i = 0; i < buckets.Length; i++) {
				if (buckets[i].Key > 0) {
					buckets[i].Component.Owner = null;
					owner.Manager?.UnregisterComponent(buckets[i].Component, owner);
				}
			}
			base.Clear();
		}

		public override bool Contains(NodeComponent component)
		{
			return component != null && component.Owner == owner;
		}

		[global::Yuzu.YuzuSerializeItemIf]
		public bool SerializeItemIf(int index, Object component)
		{
			return NodeComponent.IsSerializable(component.GetType());
		}
	}
}
