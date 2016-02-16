using System;

namespace Lime
{
	public class LayoutDebugPresenter : IPresenter
	{
		private IPresenter oldPresenter;
		private Node node;
		public Color4 color;
		
		public LayoutDebugPresenter(Node node)
			: this(node, Color4.Red)
		{
		}
		
		public LayoutDebugPresenter(Node node, Color4 color)
		{
			this.node = node;
			this.color = color;
			oldPresenter = node.Presenter;
		}
		
		public IPresenter Clone(Node newNode)
		{
			return new LayoutDebugPresenter(node, color);
		}
		
		public void Render()
		{
			if (oldPresenter != null) {
				oldPresenter.Render();
			}
			var w = node.AsWidget;
			if (w == null || w.Layout == null)
				return;
			Renderer.Blending = Blending.Alpha;
			Renderer.Shader = ShaderId.Diffuse;
			Renderer.Transform1 = w.LocalToWorldTransform;
			foreach (var r in w.Layout.DebugRectangles) {
				Renderer.DrawRectOutline(r.A, r.B, color);
			}
		}
	}
}

