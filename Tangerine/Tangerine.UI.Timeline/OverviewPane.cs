using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class OverviewPane
	{
		readonly Widget overlayWidget;
		public readonly Widget RootWidget;
		public readonly Widget ContentWidget;

		Timeline timeline => Timeline.Instance;

		public OverviewPane()
		{
			ContentWidget = new Widget { Layout = new VBoxLayout() };
			ContentWidget.Updated += delta => ContentWidget.Scale = CalculateZoom();
			overlayWidget = new Widget { Presenter = new DelegatePresenter<Widget>(RenderOverlay) };
			RootWidget = new Frame {
				LayoutCell = new LayoutCell { StretchY = 1 / 3f },
				ClipChildren = ClipMethod.ScissorTest,
				Layout = new ScrollableLayout(),
				Nodes = {
					overlayWidget,
					ContentWidget,
				}
			};
		}

		void RenderOverlay(Widget widget)
		{
			var size = RootWidget.Size;
			widget.PrepareRendererState();
			var veilColor = Colors.OverviewVeil;
			var zoom = CalculateZoom();
			var a = Vector2.Floor(timeline.ScrollOrigin * zoom) + Vector2.Half;
			var b = a + Vector2.Floor(timeline.Grid.Size * zoom) - Vector2.Half;
			b = Vector2.Min(size - Vector2.Half, b);
			Renderer.DrawRect(0, 0, a.X, size.Y, veilColor);
			Renderer.DrawRect(b.X, 0, size.X, size.Y, veilColor);
			Renderer.DrawRect(a.X, 0, b.X, a.Y, veilColor);
			Renderer.DrawRect(a.X, b.Y, b.X, size.Y, veilColor);
			Renderer.DrawRectOutline(a, b, DesktopTheme.Colors.ControlBorder, 1);
		}

		public Vector2 CalculateZoom()
		{
			Vector2 scale = RootWidget.Size / ContentWidget.Size;
			return scale;
		}
	}
}