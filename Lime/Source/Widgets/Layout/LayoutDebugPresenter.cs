using System;

namespace Lime
{
	public class LayoutDebugPresenter : CustomPresenter
	{
		public readonly Color4 Color;
		public readonly float Thickness;

		public LayoutDebugPresenter(Color4 color, float thickness = 0)
		{
			Color = color;
			Thickness = thickness;
		}

		public override void Render(Node node)
		{
			var widget = node.AsWidget;
			if (widget == null || widget.Layout == null)
				return;
			widget.PrepareRendererState();
			var t = Thickness > 0 ? Thickness : 1 / CommonWindow.Current.PixelScale;
			foreach (var r in widget.Layout.DebugRectangles) {
				Renderer.DrawRectOutline(r.A, r.B, Color, t);
			}
		}
	}
}

