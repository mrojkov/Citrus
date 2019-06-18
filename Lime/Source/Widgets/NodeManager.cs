using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public class NodeManager
	{
		private List<Node> nodes = new List<Node>();
		private BehaviorUpdateStage behaviorEarlyUpdateStage;
		private BehaviorUpdateStage behaviorLateUpdateStage;

		internal readonly AnimationSystem AnimationSystem = new AnimationSystem();
		internal readonly BehaviorSystem BehaviorSystem = new BehaviorSystem();

		public NodeManager()
		{
			behaviorEarlyUpdateStage = BehaviorSystem.GetUpdateStage(typeof(EarlyUpdateStage));
			behaviorLateUpdateStage = BehaviorSystem.GetUpdateStage(typeof(LateUpdateStage));
		}

		public void AddNode(Node node)
		{
			if (node == null) {
				throw new ArgumentNullException(nameof(node));
			}
			if (node.Manager != null) {
				throw new ArgumentException(nameof(node));
			}
			if (node.Parent != null) {
				throw new ArgumentException(nameof(node));
			}
			nodes.Add(node);
			RegisterNode(node);
		}

		public void RemoveNode(Node node)
		{
			if (node != null && node.Manager == this && node.Parent == null) {
				nodes.Remove(node);
				UnregisterNode(node);
			}
		}

		internal void RegisterNode(Node node)
		{
			node.Manager = this;
			foreach (var component in node.Components) {
				RegisterComponent(component);
			}
			foreach (var child in node.Nodes) {
				RegisterNode(child);
			}
		}

		internal void UnregisterNode(Node node)
		{
			foreach (var child in node.Nodes) {
				UnregisterNode(child);
			}
			foreach (var component in node.Components) {
				UnregisterComponent(component);
			}
			node.Manager = null;
		}

		internal void RegisterComponent(NodeComponent component)
		{
			switch (component) {
				case AnimationComponent animation:
					AnimationSystem.Add(animation);
					break;
				case BehaviorComponent behavior:
					BehaviorSystem.Add(behavior);
					break;
			}
		}

		internal void UnregisterComponent(NodeComponent component)
		{
			switch (component) {
				case AnimationComponent animation:
					AnimationSystem.Remove(animation);
					break;
				case BehaviorComponent behavior:
					BehaviorSystem.Remove(behavior);
					break;
			}
		}

		private bool updating = false;

		public void Update(float delta)
		{
			if (updating) {
				throw new InvalidOperationException();
			}
			try {
				updating = true;
				UpdateCore(delta);
			} finally {
				updating = false;
			}
		}

		private void UpdateCore(float delta)
		{
			BehaviorSystem.StartPendingBehaviors();
			behaviorEarlyUpdateStage.Update(delta);
			AnimationSystem.Update(delta);
			behaviorLateUpdateStage.Update(delta);
			UpdateBoundingRects();
		}

		private void UpdateBoundingRects()
		{
			if (Widget.EnableViewCulling) {
				foreach (var n in nodes) {
					n.UpdateBoundingRect();
				}
			}
		}
	}

	public class BehaviorComponent : NodeComponent
	{
		protected internal virtual void Start() { }

		protected internal virtual void Stop() { }
	}

	public abstract class UpdatableBehaviorComponent : BehaviorComponent
	{
		private int freezeCounter;

		internal UpdatableBehaviorFamily Family;
		internal int IndexInFamily = -1;

		internal BehaviorSystem BehaviorSystem => Owner?.Manager?.BehaviorSystem;

		public bool Frozen => freezeCounter > 0;

		public BehaviorFreezeHandle Freeze() => new BehaviorFreezeHandle(this);

		internal void IncrementFreezeCounter()
		{
			freezeCounter++;
			if (freezeCounter == 1) {
				BehaviorSystem?.Freeze(this);
			}
		}

		internal void DecrementFreezeCounter()
		{
			if (freezeCounter == 0) {
				throw new InvalidOperationException();
			}
			freezeCounter--;
			if (freezeCounter == 0) {
				BehaviorSystem?.Unfreeze(this);
			}
		}

		protected internal abstract void Update(float delta);

		public override NodeComponent Clone()
		{
			var clone = (UpdatableBehaviorComponent)base.Clone();
			clone.Family = null;
			clone.IndexInFamily = -1;
			clone.freezeCounter = 0;
			return clone;
		}
	}

	public struct BehaviorFreezeHandle : IDisposable
	{
		private UpdatableBehaviorComponent b;

		public bool IsActive => b != null;

		public static readonly BehaviorFreezeHandle None = new BehaviorFreezeHandle();

		internal BehaviorFreezeHandle(UpdatableBehaviorComponent behavior)
		{
			b = behavior;
			b.IncrementFreezeCounter();
		}

		public void Dispose()
		{
			if (b != null) {
				b.DecrementFreezeCounter();
				b = null;
			}
		}
	}

	public class EarlyUpdateStage { }
	public class LateUpdateStage { }

	internal class BehaviorUpdateStage
	{
		private DependencyGraph<UpdatableBehaviorFamily> behaviorFamilyGraph = new DependencyGraph<UpdatableBehaviorFamily>();
		private List<UpdatableBehaviorFamily> behaviorFamilyQueue = new List<UpdatableBehaviorFamily>();
		private bool behaviorFamilyQueueDirty;

		public readonly Type StageType;

		public BehaviorUpdateStage(Type stageType)
		{
			StageType = stageType;
		}

		internal UpdatableBehaviorFamily CreateBehaviorFamily(Type behaviorType)
		{
			var bf = new UpdatableBehaviorFamily(this, behaviorFamilyGraph.Count, behaviorType);
			behaviorFamilyGraph.Add(bf);
			behaviorFamilyQueueDirty = true;
			return bf;
		}

		internal void AddDependency(UpdatableBehaviorFamily successor, UpdatableBehaviorFamily predecessor)
		{
			if (successor.UpdateStage != this || predecessor.UpdateStage != this) {
				throw new InvalidOperationException();
			}
			behaviorFamilyGraph.AddDependency(successor.Index, predecessor.Index);
			behaviorFamilyQueueDirty = true;
		}

		public void Update(float delta)
		{
			if (behaviorFamilyQueueDirty) {
				behaviorFamilyQueue.Clear();
				behaviorFamilyGraph.Walk(behaviorFamilyQueue);
				behaviorFamilyQueueDirty = false;
			}
			foreach (var bf in behaviorFamilyQueue) {
				bf.Update(delta);
			}
		}
	}

	internal class DependencyGraph<T>
	{
		private List<List<int>> edges = new List<List<int>>();
		private List<T> items = new List<T>();

		public int Count => items.Count;

		public void Add(T item)
		{
			items.Add(item);
			edges.Add(new List<int>());
		}

		public void AddDependency(int successor, int predecessor)
		{
			edges[successor].Add(predecessor);
		}

		private enum NodeColor
		{
			White,
			Gray,
			Black
		}

		private List<NodeColor> colors = new List<NodeColor>();

		public void Walk(List<T> result)
		{
			colors.Clear();
			for (var i = 0; i < items.Count; i++) {
				colors.Add(NodeColor.White);
			}
			for (var i = 0; i < items.Count; i++) {
				Walk(i, result);
			}
		}

		private void Walk(int index, List<T> result)
		{
			if (colors[index] == NodeColor.Gray) {
				throw new InvalidOperationException();
			}
			if (colors[index] == NodeColor.White) {
				colors[index] = NodeColor.Gray;
				foreach (var i in edges[index]) {
					Walk(i, result);
				}
				colors[index] = NodeColor.Black;
				result.Add(items[index]);
			}
		}
	}

	internal class BehaviorSystem
	{
		private Dictionary<Type, BehaviorUpdateStage> updateStages = new Dictionary<Type, BehaviorUpdateStage>();
		private Dictionary<Type, UpdatableBehaviorFamily> behaviorFamilies = new Dictionary<Type, UpdatableBehaviorFamily>();
		private HashSet<BehaviorComponent> pendingBehaviors = new HashSet<BehaviorComponent>(ReferenceEqualityComparer.Instance);
		private HashSet<BehaviorComponent> pendingBehaviors2 = new HashSet<BehaviorComponent>(ReferenceEqualityComparer.Instance);

		public BehaviorUpdateStage GetUpdateStage(Type stageType)
		{
			if (!updateStages.TryGetValue(stageType, out var stage)) {
				stage = new BehaviorUpdateStage(stageType);
				updateStages.Add(stageType, stage);
			}
			return stage;
		}

		public void Freeze(UpdatableBehaviorComponent behavior)
		{
			behavior.Family?.DisableBehavior(behavior);
		}

		public void Unfreeze(UpdatableBehaviorComponent behavior)
		{
			behavior.Family?.EnableBehavior(behavior);
		}

		public void StartPendingBehaviors()
		{
			while (pendingBehaviors.Count > 0) {
				var t = pendingBehaviors;
				pendingBehaviors = pendingBehaviors2;
				pendingBehaviors2 = t;
				foreach (var b in pendingBehaviors2) {
					if (b is UpdatableBehaviorComponent ub) {
						var bf = GetUpdatableBehaviorFamily(ub.GetType());
						bf.AddBehavior(ub);
					}
					b.Start();
				}
				pendingBehaviors2.Clear();
			}
		}

		public void Add(BehaviorComponent behavior)
		{
			pendingBehaviors.Add(behavior);
		}

		public void Remove(BehaviorComponent behavior)
		{
			if (!pendingBehaviors.Remove(behavior)) {
				if (behavior is UpdatableBehaviorComponent ub) {
					var bf = ub.Family;
					if (bf != null) {
						bf.RemoveBehavior(ub);
					}
				}
				behavior.Stop();
			}
		}

		private UpdatableBehaviorFamily GetUpdatableBehaviorFamily(Type behaviorType)
		{
			if (behaviorFamilies.TryGetValue(behaviorType, out var behaviorFamily)) {
				return behaviorFamily;
			}
			if (!typeof(UpdatableBehaviorComponent).IsAssignableFrom(behaviorType)) {
				throw new InvalidOperationException();
			}
			var updateStageAttr = behaviorType.GetCustomAttribute<UpdateStageAttribute>();
			if (updateStageAttr == null) {
				throw new InvalidOperationException();
			}
			var updateStage = GetUpdateStage(updateStageAttr.StageType);
			behaviorFamily = updateStage.CreateBehaviorFamily(behaviorType);
			behaviorFamilies.Add(behaviorType, behaviorFamily);
			foreach (var i in behaviorType.GetCustomAttributes<UpdateAfterBehaviorAttribute>()) {
				updateStage.AddDependency(behaviorFamily, GetUpdatableBehaviorFamily(i.BehaviorType));
			}
			foreach (var i in behaviorType.GetCustomAttributes<UpdateBeforeBehaviorAttribute>()) {
				updateStage.AddDependency(GetUpdatableBehaviorFamily(i.BehaviorType), behaviorFamily);
			}
			return behaviorFamily;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class UpdateStageAttribute : Attribute
	{
		public Type StageType { get; }

		public UpdateStageAttribute(Type stageType)
		{
			StageType = stageType;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateAfterBehaviorAttribute : Attribute
	{
		public Type BehaviorType { get; }

		public UpdateAfterBehaviorAttribute(Type behaviorType)
		{
			BehaviorType = behaviorType;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateBeforeBehaviorAttribute : Attribute
	{
		public Type BehaviorType { get; }

		public UpdateBeforeBehaviorAttribute(Type behaviorType)
		{
			BehaviorType = behaviorType;
		}
	}

	internal class UpdatableBehaviorFamily
	{
		private List<UpdatableBehaviorComponent> behaviors = new List<UpdatableBehaviorComponent>();

		public readonly BehaviorUpdateStage UpdateStage;
		public readonly int Index;
		public readonly Type BehaviorType;

		public UpdatableBehaviorFamily(BehaviorUpdateStage updateStage, int index, Type behaviorType)
		{
			UpdateStage = updateStage;
			Index = index;
			BehaviorType = behaviorType;
		}

		public void AddBehavior(UpdatableBehaviorComponent b)
		{
			b.Family = this;
			if (!b.Frozen) {
				EnableBehavior(b);
			}
		}

		public void RemoveBehavior(UpdatableBehaviorComponent b)
		{
			DisableBehavior(b);
			b.Family = null;
		}

		public void EnableBehavior(UpdatableBehaviorComponent b)
		{
			b.IndexInFamily = behaviors.Count;
			behaviors.Add(b);
		}

		public void DisableBehavior(UpdatableBehaviorComponent b)
		{
			if (b.IndexInFamily >= 0) {
				behaviors[b.IndexInFamily] = null;
				b.IndexInFamily = -1;
			}
		}

		public void Update(float delta)
		{
			for (var i = behaviors.Count - 1; i >= 0; i--) {
				var b = behaviors[i];
				if (b != null) {
					b.Update(delta * b.Owner.EffectiveAnimationSpeed);
				} else {
					b = behaviors[behaviors.Count - 1];
					if (b != null) {
						b.IndexInFamily = i;
					}
					behaviors[i] = b;
					behaviors.RemoveAt(behaviors.Count - 1);
				}
			}
		}
	}

	internal class AnimationSystem
	{
		private List<List<Animation>> runningAnimationsByDepth = new List<List<Animation>>();

		public void Add(AnimationComponent component)
		{
			foreach (var a in component.Animations) {
				if (a.IsRunning) {
					AddRunningAnimation(a);
				}
			}
		}

		public void Remove(AnimationComponent component)
		{
			foreach (var a in component.Animations) {
				RemoveRunningAnimation(a);
			}
		}

		internal void AddRunningAnimation(Animation animation)
		{
			var depth = GetDepth(animation.OwnerNode);
			var list = GetRunningAnimationList(depth);
			animation.Depth = depth;
			animation.Index = list.Count;
			list.Add(animation);
		}

		internal void RemoveRunningAnimation(Animation animation)
		{
			if (animation.Depth >= 0) {
				var list = GetRunningAnimationList(animation.Depth);
				list[animation.Index] = null;
				animation.Depth = -1;
				animation.Index = -1;
			}
		}

		private List<Animation> GetRunningAnimationList(int depth)
		{
			while (depth >= runningAnimationsByDepth.Count) {
				runningAnimationsByDepth.Add(new List<Animation>());
			}
			return runningAnimationsByDepth[depth];
		}

		public void Update(float delta)
		{
			for (var i = 0; i < runningAnimationsByDepth.Count; i++) {
				var runningAnimations = runningAnimationsByDepth[i];
				for (var j = runningAnimations.Count - 1; j >= 0; j--) {
					var a = runningAnimations[j];
					if (a != null) {
						a.Advance(delta * a.OwnerNode.EffectiveAnimationSpeed);
					} else {
						a = runningAnimations[runningAnimations.Count - 1];
						if (a != null) {
							a.Index = j;
						}
						runningAnimations[j] = a;
						runningAnimations.RemoveAt(runningAnimations.Count - 1);
					}
				}
			}
		}

		private int GetDepth(Node node)
		{
			var depth = 0;
			var p = node.Parent;
			while (p != null) {
				depth++;
				p = p.Parent;
			}
			return depth;
		}
	}
}
