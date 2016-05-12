using System;

namespace Lime
{
	public class LayoutDebugPresenter : CustomPresenter
	{
		public Color4 Color = Color4.Red;
		public float Thickness;
		
		public LayoutDebugPresenter(IPresenter previous) : base(previous) { }

		public override void Render(Node node)
		{
			base.Render(node);
			var widget = node.AsWidget;
			if (widget == null || widget.Layout == null)
				return;
			widget.PrepareRendererState();
			var t = Thickness > 0 ? Thickness : 1 / Window.Current.PixelScale;
			foreach (var r in widget.Layout.DebugRectangles) {
				Renderer.DrawRectOutline(r.A, r.B, Color, t);
			}
		}
	}
}

