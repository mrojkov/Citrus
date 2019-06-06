using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public class NodeManager
	{
		private List<Node> nodes = new List<Node>();

		internal readonly AnimationSystem AnimationSystem = new AnimationSystem();
		internal readonly BehaviourSystem BehaviourSystem = new BehaviourSystem();

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
				UpdateCore(delta);
			} finally {
				updating = false;
			}
		}

		private void UpdateCore(float delta)
		{
			BehaviourSystem.StartPendingBehaviours();
			BehaviourSystem.EarlyUpdate(delta);
			AnimationSystem.Update(delta);
			BehaviourSystem.LateUpdate(delta);
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
		private int freezeCounter;

		internal BehaviourFamily Family;
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

		protected internal virtual void Start() { }

		protected internal virtual void Stop() { }

		protected internal virtual void Update(float delta) { }

		public override NodeComponent Clone()
		{
			var clone = (BehaviourComponent)base.Clone();
			clone.Family = null;
			clone.IndexInFamily = -1;
			clone.freezeCounter = 0;
			return clone;
		}
	}

	public struct BehaviourFreezeHandle : IDisposable
	{
		private BehaviourComponent b;

		public bool IsActive => b != null;

		public static readonly BehaviourFreezeHandle None = new BehaviourFreezeHandle();

		internal BehaviourFreezeHandle(BehaviourComponent behaviour)
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

	internal class BehaviourSystem
	{
		private List<List<int>> behaviourFamilyGraph = new List<List<int>>();
		private List<BehaviourFamily> behaviourFamilies = new List<BehaviourFamily>();
		private Dictionary<Type, int> behaviourFamilyIndexMap = new Dictionary<Type, int>();
		private HashSet<BehaviourComponent> pendingBehaviours = new HashSet<BehaviourComponent>(ReferenceEqualityComparer.Instance);
		private HashSet<BehaviourComponent> pendingBehaviours2 = new HashSet<BehaviourComponent>(ReferenceEqualityComparer.Instance);
		private List<BehaviourFamily> earlyBehaviourFamilyQueue = new List<BehaviourFamily>();
		private List<BehaviourFamily> lateBehaviourFamilyQueue = new List<BehaviourFamily>();
		private bool behaviourFamilyQueueDirty = false;

		public void Freeze(BehaviourComponent behaviour)
		{
			if (behaviour.Family != null) {
				behaviour.Family.Behaviours[behaviour.IndexInFamily] = null;
				behaviour.IndexInFamily = -1;
			}
		}

		public void Unfreeze(BehaviourComponent behaviour)
		{
			if (behaviour.Family != null) {
				behaviour.IndexInFamily = behaviour.Family.Behaviours.Count;
				behaviour.Family.Behaviours.Add(behaviour);
			}
		}

		public void StartPendingBehaviours()
		{
			while (pendingBehaviours.Count > 0) {
				var t = pendingBehaviours;
				pendingBehaviours = pendingBehaviours2;
				pendingBehaviours2 = t;
				foreach (var b in pendingBehaviours2) {
					var bf = behaviourFamilies[GetBehaviourFamilyIndex(b.GetType())];
					b.Family = bf;
					if (!b.Frozen) {
						b.IndexInFamily = bf.Behaviours.Count;
						bf.Behaviours.Add(b);
					}
					b.Start();
				}
				pendingBehaviours2.Clear();
			}
			if (behaviourFamilyQueueDirty) {
				BuildBehaviourFamilyQueue();
				behaviourFamilyQueueDirty = false;
			}
		}

		public void EarlyUpdate(float delta)
		{
			UpdateBehaviours(earlyBehaviourFamilyQueue, delta);
		}

		public void LateUpdate(float delta)
		{
			UpdateBehaviours(lateBehaviourFamilyQueue, delta);
		}

		private void UpdateBehaviours(List<BehaviourFamily> behaviourFamilyQueue, float delta)
		{
			foreach (var bf in behaviourFamilyQueue) {
				for (var i = bf.Behaviours.Count - 1; i >= 0; i--) {
					var b = bf.Behaviours[i];
					if (b != null) {
						b.Update(delta * b.Owner.EffectiveAnimationSpeed);
					} else {
						b = bf.Behaviours[bf.Behaviours.Count - 1];
						if (b != null) {
							b.IndexInFamily = i;
						}
						bf.Behaviours[i] = b;
						bf.Behaviours.RemoveAt(bf.Behaviours.Count - 1);
					}
				}
			}
		}

		private enum BehaviourFamilyGraphNodeColor
		{
			White,
			Gray,
			Black
		}

		private List<BehaviourFamilyGraphNodeColor> behaviourFamilyGraphNodeColors = new List<BehaviourFamilyGraphNodeColor>();

		private void BuildBehaviourFamilyQueue()
		{
			earlyBehaviourFamilyQueue.Clear();
			lateBehaviourFamilyQueue.Clear();
			behaviourFamilyGraphNodeColors.Clear();
			for (var i = 0; i < behaviourFamilyGraph.Count; i++) {
				behaviourFamilyGraphNodeColors.Add(BehaviourFamilyGraphNodeColor.White);
			}
			for (var i = 0; i < behaviourFamilyGraph.Count; i++) {
				BuildBehaviourFamilyQueue(i);
			}
		}

		private void BuildBehaviourFamilyQueue(int nodeIndex)
		{
			if (behaviourFamilyGraphNodeColors[nodeIndex] == BehaviourFamilyGraphNodeColor.Gray) {
				throw new InvalidOperationException();
			}
			if (behaviourFamilyGraphNodeColors[nodeIndex] == BehaviourFamilyGraphNodeColor.White) {
				behaviourFamilyGraphNodeColors[nodeIndex] = BehaviourFamilyGraphNodeColor.Gray;
				foreach (var i in behaviourFamilyGraph[nodeIndex]) {
					BuildBehaviourFamilyQueue(i);
				}
				behaviourFamilyGraphNodeColors[nodeIndex] = BehaviourFamilyGraphNodeColor.Black;
				var behaviourFamily = behaviourFamilies[nodeIndex];
				var queue = behaviourFamily.Late ? lateBehaviourFamilyQueue : earlyBehaviourFamilyQueue;
				queue.Add(behaviourFamilies[nodeIndex]);
			}
		}

		public void Add(BehaviourComponent behaviour)
		{
			pendingBehaviours.Add(behaviour);
		}

		public void Remove(BehaviourComponent behaviour)
		{
			if (!pendingBehaviours.Remove(behaviour) && behaviour.Family != null) {
				if (behaviour.IndexInFamily >= 0) {
					behaviour.Family.Behaviours[behaviour.IndexInFamily] = null;
					behaviour.IndexInFamily = -1;
				}
				behaviour.Family = null;
				behaviour.Stop();
			}
		}

		private int GetBehaviourFamilyIndex(Type behaviourType)
		{
			if (behaviourFamilyIndexMap.TryGetValue(behaviourType, out var index)) {
				return index;
			}
			if (!typeof(BehaviourComponent).IsAssignableFrom(behaviourType)) {
				throw new InvalidOperationException();
			}
			var late = behaviourType.GetCustomAttribute<LateBehaviourAttribute>() != null;
			index = behaviourFamilies.Count;
			behaviourFamilies.Add(new BehaviourFamily(behaviourType, late));
			behaviourFamilyGraph.Add(new List<int>());
			behaviourFamilyIndexMap.Add(behaviourType, index);
			foreach (var i in behaviourType.GetCustomAttributes<UpdateAfterBehaviourAttribute>(true)) {
				var predecessorFamilyIndex = GetBehaviourFamilyIndex(i.BehaviourType);
				if (behaviourFamilies[predecessorFamilyIndex].Late != late) {
					throw new InvalidOperationException();
				}
				behaviourFamilyGraph[index].Add(predecessorFamilyIndex);
			}
			foreach (var i in behaviourType.GetCustomAttributes<UpdateBeforeBehaviourAttribute>(true)) {
				var successorFamilyIndex = GetBehaviourFamilyIndex(i.BehaviourType);
				if (behaviourFamilies[successorFamilyIndex].Late != late) {
					throw new InvalidOperationException();
				}
				behaviourFamilyGraph[successorFamilyIndex].Add(index);
			}
			behaviourFamilyQueueDirty = true;
			return index;
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

	[AttributeUsage(AttributeTargets.Class)]
	public class LateBehaviourAttribute : Attribute
	{
	}

	internal class BehaviourFamily
	{
		public readonly bool Late;
		public readonly Type BehaviourType;
		public readonly List<BehaviourComponent> Behaviours = new List<BehaviourComponent>();

		public BehaviourFamily(Type behaviourType, bool late)
		{
			BehaviourType = behaviourType;
			Late = late;
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
