using System;
using System.Collections.Generic;
using System.Threading;

namespace Lime
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class TangerineRegisterComponentAttribute : Attribute { }

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class NodeComponentDontSerializeAttribute : Attribute
	{ }

	public sealed class AllowedOwnerTypes : Attribute
	{
		public Type[] Types;

		public AllowedOwnerTypes(params Type[] types)
		{
			Types = types;
		}
	}

	public class NodeComponent : Component, IDisposable
	{
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
					owner = value;
					OnOwnerChanged(oldOwner);
				}
			}
		}

		protected virtual void OnOwnerChanged(Node oldOwner) { }

		public virtual NodeComponent Clone()
		{
			var clone = (NodeComponent)MemberwiseClone();
			clone.owner = null;
			return clone;
		}

		public virtual void Dispose() { }
	}

	public class NodeBehavior : NodeComponent
	{
		public virtual int Order => 0;

		public virtual void Update(float delta) { }
		public virtual void LateUpdate(float delta) { }
	}

	public class NodeComponentCollection : ComponentCollection<NodeComponent>
	{
		internal static readonly NodeBehavior[] EmptyBehaviors = new NodeBehavior[0];

		private readonly Node owner;

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
			var behaviour = component as NodeBehavior;
			if (behaviour != null) {
				var type = behaviour.GetType();
				if (behaviourUpdateChecker.Value.IsMethodOverriden(type)) {
					owner.Behaviours = InsertBehaviour(owner.Behaviours, behaviour);
				}
				if (behaviourLateUpdateChecker.Value.IsMethodOverriden(type)) {
					owner.LateBehaviours = InsertBehaviour(owner.LateBehaviours, behaviour);
				}
			}
			component.Owner = owner;
		}

		private static NodeBehavior[] InsertBehaviour(NodeBehavior[] list, NodeBehavior behaviour)
		{
			var newList = new NodeBehavior[list.Length + 1];
			var i = 0;
			int? insertIndex = null;
			foreach (var b in list) {
				if (!insertIndex.HasValue && b.Order > behaviour.Order) {
					insertIndex = i++;
				}
				newList[i++] = b;
			}
			newList[insertIndex ?? list.Length] = behaviour;
			return newList;
		}

		public override bool Remove(NodeComponent component)
		{
			if (base.Remove(component)) {
				component.Owner = null;
				var behaviour = component as NodeBehavior;
				if (behaviour != null) {
					var behaviorType = behaviour.GetType();
					if (behaviourUpdateChecker.Value.IsMethodOverriden(behaviorType)) {
						owner.Behaviours = RemoveBehaviour(owner.Behaviours, behaviour);
					}
					if (behaviourLateUpdateChecker.Value.IsMethodOverriden(behaviorType)) {
						owner.LateBehaviours = RemoveBehaviour(owner.LateBehaviours, behaviour);
					}
				}
				return true;
			}
			return false;
		}

		private static NodeBehavior[] RemoveBehaviour(NodeBehavior[] list, NodeBehavior behaviour)
		{
			if (list.Length == 1) {
				return EmptyBehaviors;
			}
			var newList = new NodeBehavior[list.Length - 1];
			var i = 0;
			foreach (var b in list) {
				if (b != behaviour) {
					newList[i++] = b;
				}
			}
			return newList;
		}

		public override void Clear()
		{
			for (int i = 0; i < buckets.Length; i++) {
				if (buckets[i].Key > 0) {
					buckets[i].Component.Owner = null;
				}
			}
			base.Clear();
			owner.Behaviours = owner.LateBehaviours = EmptyBehaviors;
		}

		private static ThreadLocal<OverridenBehaviourMethodChecker> behaviourUpdateChecker =>
			new ThreadLocal<OverridenBehaviourMethodChecker>(() => new OverridenBehaviourMethodChecker(nameof(NodeBehavior.Update)));

		private static ThreadLocal<OverridenBehaviourMethodChecker> behaviourLateUpdateChecker =>
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

		[Yuzu.YuzuSerializeItemIf]
		private bool SerializeItemIf(int index, Object component)
		{
			return !component.GetType().IsDefined(typeof(NodeComponentDontSerializeAttribute), true);
		}
	}
}
