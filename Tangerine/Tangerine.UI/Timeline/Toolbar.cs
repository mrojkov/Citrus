using System;
using Lime;

namespace Tangerine.UI.Timeline
{
	public class Toolbar
	{
		public Widget Widget { get; private set; }
		
		public Toolbar()
		{
			Widget = new Widget {
				MinMaxHeight = Metrics.ToolbarHeight,
				MinWidth = Metrics.ToolbarMinWidth,
				Presenter = new WidgetPresenter(Render)
			};
		}
		
		void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawVerticalGradientRect(Vector2.Zero, widget.Size, Colors.Toolbar); 
		}
	}
}

