using System;
using System.Collections.Generic;

namespace Lime
{
	public class NodeManager
	{
		[Obsolete("A temporary solution to access global hierarchy changes. Will be removed later when work on Orange plugins will be finished.")]
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
			var componentsToRegister = new List<(NodeComponent, Node)>();
			var componentProcessor = processor as NodeComponentProcessor;
			if (componentProcessor != null) {
				foreach (var (type, componentProcessorsForType) in processorsByComponentType) {
					if (componentProcessor.TargetComponentType.IsAssignableFrom(type)) {
						componentProcessorsForType.Add(componentProcessor);
					}
				}
				foreach (var node in RootNodes) {
					GetComponentsForProcessor(node, componentProcessor, componentsToRegister);
				}
			}
			processor.Manager = this;
			processor.Start();
			foreach (var (component, owner) in componentsToRegister) {
				componentProcessor.InternalAdd(component, owner);
			}
		}

		private void GetComponentsForProcessor(Node node, NodeComponentProcessor processor, List<(NodeComponent, Node)> result)
		{
			foreach (var component in node.Components) {
				if (processor.TargetComponentType.IsAssignableFrom(component.GetType())) {
					result.Add((component, node));
				}
			}
			foreach (var child in node.Nodes) {
				GetComponentsForProcessor(node, processor, result);
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
			processor.Stop(this);
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
			var componentsToRegister = new List<(NodeComponent, Node)>();
			RegisterNodeHelper(node, componentsToRegister);
			foreach (var (component, owner) in componentsToRegister) {
				RegisterComponent(component, owner);
			}
			RaiseHierarchyChanged(new HierarchyChangedEventArgs(this, HierarchyAction.Link, node, parent));
		}

		private void RegisterNodeHelper(Node node, List<(NodeComponent, Node)> componentsToRegister)
		{
			node.Manager = this;
			if (node.GloballyFrozen) {
				frozenNodes.Add(node);
			}
			foreach (var component in node.Components) {
				componentsToRegister.Add((component, node));
			}
			foreach (var child in node.Nodes) {
				RegisterNodeHelper(child, componentsToRegister);
			}
		}

		internal void UnregisterNode(Node node, Node parent)
		{
			var componentsToUnregister = new List<(NodeComponent, Node)>();
			UnregisterNodeHelper(node, componentsToUnregister);
			foreach (var (component, owner) in componentsToUnregister) {
				UnregisterComponent(component, owner);
			}
			RaiseHierarchyChanged(new HierarchyChangedEventArgs(this, HierarchyAction.Unlink, node, parent));
		}

		private void UnregisterNodeHelper(Node node, List<(NodeComponent, Node)> componentsToUnregister)
		{
			node.Manager = null;
			frozenNodes.Remove(node);
			foreach (var child in node.Nodes) {
				UnregisterNodeHelper(child, componentsToUnregister);
			}
			foreach (var component in node.Components) {
				componentsToUnregister.Add((component, node));
			}
		}

		private void RaiseHierarchyChanged(HierarchyChangedEventArgs e)
		{
			HierarchyChanged?.Invoke(e);
			GlobalHierarchyChanged?.Invoke(e);
		}

		internal void FilterNode(Node node)
		{
			var frozen = node.GloballyFrozen;
			var frozenChanged = frozen ? frozenNodes.Add(node) : frozenNodes.Remove(node);
			if (frozenChanged) {
				foreach (var c in node.Components) {
					foreach (var p in GetProcessorsForComponentType(c.GetType())) {
						p.InternalOnOwnerFrozenChanged(c, node);
					}
				}
			}
			foreach (var n in node.Nodes) {
				FilterNode(n);
			}
		}

		internal void RegisterComponent(NodeComponent component, Node owner)
		{
			foreach (var p in GetProcessorsForComponentType(component.GetType())) {
				p.InternalAdd(component, owner);
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
