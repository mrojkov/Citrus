using System;
using System.Collections.Generic;

namespace Lime
{
	public class NodeManager
	{
		[Obsolete("For extreme cases")]
		public static event HierarchyChangedEventHandler GlobalHierarchyChanged;

		private Dictionary<Type, List<NodeComponentProcessor>> processorsByComponentType = new Dictionary<Type, List<NodeComponentProcessor>>();
		private HashSet<Node> frozenNodes = new HashSet<Node>(ReferenceEqualityComparer.Instance);

		public event HierarchyChangedEventHandler HierarchyChanged;

		public NodeManagerRootNodeCollection RootNodes { get; }

		public NodeManagerProcessorCollection Processors { get; }

		public IServiceProvider ServiceProvider { get; }

		public NodeProcessor ActiveProcessor { get; private set; }

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
					RegisterHierarchyComponentsWithinProcessor(node, componentProcessor);
				}
			}
		}

		private void RegisterHierarchyComponentsWithinProcessor(Node node, NodeComponentProcessor processor)
		{
			foreach (var component in node.Components) {
				if (processor.TargetComponentType.IsAssignableFrom(component.GetType())) {
					processor.InternalAdd(component);
				}
			}
			foreach (var child in node.Nodes) {
				RegisterHierarchyComponentsWithinProcessor(child, processor);
			}
		}

		internal void UnregisterNodeProcessor(NodeProcessor processor)
		{
			processor.Manager = null;
			if (processor is NodeComponentProcessor componentProcessor) {
				foreach (var (componentType, componentProcessors) in processorsByComponentType) {
					if (componentProcessor.TargetComponentType.IsAssignableFrom(componentType)) {
						componentProcessors.Remove(componentProcessor);
					}
				}
				foreach (var rootNode in RootNodes) {
					UnregisterHierarchyComponentsWithinProcessor(rootNode, componentProcessor);
			}
			}
			processor.Stop();
		}

		private void UnregisterHierarchyComponentsWithinProcessor(Node node, NodeComponentProcessor processor)
		{
			foreach (var child in node.Nodes) {
				UnregisterHierarchyComponentsWithinProcessor(child, processor);
			}
			foreach (var component in node.Components) {
				if (processor.TargetComponentType.IsAssignableFrom(component.GetType())) {
					processor.InternalRemove(component, node);
				}
			}
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

		internal void RegisterNode(Node node, Node parent)
		{
			RegisterNodeHelper(node);
			var hierarchyChangedEventArgs = new HierarchyChangedEventArgs(
				this,
				HierarchyAction.Link,
				node,
				parent
			);
			HierarchyChanged?.Invoke(hierarchyChangedEventArgs);
			GlobalHierarchyChanged?.Invoke(hierarchyChangedEventArgs);
		}

		private void RegisterNodeHelper(Node node)
		{
			node.Manager = this;
			if (node.GloballyFrozen) {
				frozenNodes.Add(node);
			}
			foreach (var component in node.Components) {
				RegisterComponent(component);
			}
			foreach (var child in node.Nodes) {
				RegisterNodeHelper(child);
			}
		}

		internal void UnregisterNode(Node node, Node parent)
		{
			UnregisterNodeHelper(node);
			var hierarchyChangedEventArgs = new HierarchyChangedEventArgs(
				this,
				HierarchyAction.Unlink,
				node,
				parent
			);
			HierarchyChanged?.Invoke(hierarchyChangedEventArgs);
			GlobalHierarchyChanged?.Invoke(hierarchyChangedEventArgs);
		}

		private void UnregisterNodeHelper(Node node)
		{
			node.Manager = null;
			frozenNodes.Remove(node);
			foreach (var child in node.Nodes) {
				UnregisterNodeHelper(child);
			}
			foreach (var component in node.Components) {
				UnregisterComponent(component, node);
			}
		}

		internal void FilterNode(Node node)
		{
			var frozen = node.GloballyFrozen;
			var frozenChanged = frozen ? frozenNodes.Add(node) : frozenNodes.Remove(node);
			if (frozenChanged) {
				foreach (var c in node.Components) {
					foreach (var p in GetProcessorsForComponentType(c.GetType())) {
						p.InternalOnOwnerFrozenChanged(c);
					}
				}
			}
			foreach (var n in node.Nodes) {
				FilterNode(n);
			}
		}

		internal void RegisterComponent(NodeComponent component)
		{
			foreach (var p in GetProcessorsForComponentType(component.GetType())) {
				p.InternalAdd(component);
			}
		}

		internal void UnregisterComponent(NodeComponent component, Node owner)
		{
			foreach (var p in GetProcessorsForComponentType(component.GetType())) {
				p.InternalRemove(component, owner);
			}
		}

		public void Update(float delta)
		{
			foreach (var p in Processors) {
				ActiveProcessor = p;
				p.Update(delta);
			}
			ActiveProcessor = null;
		}
	}

	public delegate void HierarchyChangedEventHandler(HierarchyChangedEventArgs e);

	public struct HierarchyChangedEventArgs
	{
		public readonly NodeManager Manager;
		public readonly HierarchyAction Action;
		public readonly Node Child;

		public readonly Node Parent;

		public HierarchyChangedEventArgs(NodeManager manager, HierarchyAction action, Node child, Node parent)
		{
			Manager = manager;
			Action = action;
			Child = child;
			Parent = parent;
		}
	}

	public enum HierarchyAction
	{
		Link,
		Unlink
	}
}
