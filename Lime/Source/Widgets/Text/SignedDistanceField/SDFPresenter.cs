using System;

namespace Lime
{
	class SDFPresenter : IPresenter
	{
		private readonly SDFRenderAction[] renderActions;
		private readonly RenderChain renderChain = new RenderChain();

		private IMaterial material;
		private Blending blending;
		private ShaderId shader;
		private bool opaque;
		private SDFRenderAction.Buffer sourceTextureBuffer;
		private SDFRenderActionMain.Buffer SDFBuffer;
		private SDFRenderActionOutline.Buffer OutlineBuffer;

		public SDFPresenter()
		{
			renderActions = new SDFRenderAction[] {
				new SDFRenderActionTextureBuilder(),
				new SDFRenderActionMain(),
				new SDFRenderActionOutline(),
				new SDFRenderActionTextureRender(),
			};
		}

		public RenderObject GetRenderObject(Node node)
		{
			var component = node.Components.Get<SignedDistanceFieldComponent>();
			if (component == null) {
				throw new InvalidOperationException();
			}

			var ro = RenderObjectPool<SDFRenderObject>.Acquire();
			try {
				component.GetOwnerRenderObjects(renderChain, ro.Objects);
			} finally {
				renderChain.Clear();
			}

			sourceTextureBuffer?.MarkAsDirty();

			var widget = (Widget)node;
			var bufferSize = new Size((int)widget.Width, (int)widget.Height);
			var sourceTextureScaling = 1f;
			if (sourceTextureBuffer?.Size != bufferSize) {
				sourceTextureBuffer = new SDFRenderAction.Buffer(bufferSize);
			}
			if (SDFBuffer?.Size != bufferSize) {
				SDFBuffer = new SDFRenderActionMain.Buffer(bufferSize);
			}
			if (OutlineBuffer?.Size != bufferSize) {
				OutlineBuffer = new SDFRenderActionOutline.Buffer(bufferSize);
			}
			ro.RenderActions = renderActions;
			ro.Material = GetMaterial(widget, component);
			ro.LocalToWorldTransform = widget.LocalToWorldTransform;
			ro.Position = widget.ContentPosition;
			ro.Size = widget.ContentSize;
			ro.Color = widget.GlobalColor;
			ro.UV0 = Vector2.Zero;
			ro.UV1 = Vector2.One;
			ro.SourceTextureBuffer = sourceTextureBuffer;
			ro.SourceTextureScaling = sourceTextureScaling;
			ro.SDFBuffer = SDFBuffer;
			ro.SDFMaterial = component.SDFMaterial;
			ro.Softness = component.Softness;
			ro.Dilate = component.Dilate;
			ro.FaceColor = component.FaceColor;
			ro.OutlineMaterial = component.OutlineMaterial;
			ro.OutlineBuffer = OutlineBuffer;
			ro.OutlineColor = component.OutlineColor;
			ro.Thickness = component.Thickness;
			ro.OutlineSoftness = component.OutlineSoftness;
			ro.OutlineEnabled = component.OutlineEnabled;
			
			return ro;
		}

		private IMaterial GetMaterial(Widget widget, SignedDistanceFieldComponent component)
		{
			blending = widget.GlobalBlending;
			shader = widget.GlobalShader;
			opaque = false;
			var isOpaqueRendering = opaque && blending == Blending.Inherited;
			return material = WidgetMaterial.GetInstance(!isOpaqueRendering ? blending : Blending.Opaque, shader, 1);
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args)
		{
			var widget = (Widget)node;
			if (!widget.BoundingRectHitTest(args.Point)) {
				return false;
			}
			var savedLayer = renderChain.CurrentLayer;
			try {
				renderChain.CurrentLayer = widget.Layer;
				for (var child = widget.FirstChild; child != null; child = child.NextSibling) {
					child.RenderChainBuilder?.AddToRenderChain(renderChain);
				}
				return renderChain.HitTest(ref args) || SelfPartialHitTest(widget, ref args);
			} finally {
				renderChain.Clear();
				renderChain.CurrentLayer = savedLayer;
			}
		}

		public bool SelfPartialHitTest(Widget widget, ref HitTestArgs args)
		{
			Node targetNode;
			for (targetNode = widget; targetNode != null; targetNode = targetNode.Parent) {
				var method = targetNode.AsWidget?.HitTestMethod ?? HitTestMethod.Contents;
				if (method == HitTestMethod.Skip || targetNode != widget && method == HitTestMethod.BoundingRect) {
					return false;
				}
				if (targetNode.HitTestTarget) {
					break;
				}
			}
			if (targetNode == null) {
				return false;
			}
			if (
				widget.HitTestMethod == HitTestMethod.BoundingRect && widget.BoundingRectHitTest(args.Point) ||
				widget.HitTestMethod == HitTestMethod.Contents && widget.PartialHitTestByContents(ref args)
			) {
				args.Node = targetNode;
				return true;
			}
			return false;
		}

		public IPresenter Clone() => new SDFPresenter();
	}
}
