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
			RootWidget.AddChangeWatcher(() => Document.Current.AnimationFrame, (value) => {
				var markers = Document.Current.Container.Markers;
				int i = markers.FindIndex(m => m.Frame == value);
				if (i >= 0) {
					upperMarker = markers[i];
				}
			});
		}

		void RootWidget_DoubleClick(WidgetInput input, Key key)
		{
			Document.Current.History.DoTransaction(() => {
				var timeline = Timeline.Instance;
				var marker = Document.Current.Container.Markers.FirstOrDefault(
					i => i.Frame == timeline.CurrentColumn);
				var newMarker = marker?.Clone() ?? new Marker { Frame = timeline.CurrentColumn };
				var r = new MarkerPropertiesDialog().Show(newMarker, canDelete: marker != null);
				if (r == MarkerPropertiesDialog.Result.Ok) {
					Core.Operations.SetMarker.Perform(Document.Current.Container, newMarker, true);
				} else if (r == MarkerPropertiesDialog.Result.Delete) {
					Core.Operations.DeleteMarker.Perform(Document.Current.Container, marker, true);
				}
			});
			// To prevent RulerbarMouseScroll.
			RootWidget.Input.ConsumeKey(Key.Mouse0);
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
				if (upperMarker != m) {
					RenderMarker(m);
				}
			}

			RenderUpperMarker();
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
			r.AY = r.BY - 4;
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

		private Rectangle GetRectangle(int frame)
		{
			return new Rectangle {
				A = new Vector2(frame * TimelineMetrics.ColWidth + 0.5f, 0),
				B = new Vector2((frame + 1) * TimelineMetrics.ColWidth + 0.5f, RootWidget.Height)
			};
		}

		private static void CopyMarker(Marker marker)
		{
			var stream = new System.IO.MemoryStream();
			Serialization.WriteObject(Document.Current.Path, stream, marker, Serialization.Format.JSON);
			Clipboard.Text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
		}

		private static bool CanParseMarkerFromClipboard()
		{
			try {
				var text = Clipboard.Text;
				var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
				Serialization.ReadObject<Marker>(Document.Current.Path, stream);
				return true;
			} catch (System.Exception) { }
			return false;
		}

		private static void PasteMarker(int frameUnderMouse)
		{
			try {
				var text = Clipboard.Text;
				var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
				var m = Serialization.ReadObject<Marker>(Document.Current.Path, stream);
				m.Frame = frameUnderMouse;
				Document.Current.History.DoTransaction(() => {
					SetMarker.Perform(Document.Current.Container, m, true);
				});
			} catch (System.Exception e) {
				Debug.Write(e);
			}
		}

		private static void DeleteMarker(Marker marker)
		{
			Document.Current.History.DoTransaction(() => {
				Core.Operations.DeleteMarker.Perform(Document.Current.Container, marker, true);
			});
		}

		public static void CopyMarkers()
		{
			var frame = new Frame();
			foreach (var marker in Document.Current.Container.Markers) {
				frame.Markers.Add(marker);
			}
			var stream = new System.IO.MemoryStream();
			Serialization.WriteObject(Document.Current.Path, stream, frame, Serialization.Format.JSON);
			var text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
			Clipboard.Text = text;
		}

		private static bool CanParseMarkersFromClipboard()
		{
			try {
				var text = Clipboard.Text;
				var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
				var frame = Serialization.ReadObject<Frame>(Document.Current.Path, stream);
				return frame.Markers.Count > 0;
			} catch (System.Exception) { }
			return false;
		}

		public static void PasteMarkers()
		{
			try {
				Document.Current.History.DoTransaction(() => {
					var text = Clipboard.Text;
					var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));
					var frame = Serialization.ReadObject<Frame>(Document.Current.Path, stream);
					foreach (var marker in frame.Markers) {
						SetMarker.Perform(Document.Current.Container, marker, true);
					}
				});
			} catch (System.Exception e) {
				Debug.Write(e);
			}
		}

		public static void DeleteMarkers()
		{
			Document.Current.History.DoTransaction(() => {
				var timeline = Timeline.Instance;
				foreach (var marker in Document.Current.Container.Markers.ToList()) {
					Core.Operations.DeleteMarker.Perform(Document.Current.Container, marker, true);
				}
			});
		}

		class ContextMenu
		{
			public void Show()
			{
				var menu = new Menu();
				var frameUnderMouse = Timeline.Instance.Grid.CellUnderMouse().X;
				var marker = Document.Current.Container.Markers.FirstOrDefault(m => m.Frame == frameUnderMouse);
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
					Enabled = Document.Current.Container.Markers.Count > 0
				});
				menu.Add(new Command(TimelineCommands.PasteMarkers.Text, PasteMarkers) {
					Enabled = CanParseMarkersFromClipboard()
				});
				menu.Add(new Command(TimelineCommands.DeleteMarkers.Text, DeleteMarkers) {
					Enabled = Document.Current.Container.Markers.Count > 0
				});
				menu.Popup();
			}
		}
	}
}
