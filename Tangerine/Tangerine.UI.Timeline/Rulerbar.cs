using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI.Timeline
{
	public class Rulerbar
	{
		public int MeasuredFrameDistance { get; set; }
		public Widget RootWidget { get; private set; }

		private Marker upperMarker = null;
		private static Marker copiedMarker;
		private static List<Marker> copiedMarkers;

		public Rulerbar()
		{
			RootWidget = new Widget {
				Id = nameof(Rulerbar),
				MinMaxHeight = Metrics.ToolbarHeight,
				HitTestTarget = true
			};
			RootWidget.CompoundPresenter.Add(new SyncDelegatePresenter<Widget>(Render));
			RootWidget.LateTasks.Add(
				new KeyPressHandler(Key.Mouse0DoubleClick, RootWidget_DoubleClick),
				new KeyPressHandler(Key.Mouse1, (input, key) => new ContextMenu().Show())
			);
			RootWidget.AddChangeWatcher(() => Document.Current.AnimationFrame, (value) => {
				var markers = Document.Current.Animation.Markers;
				int i = markers.FindIndex(m => m.Frame == value);
				if (i >= 0) {
					upperMarker = markers[i];
				}
			});
			RootWidget.AddChangeWatcher(() => Document.Current.Container, (value) => {
				upperMarker = null;
			});
		}

		void RootWidget_DoubleClick(WidgetInput input, Key key)
		{
			Document.Current.History.DoTransaction(() => {
				var timeline = Timeline.Instance;
				var marker = Document.Current.Animation.Markers.GetByFrame(timeline.CurrentColumn);
				var newMarker = marker?.Clone() ?? new Marker { Frame = timeline.CurrentColumn };
				var r = new MarkerPropertiesDialog().Show(newMarker, canDelete: marker != null);
				if (r == MarkerPropertiesDialog.Result.Ok) {
					Core.Operations.SetMarker.Perform(newMarker, true);
				} else if (r == MarkerPropertiesDialog.Result.Delete) {
					Core.Operations.DeleteMarker.Perform(marker, true);
				}
			});
			// To prevent RulerbarMouseScroll.
			RootWidget.Input.ConsumeKey(Key.Mouse0);
		}

		void Render(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, RootWidget.Size, ColorTheme.Current.Toolbar.Background);
			Renderer.MultiplyTransform1(Matrix32.Translation(-Timeline.Instance.Offset.X.Round(), 0));
			RenderCursor();
			Timeline.Instance.GetVisibleColumnRange(out var minColumn, out var maxColumn);
			for (int i = minColumn; i <= maxColumn; i++) {
				if (i % 10 == 0) {
					var x = i * TimelineMetrics.ColWidth + 0.5f;
					float textHeight = Theme.Metrics.TextHeight;
					float y = (RootWidget.Height - textHeight) / 2;
					Renderer.DrawTextLine(
						new Vector2(x, y), i.ToString(),
						Theme.Metrics.TextHeight,
						Theme.Colors.BlackText,
						0.0f);
					if (!Document.Current.Animation.IsCompound) {
						Renderer.DrawLine(x, 0, x, RootWidget.Height, ColorTheme.Current.TimelineRuler.Notchings);
					}
				}
			}
			bool metUpperMarker = false;
			foreach (var m in Document.Current.Animation.Markers) {
				if (upperMarker != m) {
					RenderMarker(m);
				} else {
					metUpperMarker = true;
				}
			}
			if (!metUpperMarker && upperMarker != null) {
				upperMarker = null;
			}
			RenderUpperMarker();
		}

		void RenderCursor()
		{
			var r = GetRectangle(Timeline.Instance.CurrentColumnEased);
			Renderer.DrawRect(
				r.A, r.B,
				Document.Current.PreviewAnimation ?
					ColorTheme.Current.TimelineRuler.RunningCursor :
					ColorTheme.Current.TimelineRuler.Cursor);
		}

		void RenderMarker(Marker marker)
		{
			var r = GetRectangle(marker.Frame);
			r.AY = r.BY - 5;
			Renderer.DrawRect(r.A, r.B, GetMarkerColor(marker));
			Renderer.DrawRectOutline(r.A, r.B, ColorTheme.Current.TimelineRuler.MarkerBorder);
			if (!string.IsNullOrWhiteSpace(marker.Id)) {
				var h = Theme.Metrics.TextHeight;
				var padding = new Thickness { Left = 3.0f, Right = 5.0f, Top = 1.0f, Bottom = 1.0f };
				var extent = FontPool.Instance.DefaultFont.MeasureTextLine(marker.Id, h, 0.0f);
				var pos = new Vector2(r.A.X, r.A.Y - extent.Y - padding.Top - padding.Bottom - 1);
				Renderer.DrawRect(pos, pos + extent + padding.LeftTop + padding.RightBottom, Theme.Colors.WhiteBackground);
				Renderer.DrawRectOutline(pos, pos + extent + padding.LeftTop + padding.RightBottom, Theme.Colors.ControlBorder);
				Renderer.DrawTextLine(pos + padding.LeftTop, marker.Id, h, Theme.Colors.BlackText, 0.0f);
			}
		}

		void RenderUpperMarker()
		{
			if (upperMarker != null)
				RenderMarker(upperMarker);
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

		private Rectangle GetRectangle(float frame)
		{
			return new Rectangle {
				A = new Vector2(frame * TimelineMetrics.ColWidth + 0.5f, 0),
				B = new Vector2((frame + 1) * TimelineMetrics.ColWidth + 1.5f, RootWidget.Height)
			};
		}

		private static void CopyMarker(Marker marker)
		{
			copiedMarker = marker;
		}

		private static bool CanParseMarkerFromClipboard()
		{
			return copiedMarker != null;
		}

		private static void PasteMarker(int frameUnderMouse)
		{
			var m = copiedMarker.Clone();
			m.Frame = frameUnderMouse;
			Document.Current.History.DoTransaction(() => {
				SetMarker.Perform(m, true);
			});
		}

		private static void DeleteMarker(Marker marker)
		{
			Document.Current.History.DoTransaction(() => {
				Core.Operations.DeleteMarker.Perform(marker, true);
			});
		}

		public static void CopyMarkers()
		{
			copiedMarkers = new List<Marker>();
			foreach (var marker in Document.Current.Animation.Markers) {
				copiedMarkers.Add(marker);
			}
		}

		private static bool CanParseMarkersFromClipboard()
		{
			return copiedMarkers != null;
		}

		public static void PasteMarkers()
		{
			Document.Current.History.DoTransaction(() => {
				foreach (var marker in copiedMarkers) {
					var m = marker.Clone();
					SetMarker.Perform(m, true);
				}
			});
		}

		public static void DeleteMarkers()
		{
			Document.Current.History.DoTransaction(() => {
				var timeline = Timeline.Instance;
				foreach (var marker in Document.Current.Animation.Markers.ToList()) {
					Core.Operations.DeleteMarker.Perform(marker, true);
				}
			});
		}

		class ContextMenu
		{
			public void Show()
			{
				var menu = new Menu();
				var frameUnderMouse = Timeline.Instance.Grid.CellUnderMouse().X;
				var marker = Document.Current.Animation.Markers.GetByFrame(frameUnderMouse);
				menu.Add(new Command("Copy Marker", () => CopyMarker(marker)) {
					Enabled = marker != null
				});
				menu.Add(new Command("Paste Marker", () => PasteMarker(frameUnderMouse)) {
					Enabled = CanParseMarkerFromClipboard()
				});
				menu.Add(new Command("Delete Marker", () => DeleteMarker(marker)) {
					Enabled = marker != null
				});
				menu.Add(Command.MenuSeparator);
				menu.Add(new Command(TimelineCommands.CopyMarkers.Text, CopyMarkers) {
					Enabled = Document.Current.Animation.Markers.Count > 0
				});
				menu.Add(new Command(TimelineCommands.PasteMarkers.Text, PasteMarkers) {
					Enabled = CanParseMarkersFromClipboard()
				});
				menu.Add(new Command(TimelineCommands.DeleteMarkers.Text, DeleteMarkers) {
					Enabled = Document.Current.Animation.Markers.Count > 0
				});
				menu.Popup();
			}
		}
	}
}
