using System;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class Rulerbar
	{
		public Widget RootWidget { get; private set; }
		
		public Rulerbar()
		{
			RootWidget = new Widget {
				Id = nameof(Rulerbar),
				MinMaxHeight = Metrics.ToolbarHeight,
				HitTestTarget = true
			};
			RootWidget.CompoundPresenter.Add(new DelegatePresenter<Widget>(Render));
		}
		
		void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawVerticalGradientRect(Vector2.Zero, RootWidget.Size, Colors.Toolbar.Background);
			Renderer.Transform1 *= Matrix32.Translation(-Timeline.Instance.ScrollOrigin.X, 0);
			RenderCursor();
			for (int i = 0; i < Timeline.Instance.ColumnCount; i++) {
				var x = i * Metrics.TimelineColWidth + 0.5f;
				if (i % 10 == 0) {
					float textHeight = DesktopTheme.Metrics.TextHeight;
					float y = (RootWidget.Height - textHeight) / 2;
					Renderer.DrawTextLine(
						new Vector2(x, y), i.ToString(),
						DesktopTheme.Metrics.TextHeight, 
						DesktopTheme.Colors.BlackText);
				}
				Renderer.DrawLine(x, RootWidget.Height - 1, x, RootWidget.Height - 4, Colors.Timeline.Ruler.Notchings);
			}
			foreach (var m in Timeline.Instance.Container.Markers) {
				RenderMarker(m);
			}
		}

		void RenderCursor()
		{
			var r = GetRectangle(Timeline.Instance.CurrentColumn);
			Renderer.DrawRect(r.A, r.B, Colors.Timeline.Ruler.Cursor);
		}

		void RenderMarker(Marker marker)
		{
			var r = GetRectangle(marker.Frame);
			r.A.Y = r.B.Y - 4;
			Renderer.DrawRect(r.A, r.B, GetMarkerColor(marker));
			Renderer.DrawRectOutline(r.A, r.B, Colors.Timeline.Ruler.Notchings);
			if (!string.IsNullOrWhiteSpace(marker.Id)) {
				var h = DesktopTheme.Metrics.TextHeight;
				var extent = Renderer.MeasureTextLine(FontPool.Instance.DefaultFont, marker.Id, h) + Vector2.One;
				var pos = new Vector2(r.A.X, r.A.Y - extent.Y);
				Renderer.DrawRect(pos, pos + extent, DesktopTheme.Colors.WhiteBackground);
				Renderer.DrawRectOutline(pos, pos + extent, DesktopTheme.Colors.ControlBorder);
				Renderer.DrawTextLine(pos, marker.Id, h, DesktopTheme.Colors.BlackText);
			}
		}

		Color4 GetMarkerColor(Marker marker)
		{
			switch (marker.Action) {
				case MarkerAction.Jump:
					return Colors.Timeline.Ruler.JumpMarker;
				case MarkerAction.Play:
					return Colors.Timeline.Ruler.PlayMarker;
				case MarkerAction.Stop:
					return Colors.Timeline.Ruler.StopMarker;
				default:
					return Colors.Timeline.Ruler.UnknownMarker;
			}
		}

		private Rectangle GetRectangle(int frame)
		{
			return new Rectangle {
				A = new Vector2(frame * Metrics.TimelineColWidth + 0.5f, 0),
				B = new Vector2((frame + 1) * Metrics.TimelineColWidth + 0.5f, RootWidget.Height)
			};
		}
	}
}
