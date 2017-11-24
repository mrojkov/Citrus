using System;
using System.Collections.Generic;

namespace Lime.RenderOptimizer
{
	public class RenderOptimizer
	{
		private readonly Widget viewWidget;
		private readonly Stack<ImageCombinerItem> imageCombinerStack = new Stack<ImageCombinerItem>();
		private ViewRectProjector rectProjector;

		public RenderOptimizer(Widget viewWidget)
		{
			this.viewWidget = viewWidget;
		}

		public void Update(Rectangle area)
		{
			rectProjector = new ViewRectProjector(viewWidget, area);
			var viewProjectors = new List<ViewProjector>();
			Process(viewWidget, viewProjectors);
			imageCombinerStack.Clear();
		}

		private void Process(Node node, IReadOnlyList<ViewProjector> viewProjectors)
		{
			var imageCombiner = node as ImageCombiner;
			if (imageCombiner != null) {
				var imageCombinerItem = ImageCombinerItem.TryCreate(imageCombiner);
				if (!imageCombiner.Enabled || imageCombinerItem == null) {
					OutsideRenderChain(node);
				} else {
					imageCombinerStack.Push(imageCombinerItem);
				}
				return;
			}

			if (imageCombinerStack.Count > 0) {
				var imageCombinerArg = node as IImageCombinerArg;
				if (imageCombinerArg != null) {
					var imageCombinerItem = imageCombinerStack.Peek();
					if (imageCombinerArg == imageCombinerItem.Arg1) {
						return;
					}
					if (imageCombinerArg == imageCombinerItem.Arg2) {
						imageCombinerStack.Pop();
						Process(node, viewProjectors);
						if (IsInsideRenderChain(node)) {
							InsideRenderChain(imageCombinerItem.Combiner);
							InsideRenderChain(imageCombinerItem.Arg1 as Node);
						} else {
							OutsideRenderChain(imageCombinerItem.Combiner);
							OutsideRenderChain(imageCombinerItem.Arg1 as Node);
						}
						return;
					}
				}
			}

			var selfSize = node.Components.Get<ContentSizeComponent>();
			if (selfSize != null && selfSize.IsEmpty) {
				OutsideRenderChain(node);
				return;
			}

			var widget = node as Widget;
			if (widget != null && (!widget.Visible || widget.Color.A == 0)) {
				OutsideRenderChain(node);
				return;
			}
			var node3D = node as Node3D;
			if (node3D != null && !node3D.Visible) {
				OutsideRenderChain(node);
				return;
			}

			if (selfSize != null) {
				var size = selfSize.Size.ForProjection(node);
				if (viewProjectors.Count == 0) {
					size = rectProjector.Project(node, size);
				} else {
					var projectionNode = node;
					for (var i = viewProjectors.Count - 1; i >= 0; i--) {
						var viewProjector = viewProjectors[i];
						size = viewProjector.Project(projectionNode, size);
						projectionNode = viewProjector.ProjectorNode;
					}
				}
				if (!rectProjector.IsOnView(size)) {
					OutsideRenderChain(node);
					return;
				}
			}

			var viewport = node as Viewport3D;
			if (viewport != null) {
				var childProjectors = new List<ViewProjector>();
				childProjectors.AddRange(viewProjectors);
				childProjectors.Add(new Viewport3DProjector(viewport));
				viewProjectors = childProjectors;
			}
			var widgetAdapter = node as WidgetAdapter3D;
			if (widgetAdapter != null) {
				var childProjectors = new List<ViewProjector>();
				childProjectors.AddRange(viewProjectors);
				childProjectors.Add(new WidgetAdapter3DProjector(widgetAdapter));
				viewProjectors = childProjectors;
			}

			InsideRenderChain(node);
			if (node is ParticleEmitter) {
				return;
			}
			for (var i = 0; i < node.Nodes.Count; i++) {
				Process(node.Nodes[i], viewProjectors);
			}
		}

		private static void OutsideRenderChain(Node node)
		{
			node.RenderChainBuilder = null;
		}

		private static void InsideRenderChain(Node node)
		{
			node.RenderChainBuilder = node;
		}

		private static bool IsInsideRenderChain(Node node)
		{
			return node.RenderChainBuilder == node;
		}
	}
}
