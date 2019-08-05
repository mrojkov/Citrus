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
		internal BehaviorUpdateFamily UpdateFamily;
		internal int IndexInUpdateFamily = -1;

		protected internal bool Suspended { get; private set; }

		protected internal virtual void Start() { }

		protected internal virtual void Stop() { }

		protected internal virtual void Update(float delta) { }

		protected internal virtual void OnOwnerFrozenChanged() { }

		protected void Suspend()
		{
			Suspended = true;
			UpdateFamily.Filter(this);
		}

		public void Resume()
		{
			Suspended = false;
			UpdateFamily.Filter(this);
		}

		public override NodeComponent Clone()
		{
			var clone = (BehaviorComponent)base.Clone();
			clone.StartQueueNode = null;
			clone.Suspended = false;
			clone.UpdateFamily = null;
			clone.IndexInUpdateFamily = -1;
			return clone;
		}
	}

	public class PreEarlyUpdateStage { }

	public class EarlyUpdateStage { }

	public class PostEarlyUpdateStage { }

	public class PreLateUpdateStage { }

	public class LateUpdateStage { }

	public class PostLateUpdateStage { }

	internal class BehaviorUpdateStage
	{
		private DependencyGraph<BehaviorUpdateFamily> familyGraph = new DependencyGraph<BehaviorUpdateFamily>();
		private List<BehaviorUpdateFamily> familyQueue = new List<BehaviorUpdateFamily>();
		private bool familyQueueDirty;

		public readonly Type StageType;

		public BehaviorUpdateStage(Type stageType)
		{
			StageType = stageType;
		}

		internal BehaviorUpdateFamily CreateFamily(Type behaviorType, bool updateFrozen)
		{
			var family = new BehaviorUpdateFamily(this, familyGraph.Count, behaviorType, updateFrozen);
			familyGraph.Add(family);
			familyQueueDirty = true;
			return family;
		}

		internal void AddDependency(BehaviorUpdateFamily successor, BehaviorUpdateFamily predecessor)
		{
			if (successor.UpdateStage != this || predecessor.UpdateStage != this) {
				throw new InvalidOperationException();
			}
			familyGraph.AddDependency(successor.Index, predecessor.Index);
			familyQueueDirty = true;
		}

		public void Update(float delta)
		{
			if (familyQueueDirty) {
				familyQueue.Clear();
				familyGraph.Walk(familyQueue);
				familyQueueDirty = false;
			}
			foreach (var f in familyQueue) {
				f.Update(delta);
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
		private Dictionary<Type, BehaviorUpdateFamily> updateFamilies = new Dictionary<Type, BehaviorUpdateFamily>();
		private LinkedList<BehaviorComponent> behaviorsToStart = new LinkedList<BehaviorComponent>();

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
				b.UpdateFamily = GetUpdateFamily(b.GetType());
				b.UpdateFamily?.Filter(b);
				b.Start();
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
				behavior.UpdateFamily?.Dequeue(behavior);
				behavior.UpdateFamily = null;
				behavior.Stop();
			}
		}

		public void OnNodeFrozenChanged(BehaviorComponent behavior)
		{
			if (behavior.StartQueueNode == null) {
				behavior.UpdateFamily?.Filter(behavior);
				behavior.OnOwnerFrozenChanged();
			}
		}

		private BehaviorUpdateFamily GetUpdateFamily(Type behaviorType)
		{
			if (updateFamilies.TryGetValue(behaviorType, out var behaviorFamily)) {
				return behaviorFamily;
			}
			var updateStageAttr = behaviorType.GetCustomAttribute<UpdateStageAttribute>();
			if (updateStageAttr == null) {
				return null;
			}
			var updateStage = GetUpdateStage(updateStageAttr.StageType);
			var updateFrozen = behaviorType.IsDefined(typeof(UpdateFrozenAttribute));
			behaviorFamily = updateStage.CreateFamily(behaviorType, updateFrozen);
			updateFamilies.Add(behaviorType, behaviorFamily);
			foreach (var i in behaviorType.GetCustomAttributes<UpdateAfterBehaviorAttribute>()) {
				updateStage.AddDependency(behaviorFamily, GetUpdateFamily(i.BehaviorType));
			}
			foreach (var i in behaviorType.GetCustomAttributes<UpdateBeforeBehaviorAttribute>()) {
				updateStage.AddDependency(GetUpdateFamily(i.BehaviorType), behaviorFamily);
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
	public class UpdateFrozenAttribute : Attribute
	{
	}

	internal class BehaviorUpdateFamily
	{
		private List<BehaviorComponent> behaviors = new List<BehaviorComponent>();

		public readonly BehaviorUpdateStage UpdateStage;
		public readonly int Index;
		public readonly Type BehaviorType;
		public readonly bool UpdateFrozen;

		public BehaviorUpdateFamily(BehaviorUpdateStage updateStage, int index, Type behaviorType, bool updateFrozen)
		{
			UpdateStage = updateStage;
			Index = index;
			BehaviorType = behaviorType;
			UpdateFrozen = updateFrozen;
		}

		public void Filter(BehaviorComponent b)
		{
			if ((b.Owner.GloballyFrozen && !UpdateFrozen) || b.Suspended) {
				Dequeue(b);
			} else {
				Enqueue(b);
			}
		}

		public bool Enqueue(BehaviorComponent b)
		{
			if (b.IndexInUpdateFamily < 0) {
				b.IndexInUpdateFamily = behaviors.Count;
				behaviors.Add(b);
				return true;
			}
			return false;
		}

		public bool Dequeue(BehaviorComponent b)
		{
			if (b.IndexInUpdateFamily >= 0) {
				behaviors[b.IndexInUpdateFamily] = null;
				b.IndexInUpdateFamily = -1;
				return true;
			}
			return false;
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
						b.IndexInUpdateFamily = i;
					}
					behaviors[i] = b;
					behaviors.RemoveAt(behaviors.Count - 1);
				}
			}
		}
	}
}
