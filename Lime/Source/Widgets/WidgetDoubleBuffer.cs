using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lime
{
	public class WidgetDoubleBuffer
	{
		private Widget widget;
		private RenderChain renderChain = new RenderChain();
		public RenderTexture Texture { get; private set; }

		public bool CheckOnlyWidgetContents = true;

		public WidgetDoubleBuffer(Widget widget)
		{
			this.widget = widget;
		}

		public void Render()
		{
			Size widgetSize = new Size(widget.Width.Ceiling(), widget.Height.Ceiling());
			if (Texture != null && Texture.ImageSize != widgetSize) {
				Texture.Dispose();
			}
			if (Texture == null) {
				Texture = new RenderTexture(widgetSize.Width, widgetSize.Height, RenderTextureFormat.RGBA8);
			}
			if (CheckOnlyWidgetContents) {
				widget.DoubleBufferValid = true;
			}
			if (!CheckAndRestoreDoubleBufferValidFlag(widget)) {
				widget.RenderToTexture(Texture, renderChain);
			}
			Renderer.Blending = widget.GlobalBlending;
			Renderer.Transform1 = widget.LocalToWorldTransform;
			Renderer.DrawSprite(Texture, widget.GlobalColor, Vector2.Zero, widget.Size, Vector2.Zero, Vector2.One);
		}

		private static bool CheckAndRestoreDoubleBufferValidFlag(Node node)
		{
			bool r = node.DoubleBufferValid;
			for (var n = node.Nodes.FirstOrDefault(); n != null; n = n.NextSibling) {
				if (!CheckAndRestoreDoubleBufferValidFlag(n)) {
					r = false;
				}
			}
			node.DoubleBufferValid = true;
			return r;
		}
	}
}
