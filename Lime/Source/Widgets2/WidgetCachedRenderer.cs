using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime.Widgets2
{
	class WidgetCachedRenderer : Node
	{
		private Widget widget;
		private RenderList renderList;
		private Matrix32 inversedWorldTransform;
		private static RenderChain renderChain = new RenderChain();

		public WidgetCachedRenderer(Widget widget)
		{
			this.widget = widget;
			renderList = new RenderList();
		}

		public void Invalidate()
		{
			renderList.Clear();
		}

		public bool Prepare()
		{
			if (Renderer.CurrentRenderList != Renderer.MainRenderList) {
				return false;
			}
			if (!renderList.Empty) {
				return true;
			}
			inversedWorldTransform = widget.LocalToWorldTransform.CalcInversed();
			Renderer.CurrentRenderList = renderList;
			widget.AddToRenderChain(renderChain);
			for (var node = widget.Nodes.FirstOrNull(); node != null; node = node.NextSibling) {
				node.AddToRenderChain(renderChain);
			}
			renderChain.RenderAndClear();
			Renderer.CurrentRenderList = Renderer.MainRenderList;
			return true;
		}

		public override void Render()
		{
			if (renderList.Empty) {
				return;
			}
			Renderer.Flush();
			Renderer.PushProjectionMatrix();
			Renderer.Projection = (Matrix44)(inversedWorldTransform * widget.LocalToWorldTransform) * Renderer.Projection;
			renderList.Render();
			Renderer.PopProjectionMatrix();
		}

		public override void Dispose()
		{
			renderList.Clear();
		}
	}
}
