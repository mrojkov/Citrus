using System;
using Lime;

namespace Tangerine.UI.Timeline
{
	public class Toolbar
	{
		public Widget RootWidget { get; private set; }
		
		public Toolbar()
		{
			RootWidget = new Widget {
				MinMaxHeight = Metrics.ToolbarHeight,
				MinWidth = Metrics.ToolbarMinWidth,
				Presenter = new DelegatePresenter<Widget>(Render)
			};
		}
		
		void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawVerticalGradientRect(Vector2.Zero, widget.Size, Colors.Toolbar); 
		}
	}
}

