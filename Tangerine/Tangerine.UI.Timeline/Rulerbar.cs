using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class Rulerbar
	{
		public Widget Widget { get; private set; }
		
		public Rulerbar()
		{
			Widget = new Widget { MinMaxHeight = Metrics.ToolbarHeight, Presenter = new DelegatePresenter<Widget>(Render) };
		}
		
		void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawVerticalGradientRect(Vector2.Zero, Widget.Size, Colors.Toolbar);
			Renderer.Transform1 *= Matrix32.Translation(-Timeline.Instance.ScrollOrigin.X, 0);
			for (int i = 0; i < Timeline.Instance.ColumnCount; i++) {
				var x = i * Metrics.ColWidth + 0.5f;
				if (i % 10 == 0) {
					float textHeight = DesktopTheme.Metrics.TextHeight;
					float y = Widget.Height - textHeight;
					Renderer.DrawTextLine(
						new Vector2(x, y), i.ToString(),
						DesktopTheme.Metrics.TextHeight, 
						DesktopTheme.Colors.BlackText.ABGR);
				}
			}
		}
	}
}
