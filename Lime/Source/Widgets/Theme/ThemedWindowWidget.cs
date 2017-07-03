#if !ANDROID && !iOS
using System;

namespace Lime
{
	public class ThemedDefaultWindowWidget : DefaultWindowWidget
	{
		public override bool IsNotDecorated() => false;

		public ThemedDefaultWindowWidget(IWindow window) : base(window)
		{
			CompoundPresenter.Push(new WindowWidgetPresenter());
		}
	}

	public class ThemedInvalidableWindowWidget : InvalidableWindowWidget
	{
		public override bool IsNotDecorated() => false;

		public ThemedInvalidableWindowWidget(IWindow window) : base(window)
		{
			CompoundPresenter.Push(new WindowWidgetPresenter());
		}
	}

	internal class WindowWidgetPresenter : CustomPresenter
	{
		public override void Render(Node node)
		{
			var widget = node.AsWidget;
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, widget.Size, Theme.Colors.GrayBackground);
		}
	}
}
#endif