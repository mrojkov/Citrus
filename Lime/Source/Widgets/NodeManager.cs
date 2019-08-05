using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public class NodeManager
	{
		private Dictionary<Type, List<NodeComponentProcessor>> processorsByComponentType = new Dictionary<Type, List<NodeComponentProcessor>>();

		public NodeManagerRootNodeCollection RootNodes { get; }

		public NodeManagerProcessorCollection Processors { get; }

		public IServiceProvider ServiceProvider { get; }

		public NodeManager(IServiceProvider serviceProvider)
		{
			ServiceProvider = serviceProvider;
			RootNodes = new NodeManagerRootNodeCollection(this);
			Processors = new NodeManagerProcessorCollection(this);
		}

		internal void RegisterNodeProcessor(NodeProcessor processor)
		{
			processor.Manager = this;
			processor.Start();
			if (processor is NodeComponentProcessor componentProcessor) {
				foreach (var (componentType, componentProcessors) in processorsByComponentType) {
					if (componentProcessor.TargetComponentType.IsAssignableFrom(componentType)) {
						componentProcessors.Add(componentProcessor);
					}
				}
				foreach (var node in RootNodes) {
					AddComponentsToProcessor(node, componentProcessor);
				}
			}
		}

		private void AddComponentsToProcessor(Node node, NodeComponentProcessor processor)
		{
			foreach (var component in node.Components) {
				if (processor.TargetComponentType.IsAssignableFrom(component.GetType())) {
					processor.Add(component);
				}
			}
			foreach (var child in node.Nodes) {
				AddComponentsToProcessor(child, processor);
			}
		}

		internal void UnregisterNodeProcessor(NodeProcessor processor)
		{
			if (processor is NodeComponentProcessor componentProcessor) {
				foreach (var (componentType, componentProcessors) in processorsByComponentType) {
					if (componentProcessor.TargetComponentType.IsAssignableFrom(componentType)) {
						componentProcessors.Remove(componentProcessor);
					}
				}
			}
			processor.Manager = null;
			processor.Stop();
		}

		private List<NodeComponentProcessor> GetProcessorsForComponentType(Type type)
		{
			if (processorsByComponentType.TryGetValue(type, out var targetProcessors)) {
				return targetProcessors;
			}
			targetProcessors = new List<NodeComponentProcessor>();
			foreach (var p in Processors) {
				if (p is NodeComponentProcessor cp && cp.TargetComponentType.IsAssignableFrom(type)) {
					targetProcessors.Add(cp);
				}
			}
			processorsByComponentType.Add(type, targetProcessors);
			return targetProcessors;
		}

		internal void RegisterNode(Node node)
		{
			node.Manager = this;
			if (node.GloballyFrozen) {
				frozenNodes.Add(node);
			}
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
			frozenNodes.Remove(node);
			node.Manager = null;
		}

		private HashSet<Node> frozenNodes = new HashSet<Node>();

		internal void OnFilterChanged(Node node)
		{
			var frozen = node.GloballyFrozen;
			var frozenChanged = frozen ? frozenNodes.Add(node) : frozenNodes.Remove(node);
			if (frozenChanged) {
				foreach (var c in node.Components) {
					foreach (var p in GetProcessorsForComponentType(c.GetType())) {
						p.OnNodeFrozenChanged(c);
					}
				}
				foreach (var n in node.Nodes) {
					OnFilterChanged(n);
				}
			}
		}

		internal void RegisterComponent(NodeComponent component)
		{
			foreach (var p in GetProcessorsForComponentType(component.GetType())) {
				p.Add(component);
			}
		}

		internal void UnregisterComponent(NodeComponent component)
		{
			foreach (var p in GetProcessorsForComponentType(component.GetType())) {
				p.Remove(component);
			}
		}

		public void Update(float delta)
		{
			foreach (var p in Processors) {
				p.Update(delta);
			}
		}
	}

	public class BehaviorComponent : NodeComponent
	{
		internal LinkedListNode<BehaviorComponent> StartQueueNode;
		internal BehaviorFamily Family;
		internal int IndexInFamily = -1;

		protected internal virtual void Start() { }

		protected internal virtual void Stop() { }

		protected internal virtual void Update(float delta) { }

		protected internal virtual void OnOwnerFrozenChanged() { }

		public override NodeComponent Clone()
		{
			var clone = (BehaviorComponent)base.Clone();
			clone.StartQueueNode = null;
			clone.Family = null;
			clone.IndexInFamily = -1;
			return clone;
		}
	}

	public class EarlyUpdateStage { }
	public class LateUpdateStage { }

	internal class BehaviorUpdateStage
	{
		private DependencyGraph<BehaviorFamily> behaviorFamilyGraph = new DependencyGraph<BehaviorFamily>();
		private List<BehaviorFamily> behaviorFamilyQueue = new List<BehaviorFamily>();
		private bool behaviorFamilyQueueDirty;

		public readonly Type StageType;

		public BehaviorUpdateStage(Type stageType)
		{
			StageType = stageType;
		}

		internal BehaviorFamily CreateBehaviorFamily(Type behaviorType, bool keepActiveWhenNodeFrozen)
		{
			var bf = new BehaviorFamily(this, behaviorFamilyGraph.Count, behaviorType, keepActiveWhenNodeFrozen);
			behaviorFamilyGraph.Add(bf);
			behaviorFamilyQueueDirty = true;
			return bf;
		}

		internal void AddDependency(BehaviorFamily successor, BehaviorFamily predecessor)
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
		private BehaviorUpdateStage defaultUpdateStage;
		private Dictionary<Type, BehaviorUpdateStage> updateStages = new Dictionary<Type, BehaviorUpdateStage>();
		private Dictionary<Type, BehaviorFamily> behaviorFamilies = new Dictionary<Type, BehaviorFamily>();
		private LinkedList<BehaviorComponent> behaviorsToStart = new LinkedList<BehaviorComponent>();

		public BehaviorSystem(Type defaultUpdateStageType)
		{
			defaultUpdateStage = GetUpdateStage(defaultUpdateStageType);
		}

		public BehaviorUpdateStage GetUpdateStage(Type stageType)
		{
			if (!updateStages.TryGetValue(stageType, out var stage)) {
				stage = new BehaviorUpdateStage(stageType);
				updateStages.Add(stageType, stage);
			}
			return stage;
		}

		public void StartPendingBehaviors()
		{
			while (behaviorsToStart.Count > 0) {
				var b = behaviorsToStart.First.Value;
				behaviorsToStart.RemoveFirst();
				b.StartQueueNode = null;
				b.Family = GetBehaviorFamily(b.GetType());
				b.Start();
				EnqueueDequeueBehavior(b);
			}
		}

		public void Add(BehaviorComponent behavior)
		{
			behavior.StartQueueNode = behaviorsToStart.AddLast(behavior);
		}

		public void Remove(BehaviorComponent behavior)
		{
			if (behavior.StartQueueNode != null) {
				behaviorsToStart.Remove(behavior.StartQueueNode);
				behavior.StartQueueNode = null;
			} else {
				behavior.Family?.Dequeue(behavior);
				behavior.Family = null;
				behavior.Stop();
			}
		}

		public void OnNodeFrozenChanged(BehaviorComponent behavior)
		{
			if (behavior.StartQueueNode == null) {
				EnqueueDequeueBehavior(behavior);
				behavior.OnOwnerFrozenChanged();
			}
		}

		private void EnqueueDequeueBehavior(BehaviorComponent behavior)
		{
			if (behavior.Family != null) {
				var enabled = !behavior.Owner.GloballyFrozen || behavior.Family.KeepActiveWhenNodeFrozen;
				if (enabled) {
					behavior.Family.Enqueue(behavior);
				} else {
					behavior.Family.Dequeue(behavior);
				}
			}
		}

		private BehaviorFamily GetBehaviorFamily(Type behaviorType)
		{
			if (behaviorFamilies.TryGetValue(behaviorType, out var behaviorFamily)) {
				return behaviorFamily;
			}
			var updateStage = defaultUpdateStage;
			var updateStageAttr = behaviorType.GetCustomAttribute<UpdateStageAttribute>();
			if (updateStageAttr != null) {
				updateStage = GetUpdateStage(updateStageAttr.StageType);
			}
			var keepActiveWhenNodeFrozen = behaviorType.IsDefined(typeof(KeepActiveWhenNodeFrozenAttribute));
			behaviorFamily = updateStage.CreateBehaviorFamily(behaviorType, keepActiveWhenNodeFrozen);
			behaviorFamilies.Add(behaviorType, behaviorFamily);
			foreach (var i in behaviorType.GetCustomAttributes<UpdateAfterBehaviorAttribute>()) {
				updateStage.AddDependency(behaviorFamily, GetBehaviorFamily(i.BehaviorType));
			}
			foreach (var i in behaviorType.GetCustomAttributes<UpdateBeforeBehaviorAttribute>()) {
				updateStage.AddDependency(GetBehaviorFamily(i.BehaviorType), behaviorFamily);
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

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class KeepActiveWhenNodeFrozenAttribute : Attribute
	{
	}

	internal class BehaviorFamily
	{
		private List<BehaviorComponent> behaviors = new List<BehaviorComponent>();

		public readonly BehaviorUpdateStage UpdateStage;
		public readonly int Index;
		public readonly Type BehaviorType;
		public readonly bool KeepActiveWhenNodeFrozen;

		public BehaviorFamily(BehaviorUpdateStage updateStage, int index, Type behaviorType, bool keepActiveWhenNodeFrozen)
		{
			UpdateStage = updateStage;
			Index = index;
			BehaviorType = behaviorType;
			KeepActiveWhenNodeFrozen = keepActiveWhenNodeFrozen;
		}

		public bool Enqueue(BehaviorComponent b)
		{
			if (b.IndexInFamily < 0) {
				b.IndexInFamily = behaviors.Count;
				behaviors.Add(b);
				return true;
			}
			return false;
		}

		public bool Dequeue(BehaviorComponent b)
		{
			if (b.IndexInFamily >= 0) {
				behaviors[b.IndexInFamily] = null;
				b.IndexInFamily = -1;
				return true;
			}
			return false;
		}

		public void Update(float delta)
		{
			var idx = 0;
			while (idx < behaviors.Count) {
				var b = behaviors[idx];
				if (b != null) {
					b.Update(delta * b.Owner.EffectiveAnimationSpeed);
					idx++;
				} else {
					b = behaviors[behaviors.Count - 1];
					if (b != null) {
						b.IndexInFamily = idx;
					}
					behaviors[idx] = b;
					behaviors.RemoveAt(behaviors.Count - 1);
				}
			}
		}
	}
}
