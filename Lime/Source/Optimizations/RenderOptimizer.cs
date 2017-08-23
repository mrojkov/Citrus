using System;
using System.Collections.Generic;

namespace Lime.RenderOptimizer
{
	public class RenderOptimizer
	{
		private static readonly List<Type> simpleWidgetTypes = new List<Type>() {
			typeof(Image),
			typeof(SimpleText),
			typeof(RichText),
		};

		private readonly Widget viewWidget;
		private readonly Stack<ImageCombinerItem> imageCombinerStack = new Stack<ImageCombinerItem>();
		private ViewRectProjector rectProjector;

		public RenderOptimizer(Widget viewWidget, bool processContentSize = false)
		{
			this.viewWidget = viewWidget;

			if (processContentSize) {
				ProcessContentSize(viewWidget);
			}
		}

		private static ContentSizeComponent ProcessContentSize(Node node)
		{
			var selfSize = node.Components.Get<ContentSizeComponent>();
			if (selfSize != null) {
				return selfSize;
			}
			if (node.RenderChainBuilder == null) {
				return ProcessEmptyNode(node);
			}

			var imageCombiner = node as ImageCombiner;
			if (imageCombiner != null) {
				return ProcessEmptyNode(node);
			}
			var distortionMesh = node as DistortionMesh;
			if (distortionMesh != null) {
				return ProcessDistortionMesh(distortionMesh);
			}
			var particleEmitter = node as ParticleEmitter;
			if (particleEmitter != null) {
				return ProcessParticleEmitter(particleEmitter);
			}
			var nodeType = node.GetType();
			for (var i = 0; i < simpleWidgetTypes.Count; i++) {
				if (nodeType == simpleWidgetTypes[i]) {
					return ProcessSimpleWidget((Widget)node);
				}
			}

			Dictionary<Node, ContentSizeComponent> childrenContent = null;
			if (node.Nodes.Count > 0) {
				var existsNonOptimizable = false;
				childrenContent = new Dictionary<Node, ContentSizeComponent>();
				for (var i = 0; i < node.Nodes.Count; i++) {
					var child = node.Nodes[i];
					var childContent = ProcessContentSize(child);
					if (childContent != null) {
						if (!childContent.IsEmpty) {
							childrenContent.Add(child, childContent);
						}
					} else {
						existsNonOptimizable = true;
					}
				}
				if (existsNonOptimizable) {
					return null;
				}
			}

			if (childrenContent == null || childrenContent.Count == 0) {
				if (node is Widget) {
					return ProcessSimpleWidget(node as Widget);
				} else {
					return node.RenderChainBuilder == null ? ProcessEmptyNode(node) : null;
				}
			}
			var viewport = node as Viewport3D;
			if (viewport != null) {
				return ProcessViewport(viewport, childrenContent);
			}
			var widget = node as Widget;
			if (widget != null) {
				return ProcessWidget(widget, childrenContent);
			}
			var widgetAdapter = node as WidgetAdapter3D;
			if (widgetAdapter != null) {
				return ProcessWidgetAdapter(widgetAdapter);
			}
			var node3D = node as Node3D;
			if (node3D != null) {
				return ProcessNode3D(node3D, childrenContent);
			}
			return null;
		}

		private static ContentSizeComponent ProcessEmptyNode(Node node)
		{
			var selfSize = node.Components.GetOrAdd<ContentSizeComponent>();
			selfSize.Size = null;
			return selfSize;
		}

		private static ContentSizeComponent ProcessSimpleWidget(Widget widget)
		{
			var selfSize = widget.Components.GetOrAdd<ContentSizeComponent>();
			selfSize.Size = new ContentRectangle();
			return selfSize;
		}

		private static ContentSizeComponent ProcessDistortionMesh(DistortionMesh distortionMesh)
		{
			if (distortionMesh.Nodes.Count == 0) {
				return ProcessEmptyNode(distortionMesh);
			}

			var aabb = distortionMesh.GetLocalAABB();
			var selfSize = distortionMesh.Components.GetOrAdd<ContentSizeComponent>();
			selfSize.Size = new ContentRectangle(aabb, Vector2.One / distortionMesh.Size);
			return selfSize;
		}

		private static ContentSizeComponent ProcessWidget(Widget widget, Dictionary<Node, ContentSizeComponent> children)
		{
			var animationDuration = 0;
			foreach (var child in children) {
				var childWidget = (Widget)child.Key;
				foreach (var animator in childWidget.Animators) {
					animationDuration = Math.Max(animationDuration, animator.Duration);
				}
			}

			var animationFrame = widget.DefaultAnimation.Frame;
			var projector = new ViewRectProjector(widget, Rectangle.Empty);
			var aabb = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
			var isEmpty = true;
			for (var frame = 0; frame <= animationDuration; frame++) {
				widget.DefaultAnimation.Frame = frame;
				foreach (var child in children) {
					var childWidget = (Widget)child.Key;
					if (!childWidget.Visible || childWidget.Color.A == 0) {
						continue;
					}
					isEmpty = false;

					var size = child.Value.Size.ForProjection(child.Key);
					size = projector.Project(child.Key, size);
					var childAabb = ((ContentRectangle)size).Data;
					aabb = Rectangle.Bounds(aabb, childAabb);
				}
			}
			widget.DefaultAnimation.Frame = animationFrame;
			if (isEmpty) {
				return ProcessEmptyNode(widget);
			}

			var selfSize = widget.Components.GetOrAdd<ContentSizeComponent>();
			selfSize.Size = new ContentRectangle(aabb, Vector2.One / widget.Size);
			return selfSize;
		}

		private static ContentSizeComponent ProcessParticleEmitter(ParticleEmitter emitter)
		{
			if (emitter.ParticlesLinkage != ParticlesLinkage.Parent) {
				return null;
			}

			const float EmulationDuration = 15f;
			const float EmulationStep = (float)AnimationUtils.SecondsPerFrame;
			var parent = emitter.ParentWidget;
			var modifiers = new List<ParticleModifier>();
			for (var i = 0; i < emitter.Nodes.Count; i++) {
				var modifier = emitter.Nodes[i] as ParticleModifier;
				if (modifier != null) {
					modifiers.Add(modifier);
				}
			}
			var corners = new[] {
				Vector2.Zero,
				Vector2.Right,
				Vector2.One,
				Vector2.Down
			};
			// Particles pivot == (0.5, 0.5)
			for (var i = 0; i < corners.Length; i++) {
				corners[i] = corners[i] - Vector2.Half;
			}
			var aabb = new Rectangle(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
			var isEmpty = true;
			var animatedFrames = new SortedSet<int>();
			foreach (var animator in emitter.Animators) {
				for (var i = 0; i < animator.ReadonlyKeys.Count; i++) {
					animatedFrames.Add(animator.ReadonlyKeys[i].Frame);
				}
			}
			if (animatedFrames.Count == 0) {
				animatedFrames.Add(parent.DefaultAnimation.Frame);
			}

			var grandParent = parent.Parent;
			var parentIndex = grandParent?.Nodes.IndexOf(parent) ?? 0;
			parent.Unlink();
			var animationFrame = parent.DefaultAnimation.Frame;
			var parentVisibility = parent.Visible;
			parent.Visible = true;

			foreach (var frame in animatedFrames) {
				parent.DefaultAnimation.Frame = frame;
				var parentToLocalTransform = emitter.CalcLocalToParentTransform().CalcInversed();

				for (var time = 0f; time < EmulationDuration; time += EmulationStep) {
					foreach (var particle in emitter.particles) {
						if (particle.ColorCurrent.A <= 0) {
							continue;
						}

						isEmpty = false;
						var angle = particle.Angle;
						if (emitter.AlongPathOrientation) {
							angle += particle.FullDirection;
						}
						var m = modifiers[particle.ModifierIndex];
						var particleSize = particle.ScaleCurrent * m.Size;
						var orientation = Vector2.CosSinRough(angle * Mathf.DegToRad);
						var perpendicularOrientation = new Vector2(-orientation.Y, orientation.X);
						var transform = new Matrix32 {
							U = particleSize.X * orientation,
							V = particleSize.Y * perpendicularOrientation,
							T = particle.FullPosition
						};
						transform *= parentToLocalTransform;
						for (var i = 0; i < corners.Length; i++) {
							var corner = transform.TransformVector(corners[i]);
							aabb = aabb.IncludingPoint(corner);
						}
					}
					emitter.Update(EmulationStep);
				}
				emitter.DeleteAllParticles();
			}

			grandParent?.Nodes.Insert(parentIndex, parent);
			parent.DefaultAnimation.Frame = animationFrame;
			parent.Visible = parentVisibility;

			if (isEmpty) {
				return ProcessEmptyNode(emitter);
			}

			var safeHV = aabb.Size * 0.1f;
			aabb = aabb.ExpandedBy(new Thickness(safeHV.X, safeHV.Y));

			var selfSize = emitter.Components.GetOrAdd<ContentSizeComponent>();
			selfSize.Size = new ContentRectangle(aabb, Vector2.One / emitter.Size);
			return selfSize;
		}

		private static ContentSizeComponent ProcessWidgetAdapter(WidgetAdapter3D widgetAdapter)
		{
			var contentSize = widgetAdapter.Widget.Components.Get<ContentSizeComponent>();
			if (contentSize == null) {
				return null;
			}
			if (contentSize.IsEmpty) {
				return ProcessEmptyNode(widgetAdapter);
			}

			var size = (ContentRectangle)contentSize.Size;
			var selfSize = widgetAdapter.Components.GetOrAdd<ContentSizeComponent>();
			selfSize.Size = new WidgetAdapter3DProjector(widgetAdapter).Project(widgetAdapter.Widget, size);
			return selfSize;
		}

		private static ContentSizeComponent ProcessNode3D(Node3D node3D, Dictionary<Node, ContentSizeComponent> children)
		{
			// TODO: Content size for 3D nodes
			return null;
		}

		private static ContentSizeComponent ProcessViewport(Viewport3D viewport, Dictionary<Node, ContentSizeComponent> children)
		{
			// TODO: Content size for viewports
			return null;
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
			node.RenderChainBuilder = DefaultRenderChainBuilder.Instance;
		}

		private static bool IsInsideRenderChain(Node node)
		{
			return node.RenderChainBuilder == DefaultRenderChainBuilder.Instance;
		}
	}
}
