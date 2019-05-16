using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lime
{
	public class NodeManager
	{
		private List<Node> nodes = new List<Node>();
		private BehaviourSystem behaviourSystem = new BehaviourSystem();

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
				case BehaviourComponent behaviour:
					behaviourSystem.Add(behaviour);
					break;
			}
		}

		internal void UnregisterComponent(NodeComponent component)
		{
			switch (component) {
				case BehaviourComponent behaviour:
					behaviourSystem.Remove(behaviour);
					break;
			}
		}

		public void Update(float delta)
		{
			behaviourSystem.Update(delta);
		}
	}

	public class BehaviourComponent : NodeComponent
	{
		internal BehaviourFamily Family;
		internal int IndexInFamily = -1;

		protected internal virtual void Start() { }

		protected internal virtual void Stop() { }

		protected internal virtual void Update(float delta) { }
	}

	internal class BehaviourSystem
	{
		private List<List<int>> behaviourFamilyGraph = new List<List<int>>();
		private List<BehaviourFamily> behaviourFamilies = new List<BehaviourFamily>();
		private Dictionary<Type, int> behaviourFamilyIndexMap = new Dictionary<Type, int>();
		private HashSet<BehaviourComponent> pendingBehaviours = new HashSet<BehaviourComponent>();
		private HashSet<BehaviourComponent> pendingBehaviours2 = new HashSet<BehaviourComponent>();
		private List<BehaviourFamily> behaviourFamilyQueue = new List<BehaviourFamily>();
		private bool behaviourFamilyQueueDirty = false;

		public void Update(float delta)
		{
			while (pendingBehaviours.Count > 0) {
				var t = pendingBehaviours;
				pendingBehaviours = pendingBehaviours2;
				pendingBehaviours2 = t;
				foreach (var b in pendingBehaviours2) {
					var bf = behaviourFamilies[GetBehaviourFamilyIndex(b.GetType())];
					b.Family = bf;
					b.IndexInFamily = bf.Behaviours.Count;
					bf.Behaviours.Add(b);
					b.Start();
				}
				pendingBehaviours2.Clear();
			}
			if (behaviourFamilyQueueDirty) {
				BuildBehaviourFamilyQueue();
				behaviourFamilyQueueDirty = false;
			}
			foreach (var bf in behaviourFamilyQueue) {
				ProcessRemovals(bf);
				for (var i = 0; i < bf.Behaviours.Count; i++) {
					bf.Behaviours[i]?.Update(delta);
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
			behaviourFamilyQueue.Clear();
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
				behaviourFamilyQueue.Add(behaviourFamilies[nodeIndex]);
			}
		}

		public void Add(BehaviourComponent behaviour)
		{
			pendingBehaviours.Add(behaviour);
		}

		public void Remove(BehaviourComponent behaviour)
		{
			if (!pendingBehaviours.Remove(behaviour)) {
				var family = behaviour.Family;
				family.Behaviours[behaviour.IndexInFamily] = null;
				family.Removals.Add(behaviour.IndexInFamily);
				behaviour.Family = null;
				behaviour.IndexInFamily = -1;
				behaviour.Stop();
			}
		}

		private void ProcessRemovals(BehaviourFamily family)
		{
			//foreach (var i in family.Removals) {
			//	var lastIndex = family.Behaviours.Count - 1;
			//	while (lastIndex >= 0 && family.Behaviours[lastIndex] == null) {
			//		family.Behaviours.RemoveAt(lastIndex);
			//		lastIndex--;
			//	}
			//	if (lastIndex < 0) {
			//		break;
			//	}
			//	if (i > lastIndex) {
			//		continue;
			//	}
			//	family.Behaviours[i] = family.Behaviours[lastIndex];
			//	family.Behaviours[i].IndexInFamily = i;
			//	family.Behaviours.RemoveAt(lastIndex);
			//}
		}

		private int GetBehaviourFamilyIndex(Type behaviourType)
		{
			if (behaviourFamilyIndexMap.TryGetValue(behaviourType, out var index)) {
				return index;
			}
			index = behaviourFamilies.Count;
			behaviourFamilies.Add(new BehaviourFamily(behaviourType));
			behaviourFamilyGraph.Add(new List<int>());
			behaviourFamilyIndexMap.Add(behaviourType, index);
			foreach (var i in behaviourType.GetCustomAttributes<UpdateAfterBehaviourAttribute>(true)) {
				behaviourFamilyGraph[index].Add(GetBehaviourFamilyIndex(i.BehaviourType));
			}
			foreach (var i in behaviourType.GetCustomAttributes<UpdateBeforeBehaviourAttribute>(true)) {
				behaviourFamilyGraph[GetBehaviourFamilyIndex(i.BehaviourType)].Add(index);
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

	internal class BehaviourFamily
	{
		public readonly Type BehaviourType;
		public readonly List<BehaviourComponent> Behaviours = new List<BehaviourComponent>();
		public readonly List<int> Removals = new List<int>();

		public BehaviourFamily(Type behaviourType)
		{
			BehaviourType = behaviourType;
		}
	}
}
