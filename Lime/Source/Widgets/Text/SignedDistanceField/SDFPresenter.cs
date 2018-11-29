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

		public SDFPresenter()
		{
			renderActions = new SDFRenderAction[] {
				new SDFRenderActionUnderlay(),
				new SDFRenderActionMain(),
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
				foreach (var item in ro.Objects) {
					var simpleTextRO = item as SimpleText.RenderObject;
					if (simpleTextRO != null) {
						ro.FaceColor = simpleTextRO.Color;
						ro.SpriteList = simpleTextRO.SpriteList;
					} else {
						var richTextRO = item as RichText.RenderObject;
						ro.FaceColor = richTextRO.Color;
						ro.SpriteList = richTextRO.SpriteList;
					}
				}
			} finally {
				renderChain.Clear();
			}

			var widget = (Widget)node;
			ro.RenderActions = renderActions;
			ro.Material = GetMaterial(widget, component);
			ro.LocalToWorldTransform = widget.LocalToWorldTransform;
			ro.Position = widget.ContentPosition;
			ro.Size = widget.ContentSize;
			ro.Color = widget.GlobalColor;
			ro.UV0 = Vector2.Zero;
			ro.UV1 = Vector2.One;
			ro.SDFMaterialProvider = component.SDFMaterialProvider;
			ro.Softness = component.Softness;
			ro.Dilate = component.Dilate;
			ro.OutlineColor = component.OutlineColor;
			ro.Thickness = component.Thickness;
			ro.UnderlayMaterialProvider = component.UnderlayMaterialProvider;
			ro.UnderlayColor = component.UnderlayColor;
			ro.UnderlayDilate = component.UnderlayDilate;
			ro.UnderlaySoftness = component.UnderlaySoftness;
			ro.UnderlayOffset = component.UnderlayOffset;
			ro.UnderlayEnabled = component.UnderlayEnabled;

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
