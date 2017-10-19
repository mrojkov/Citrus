using System;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class Rulerbar
	{
		public int MeasuredFrameDistance { get; set; }
		public Widget RootWidget { get; private set; }

		public Rulerbar()
		{
			RootWidget = new Widget {
				Id = nameof(Rulerbar),
				MinMaxHeight = Metrics.ToolbarHeight,
				HitTestTarget = true
			};
			RootWidget.CompoundPresenter.Add(new DelegatePresenter<Widget>(Render));
			RootWidget.LateTasks.Add(
				new KeyPressHandler(Key.Mouse0DoubleClick, RootWidget_DoubleClick),
				new KeyPressHandler(Key.Mouse1, (input, key) => new ContextMenu().Show())
			);
		}

		void RootWidget_DoubleClick(WidgetInput input, Key key)
		{
			var timeline = Timeline.Instance;
			var marker = Document.Current.Container.Markers.FirstOrDefault(
				i => i.Frame == timeline.CurrentColumn);
			var newMarker = marker?.Clone() ?? new Marker { Frame = timeline.CurrentColumn };
			var r = new MarkerPropertiesDialog().Show(newMarker, canDelete: marker != null);
			if (r == MarkerPropertiesDialog.Result.Ok) {
				Core.Operations.SetMarker.Perform(Document.Current.Container.DefaultAnimation.Markers, newMarker);
			} else if (r == MarkerPropertiesDialog.Result.Delete) {
				Core.Operations.DeleteMarker.Perform(Document.Current.Container.DefaultAnimation.Markers, marker);
			}
		}

		void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, RootWidget.Size, ColorTheme.Current.Toolbar.Background);
			Renderer.Transform1 *= Matrix32.Translation(-Timeline.Instance.Offset.X, 0);
			RenderCursor();
			for (int i = 0; i < Timeline.Instance.ColumnCount; i++) {
				var x = i * TimelineMetrics.ColWidth + 0.5f;
				if (i % 10 == 0) {
					float textHeight = Theme.Metrics.TextHeight;
					float y = (RootWidget.Height - textHeight) / 2;
					Renderer.DrawTextLine(
						new Vector2(x, y), i.ToString(),
						Theme.Metrics.TextHeight,
						Theme.Colors.BlackText,
						0.0f);
					Renderer.DrawLine(x, 0, x, RootWidget.Height, ColorTheme.Current.TimelineRuler.Notchings);
				}
			}
			foreach (var m in Document.Current.Container.Markers) {
				RenderMarker(m);
			}
		}

		void RenderCursor()
		{
			var r = GetRectangle(Timeline.Instance.CurrentColumn);
			Renderer.DrawRect(
				r.A, r.B,
				Document.Current.PreviewAnimation ?
					ColorTheme.Current.TimelineRuler.RunningCursor :
					ColorTheme.Current.TimelineRuler.Cursor);
		}

		void RenderMarker(Marker marker)
		{
			var r = GetRectangle(marker.Frame);
			r.A.Y = r.B.Y - 4;
			Renderer.DrawRect(r.A, r.B, GetMarkerColor(marker));
			if (!string.IsNullOrWhiteSpace(marker.Id)) {
				var h = Theme.Metrics.TextHeight;
				var padding = new Thickness { Left = 3.0f, Right = 5.0f, Top = 1.0f, Bottom = 1.0f };
				var extent = Renderer.MeasureTextLine(FontPool.Instance.DefaultFont, marker.Id, h, 0.0f);
				var pos = new Vector2(r.A.X, r.A.Y - extent.Y - padding.Top - padding.Bottom);
				Renderer.DrawRect(pos, pos + extent + padding.LeftTop + padding.RightBottom, Theme.Colors.WhiteBackground);
				Renderer.DrawRectOutline(pos, pos + extent + padding.LeftTop + padding.RightBottom, Theme.Colors.ControlBorder);
				Renderer.DrawTextLine(pos + padding.LeftTop, marker.Id, h, Theme.Colors.BlackText, 0.0f);
			}
		}

		Color4 GetMarkerColor(Marker marker)
		{
			switch (marker.Action) {
				case MarkerAction.Jump:
					return ColorTheme.Current.TimelineRuler.JumpMarker;
				case MarkerAction.Play:
					return ColorTheme.Current.TimelineRuler.PlayMarker;
				case MarkerAction.Stop:
					return ColorTheme.Current.TimelineRuler.StopMarker;
				default:
					return ColorTheme.Current.TimelineRuler.UnknownMarker;
			}
		}

		private Rectangle GetRectangle(int frame)
		{
			return new Rectangle {
				A = new Vector2(frame * TimelineMetrics.ColWidth + 0.5f, 0),
				B = new Vector2((frame + 1) * TimelineMetrics.ColWidth + 0.5f, RootWidget.Height)
			};
		}

		class ContextMenu
		{
			public void Show()
			{
				new Menu {
					TimelineCommands.CopyMarkers,
					TimelineCommands.PasteMarkers,
					TimelineCommands.DeleteMarkers
				}.Popup();
			}
		}
	}
}
