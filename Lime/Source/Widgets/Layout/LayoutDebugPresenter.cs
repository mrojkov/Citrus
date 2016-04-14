using System;

namespace Lime
{
	public class LayoutDebugPresenter : CustomPresenter
	{
		public Color4 color = Color4.Red;
		
		public LayoutDebugPresenter(IPresenter previous) : base(previous) { }

		public override void Render(Node node)
		{
			base.Render(node);
			var widget = node.AsWidget;
			if (widget == null || widget.Layout == null)
				return;
			Renderer.Blending = Blending.Alpha;
			Renderer.Shader = ShaderId.Diffuse;
			Renderer.Transform1 = widget.LocalToWorldTransform;
			foreach (var r in widget.Layout.DebugRectangles) {
				Renderer.DrawRectOutline(r.A, r.B, color);
			}
		}
	}
}

