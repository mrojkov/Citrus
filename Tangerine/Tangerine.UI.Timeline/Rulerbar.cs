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
			RenderCursor();
			for (int i = 0; i < Timeline.Instance.ColumnCount; i++) {
				var x = i * Metrics.TimelineColWidth + 0.5f;
				if (i % 10 == 0) {
					float textHeight = DesktopTheme.Metrics.TextHeight;
					float y = (Widget.Height - textHeight) / 2;
					Renderer.DrawTextLine(
						new Vector2(x, y), i.ToString(),
						DesktopTheme.Metrics.TextHeight, 
						DesktopTheme.Colors.BlackText.ABGR);
				}
				Renderer.DrawLine(x, Widget.Height - 1, x, Widget.Height - 4, Colors.Timeline.Ruler.Notchings);
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
				B = new Vector2((frame + 1) * Metrics.TimelineColWidth + 0.5f, Widget.Height)
			};
		}
	}
}
