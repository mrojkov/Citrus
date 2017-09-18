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
			ContentWidget = new CustomFrame {
				Layout = new VBoxLayout(),
				SizeChanged = RefreshContentScale
			};
			overlayWidget = new Widget { Presenter = new DelegatePresenter<Widget>(RenderOverlay) };
			RootWidget = new CustomFrame {
				Id = nameof(OverviewPane),
				LayoutCell = new LayoutCell { StretchY = 1 / 3f },
				ClipChildren = ClipMethod.ScissorTest,
				Layout = new StackLayout { HorizontallySizeable = true, VerticallySizeable = true },
				HitTestTarget = true,
				Nodes = {
					overlayWidget,
					ContentWidget,
				},
				SizeChanged = RefreshContentScale
			};
		}
			
		void RefreshContentScale()
		{
			ContentWidget.Scale = RootWidget.Size / Vector2.Max(Vector2.One, ContentWidget.Size);
		}

		void RenderOverlay(Widget widget)
		{
			var size = RootWidget.Size;
			widget.PrepareRendererState();
			var veilColor = ColorTheme.Current.TimelineOverview.Veil;
			var zoom = ContentWidget.Scale;
			var a = Vector2.Floor(timeline.Offset * zoom);
			var b = a + Vector2.Floor(new Vector2(timeline.Ruler.RootWidget.Width, timeline.Roll.RootWidget.Height) * zoom);
			b = Vector2.Min(size, b);
			Renderer.DrawRect(0, 0, a.X, size.Y, veilColor);
			Renderer.DrawRect(b.X, 0, size.X, size.Y, veilColor);
			Renderer.DrawRect(a.X, 0, b.X, a.Y, veilColor);
			Renderer.DrawRect(a.X, b.Y, b.X, size.Y, veilColor);
			Renderer.DrawRectOutline(a, b, ColorTheme.Current.TimelineOverview.Border);
		}

		class CustomFrame : Frame
		{
			public Action SizeChanged;

			protected override void OnSizeChanged(Vector2 sizeDelta)
			{
				SizeChanged?.Invoke();
				base.OnSizeChanged(sizeDelta);
			}
		}
	}
}