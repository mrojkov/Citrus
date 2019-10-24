#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedFrame : Frame
	{
		public override bool IsNotDecorated() => false;

		public ThemedFrame()
		{
			CompoundPresenter.Add(new ThemedFramePresenter(Theme.Colors.GrayBackground, Theme.Colors.ControlBorder));
		}
	}

	public class ThemedFramePresenter : IPresenter
	{
		private readonly Color4 innerColor;
		private readonly Color4 borderColor;

		public ThemedFramePresenter(Color4 innerColor, Color4 borderColor)
		{
			this.innerColor = innerColor;
			this.borderColor = borderColor;
		}

		public Lime.RenderObject GetRenderObject(Node node)
		{
			var widget = (Widget)node;
			var ro = RenderObjectPool<RenderObject>.Acquire();
			ro.CaptureRenderState(widget);
			ro.Size = widget.Size;
			ro.InnerColor = widget.GloballyEnabled ? innerColor : Theme.Colors.DisabledBackground;
			ro.BorderColor = borderColor;
			return ro;
		}

		public bool PartialHitTest(Node node, ref HitTestArgs args) => false;

		private class RenderObject : WidgetRenderObject
		{
			public Vector2 Size;
			public Color4 InnerColor;
			public Color4 BorderColor;

			public override void Render()
			{
				PrepareRenderState();
				Renderer.DrawRect(Vector2.Zero, Size, InnerColor);
				Renderer.DrawRectOutline(Vector2.Zero, Size, BorderColor);
			}
		}
	}
}
#endif
