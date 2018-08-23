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

	internal class ThemedFramePresenter : CustomPresenter
	{
		private readonly Color4 innerColor;
		private readonly Color4 borderColor;

		public ThemedFramePresenter(Color4 innerColor, Color4 borderColor)
		{
			this.innerColor = innerColor;
			this.borderColor = borderColor;
		}

		public override void Render(Node node)
		{
			var widget = node.AsWidget;
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, innerColor);
			Renderer.DrawRectOutline(Vector2.Zero, widget.Size, borderColor);
		}
	}
}
#endif
