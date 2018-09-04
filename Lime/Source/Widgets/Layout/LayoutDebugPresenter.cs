using System;
using System.Collections.Generic;

namespace Lime
{
	public class LayoutDebugPresenter : IPresenter
	{
		public readonly Color4 Color;
		public readonly float Thickness;

		public LayoutDebugPresenter(Color4 color, float thickness = 0)
		{
			Color = color;
			Thickness = thickness;
		}

		public Lime.RenderObject GetRenderObject(Node node)
		{
			var widget = node as Widget;
			if (widget == null || widget.Layout == null) {
				return null;
			}
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(widget);
			ro.Color = Color;
			ro.Thickness = Thickness > 0 ? Thickness : 1 / CommonWindow.Current.PixelScale;
			foreach (var r in widget.Layout.DebugRectangles) {
				ro.DebugRectangles.Add(r);
			}
			return ro;
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

		public IPresenter Clone() => (IPresenter)MemberwiseClone();

		private class RenderObject : WidgetRenderObject
		{
			public float Thickness;
			public Color4 Color;
			public List<Rectangle> DebugRectangles = new List<Rectangle>();

			public override void Render()
			{
				PrepareRenderState();
				foreach (var r in DebugRectangles) {
					Renderer.DrawRectOutline(r.A, r.B, Color, Thickness);
				}
			}

			protected override void OnRelease()
			{
				DebugRectangles.Clear();
			}
		}
	}
}

