#if !ANDROID && !iOS
using System;

namespace Lime
{
	[YuzuDontGenerateDeserializer]
	public class ThemedSlider : Slider
	{
		public override bool IsNotDecorated() => false;

		public ThemedSlider()
		{
			var rail = new Spline { Id = "Rail" };
			rail.AddNode(new SplinePoint { Position = new Vector2(0, 0.5f) });
			rail.AddNode(new SplinePoint { Position = new Vector2(1, 0.5f) });
			AddNode(rail);
			rail.ExpandToContainerWithAnchors();
			var thumb = new Widget {
				Id = "Thumb",
				Size = new Vector2(8, 16),
				Pivot = Vector2.Half,
			};
			AddNode(thumb);
			MinSize = new Vector2(30, 16);
			thumb.CompoundPresenter.Add(new SliderThumbPresenter());
			CompoundPresenter.Add(new SliderPresenter());
		}

		class SliderThumbPresenter : CustomPresenter
		{
			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				var p = new Vector2(0, 2);
				Renderer.DrawVerticalGradientRect(-p, p + widget.Size, Theme.Colors.ButtonDefault);
				Renderer.DrawRectOutline(-p, p + widget.Size, Theme.Colors.ControlBorder);
			}
		}

		class SliderPresenter : CustomPresenter
		{
			public override void Render(Node node)
			{
				var widget = node.AsWidget;
				widget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, widget.Size, Theme.Colors.WhiteBackground);
				Renderer.DrawRectOutline(Vector2.Zero, widget.Size, Theme.Colors.ControlBorder);
			}
		}
	}
}
#endif
