using System;

namespace Lime
{
	public class LayoutDebugPresenter : IPresenter
	{
		private IPresenter oldPresenter;
		private Node node;
		public Color4 color;
		
		public LayoutDebugPresenter() : this(Color4.Red)
		{
		}
		
		public LayoutDebugPresenter(Color4 color)
		{
			this.color = color;
		}

		void IPresenter.OnAssign(Node node)
		{
			this.node = node;
			oldPresenter = node.Presenter;
		}

		IPresenter IPresenter.Clone(Node node)
		{
			return new LayoutDebugPresenter(color);
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

