using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public class NodeManager
	{
		private List<Node> nodes = new List<Node>();
		private BehaviourUpdateStage behaviourEarlyUpdateStage;
		private BehaviourUpdateStage behaviourLateUpdateStage;

		internal readonly AnimationSystem AnimationSystem = new AnimationSystem();
		internal readonly BehaviourSystem BehaviourSystem = new BehaviourSystem();

		public NodeManager()
		{
			behaviourEarlyUpdateStage = BehaviourSystem.GetUpdateStage(typeof(EarlyUpdateStage));
			behaviourLateUpdateStage = BehaviourSystem.GetUpdateStage(typeof(LateUpdateStage));
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
				case BehaviourComponent behaviour:
					BehaviourSystem.Add(behaviour);
					break;
			}
		}

		internal void UnregisterComponent(NodeComponent component)
		{
			switch (component) {
				case AnimationComponent animation:
					AnimationSystem.Remove(animation);
					break;
				case BehaviourComponent behaviour:
					BehaviourSystem.Remove(behaviour);
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
			BehaviourSystem.StartPendingBehaviours();
			behaviourEarlyUpdateStage.Update(delta);
			AnimationSystem.Update(delta);
			behaviourLateUpdateStage.Update(delta);
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

	public class BehaviourComponent : NodeComponent
	{
		protected internal virtual void Start() { }

		protected internal virtual void Stop() { }
	}

	public abstract class UpdatableBehaviourComponent : BehaviourComponent
	{
		private int freezeCounter;

		internal UpdatableBehaviourFamily Family;
		internal int IndexInFamily = -1;

		internal BehaviourSystem BehaviourSystem => Owner?.Manager?.BehaviourSystem;

		public bool Frozen => freezeCounter > 0;

		public BehaviourFreezeHandle Freeze() => new BehaviourFreezeHandle(this);

		internal void IncrementFreezeCounter()
		{
			freezeCounter++;
			if (freezeCounter == 1) {
				BehaviourSystem?.Freeze(this);
			}
		}

		internal void DecrementFreezeCounter()
		{
			if (freezeCounter == 0) {
				throw new InvalidOperationException();
			}
			freezeCounter--;
			if (freezeCounter == 0) {
				BehaviourSystem?.Unfreeze(this);
			}
		}

		protected internal abstract void Update(float delta);

		public override NodeComponent Clone()
		{
			var clone = (UpdatableBehaviourComponent)base.Clone();
			clone.Family = null;
			clone.IndexInFamily = -1;
			clone.freezeCounter = 0;
			return clone;
		}
	}

	public struct BehaviourFreezeHandle : IDisposable
	{
		private UpdatableBehaviourComponent b;

		public bool IsActive => b != null;

		public static readonly BehaviourFreezeHandle None = new BehaviourFreezeHandle();

		internal BehaviourFreezeHandle(UpdatableBehaviourComponent behaviour)
		{
			b = behaviour;
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

	internal class BehaviourUpdateStage
	{
		private DependencyGraph<UpdatableBehaviourFamily> behaviourFamilyGraph = new DependencyGraph<UpdatableBehaviourFamily>();
		private List<UpdatableBehaviourFamily> behaviourFamilyQueue = new List<UpdatableBehaviourFamily>();
		private bool behaviourFamilyQueueDirty;

		public readonly Type StageType;

		public BehaviourUpdateStage(Type stageType)
		{
			StageType = stageType;
		}

		internal UpdatableBehaviourFamily CreateBehaviourFamily(Type behaviourType)
		{
			var bf = new UpdatableBehaviourFamily(this, behaviourFamilyGraph.Count, behaviourType);
			behaviourFamilyGraph.Add(bf);
			behaviourFamilyQueueDirty = true;
			return bf;
		}

		internal void AddDependency(UpdatableBehaviourFamily successor, UpdatableBehaviourFamily predecessor)
		{
			if (successor.UpdateStage != this || predecessor.UpdateStage != this) {
				throw new InvalidOperationException();
			}
			behaviourFamilyGraph.AddDependency(successor.Index, predecessor.Index);
			behaviourFamilyQueueDirty = true;
		}

		public void Update(float delta)
		{
			if (behaviourFamilyQueueDirty) {
				behaviourFamilyQueue.Clear();
				behaviourFamilyGraph.Walk(behaviourFamilyQueue);
				behaviourFamilyQueueDirty = false;
			}
			foreach (var bf in behaviourFamilyQueue) {
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

	internal class BehaviourSystem
	{
		private Dictionary<Type, BehaviourUpdateStage> updateStages = new Dictionary<Type, BehaviourUpdateStage>();
		private Dictionary<Type, UpdatableBehaviourFamily> behaviourFamilies = new Dictionary<Type, UpdatableBehaviourFamily>();
		private HashSet<BehaviourComponent> pendingBehaviours = new HashSet<BehaviourComponent>(ReferenceEqualityComparer.Instance);
		private HashSet<BehaviourComponent> pendingBehaviours2 = new HashSet<BehaviourComponent>(ReferenceEqualityComparer.Instance);

		public BehaviourUpdateStage GetUpdateStage(Type stageType)
		{
			if (!updateStages.TryGetValue(stageType, out var stage)) {
				stage = new BehaviourUpdateStage(stageType);
				updateStages.Add(stageType, stage);
			}
			return stage;
		}

		public void Freeze(UpdatableBehaviourComponent behaviour)
		{
			behaviour.Family?.DisableBehaviour(behaviour);
		}

		public void Unfreeze(UpdatableBehaviourComponent behaviour)
		{
			behaviour.Family?.EnableBehaviour(behaviour);
		}

		public void StartPendingBehaviours()
		{
			while (pendingBehaviours.Count > 0) {
				var t = pendingBehaviours;
				pendingBehaviours = pendingBehaviours2;
				pendingBehaviours2 = t;
				foreach (var b in pendingBehaviours2) {
					if (b is UpdatableBehaviourComponent ub) {
						var bf = GetUpdatableBehaviourFamily(ub.GetType());
						bf.AddBehaviour(ub);
					}
					b.Start();
				}
				pendingBehaviours2.Clear();
			}
		}

		public void Add(BehaviourComponent behaviour)
		{
			pendingBehaviours.Add(behaviour);
		}

		public void Remove(BehaviourComponent behaviour)
		{
			if (!pendingBehaviours.Remove(behaviour)) {
				if (behaviour is UpdatableBehaviourComponent ub) {
					var bf = ub.Family;
					if (bf != null) {
						bf.RemoveBehaviour(ub);
					}
				}
				behaviour.Stop();
			}
		}

		private UpdatableBehaviourFamily GetUpdatableBehaviourFamily(Type behaviourType)
		{
			if (behaviourFamilies.TryGetValue(behaviourType, out var behaviourFamily)) {
				return behaviourFamily;
			}
			if (!typeof(UpdatableBehaviourComponent).IsAssignableFrom(behaviourType)) {
				throw new InvalidOperationException();
			}
			var updateStageAttr = behaviourType.GetCustomAttribute<UpdateStageAttribute>();
			if (updateStageAttr == null) {
				throw new InvalidOperationException();
			}
			var updateStage = GetUpdateStage(updateStageAttr.StageType);
			behaviourFamily = updateStage.CreateBehaviourFamily(behaviourType);
			behaviourFamilies.Add(behaviourType, behaviourFamily);
			foreach (var i in behaviourType.GetCustomAttributes<UpdateAfterBehaviourAttribute>()) {
				updateStage.AddDependency(behaviourFamily, GetUpdatableBehaviourFamily(i.BehaviourType));
			}
			foreach (var i in behaviourType.GetCustomAttributes<UpdateBeforeBehaviourAttribute>()) {
				updateStage.AddDependency(GetUpdatableBehaviourFamily(i.BehaviourType), behaviourFamily);
			}
			return behaviourFamily;
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
	public class UpdateAfterBehaviourAttribute : Attribute
	{
		public Type BehaviourType { get; }

		public UpdateAfterBehaviourAttribute(Type behaviourType)
		{
			BehaviourType = behaviourType;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateBeforeBehaviourAttribute : Attribute
	{
		public Type BehaviourType { get; }

		public UpdateBeforeBehaviourAttribute(Type behaviourType)
		{
			BehaviourType = behaviourType;
		}
	}

	internal class UpdatableBehaviourFamily
	{
		private List<UpdatableBehaviourComponent> behaviours = new List<UpdatableBehaviourComponent>();

		public readonly BehaviourUpdateStage UpdateStage;
		public readonly int Index;
		public readonly Type BehaviourType;

		public UpdatableBehaviourFamily(BehaviourUpdateStage updateStage, int index, Type behaviourType)
		{
			UpdateStage = updateStage;
			Index = index;
			BehaviourType = behaviourType;
		}

		public void AddBehaviour(UpdatableBehaviourComponent b)
		{
			b.Family = this;
			if (!b.Frozen) {
				EnableBehaviour(b);
			}
		}

		public void RemoveBehaviour(UpdatableBehaviourComponent b)
		{
			DisableBehaviour(b);
			b.Family = null;
		}

		public void EnableBehaviour(UpdatableBehaviourComponent b)
		{
			b.IndexInFamily = behaviours.Count;
			behaviours.Add(b);
		}

		public void DisableBehaviour(UpdatableBehaviourComponent b)
		{
			if (b.IndexInFamily >= 0) {
				behaviours[b.IndexInFamily] = null;
				b.IndexInFamily = -1;
			}
		}

		public void Update(float delta)
		{
			for (var i = behaviours.Count - 1; i >= 0; i--) {
				var b = behaviours[i];
				if (b != null) {
					b.Update(delta * b.Owner.EffectiveAnimationSpeed);
				} else {
					b = behaviours[behaviours.Count - 1];
					if (b != null) {
						b.IndexInFamily = i;
					}
					behaviours[i] = b;
					behaviours.RemoveAt(behaviours.Count - 1);
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
