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
			Renderer.DrawRect(Vector2.Zero, RootWidget.Size, ToolbarColors.Background);
			Renderer.Transform1 *= Matrix32.Translation(-Timeline.Instance.ScrollPos.X, 0);
			RenderCursor();
			for (int i = 0; i < Timeline.Instance.ColumnCount; i++) {
				var x = i * TimelineMetrics.ColWidth + 0.5f;
				if (i % 10 == 0) {
					float textHeight = DesktopTheme.Metrics.TextHeight;
					float y = (RootWidget.Height - textHeight) / 2;
					Renderer.DrawTextLine(
						new Vector2(x, y), i.ToString(),
						DesktopTheme.Metrics.TextHeight, 
						DesktopTheme.Colors.BlackText);
					Renderer.DrawLine(x, 0, x, RootWidget.Height, TimelineRulerColors.Notchings);
				}
			}
			foreach (var m in Timeline.Instance.Container.Markers) {
				RenderMarker(m);
			}
		}

		void RenderCursor()
		{
			var r = GetRectangle(Timeline.Instance.CurrentColumn);
			Renderer.DrawRect(r.A, r.B, Document.Current.Container.IsRunning ? TimelineRulerColors.RunningCursor : TimelineRulerColors.Cursor);
		}

		void RenderMarker(Marker marker)
		{
			var r = GetRectangle(marker.Frame);
			r.A.Y = r.B.Y - 4;
			Renderer.DrawRect(r.A, r.B, GetMarkerColor(marker));
			Renderer.DrawRectOutline(r.A, r.B, TimelineRulerColors.Notchings);
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
					return TimelineRulerColors.JumpMarker;
				case MarkerAction.Play:
					return TimelineRulerColors.PlayMarker;
				case MarkerAction.Stop:
					return TimelineRulerColors.StopMarker;
				default:
					return TimelineRulerColors.UnknownMarker;
			}
		}

		private Rectangle GetRectangle(int frame)
		{
			return new Rectangle {
				A = new Vector2(frame * TimelineMetrics.ColWidth + 0.5f, 0),
				B = new Vector2((frame + 1) * TimelineMetrics.ColWidth + 0.5f, RootWidget.Height)
			};
		}
	}
}
