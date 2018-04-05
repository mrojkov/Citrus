using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class GridPane
	{
		readonly Timeline timeline;

		public readonly Widget RootWidget;
		public readonly Widget ContentWidget;
		public event Action<Widget> OnPostRender;

		public Vector2 Size => RootWidget.Size;
		public Vector2 ContentSize => ContentWidget.Size;

		public GridPane(Timeline timeline)
		{
			this.timeline = timeline;
			timeline.OffsetChanged += value => ContentWidget.Position = -value;
			RootWidget = new Frame {
				Id = nameof(GridPane),
				Layout = new StackLayout { HorizontallySizeable = true, VerticallySizeable = true },
				ClipChildren = ClipMethod.ScissorTest,
				HitTestTarget = true,
			};
			ContentWidget = new Widget {
				Id = nameof(GridPane) + "Content",
				Padding = new Thickness { Top = 1, Bottom = 1 },
				Layout = new VBoxLayout { Spacing = TimelineMetrics.RowSpacing },
				Presenter = new DelegatePresenter<Node>(RenderBackgroundAndGrid),
				PostPresenter = new DelegatePresenter<Widget>(w => OnPostRender(w))
			};
			RootWidget.AddNode(ContentWidget);
			RootWidget.AddChangeWatcher(() => RootWidget.Size,
				// Some document operation processors (e.g. ColumnCountUpdater) require up-to-date timeline dimensions.
				_ => Core.Operations.Dummy.Perform());
			OnPostRender += RenderSelection;
			OnPostRender += RenderCursor;
			RootWidget.Tasks.Add(HandleRightClickTask);
		}

		IEnumerator<object> HandleRightClickTask()
		{
			while (true) {
				if (RootWidget.Input.WasMouseReleased(1)) {
					new Menu {
						TimelineCommands.InsertFrame,
						TimelineCommands.DeleteFrame,
					}.Popup();
				}
				yield return null;
			}
		}

		private void RenderBackgroundAndGrid(Node node)
		{
			RootWidget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, RootWidget.Size, ColorTheme.Current.TimelineGrid.Lines);

			if (ContentWidget.Nodes.Count > 0) {
				ContentWidget.PrepareRendererState();
				Renderer.DrawRect(Vector2.Zero, ContentWidget.Size, ColorTheme.Current.Basic.WhiteBackground);

				RenderInvolvedFrames();
				RenderVerticalLines();
				RenderHorizontalLines();
				RenderMarkersBoundaries();
			}
		}

		private void RenderInvolvedFrames()
		{
			foreach (var row in Document.Current.Rows) {
				var nodeRow = row.Components.Get<Core.Components.NodeRow>()?.Node;

				if (nodeRow == null) {
					continue;
				}

				int lastFrameIndex = 0;

				foreach (var animator in nodeRow.Animators) {
					var key = animator.ReadonlyKeys.LastOrDefault();
					if (key != null && key.Frame > lastFrameIndex) {
						lastFrameIndex = key.Frame;
					}
				}

				if (lastFrameIndex > 0) {
					var gridWidget = row.GridWidget();

					Renderer.DrawRect(
						0.0f, gridWidget.Top(), (lastFrameIndex * TimelineMetrics.ColWidth), gridWidget.Bottom(),
						ColorTheme.Current.TimelineGrid.InvolvedFrames);
				}
			}
		}

		private void RenderVerticalLines()
		{
			Vector2 from = new Vector2(0.0f, 1.0f);
			Vector2 to = new Vector2(0.0f, ContentWidget.Height - 2.0f);

			for (int columnIndex = 0; columnIndex <= timeline.ColumnCount; columnIndex++) {
				if (timeline.IsColumnVisible(columnIndex)) {
					from.X = to.X = 0.5f + columnIndex * TimelineMetrics.ColWidth;
					Renderer.DrawLine(from, to, ColorTheme.Current.TimelineGrid.LinesLight);
				}
			}
		}

		private void RenderHorizontalLines()
		{
			Vector2 from = new Vector2(0.0f, 0.5f);
			Vector2 to = new Vector2(ContentWidget.Width, 0.5f);

			Renderer.DrawLine(from, to, ColorTheme.Current.TimelineGrid.Lines);

			foreach (var row in Document.Current.Rows) {
				from.Y = to.Y = 0.5f + row.GridWidget().Bottom();
				Renderer.DrawLine(from, to, ColorTheme.Current.TimelineGrid.Lines);
			}
		}

		private void RenderMarkersBoundaries()
		{
			Vector2 from = new Vector2(0.0f, 1.0f);
			Vector2 to = new Vector2(0.0f, ContentWidget.Height - 2.0f);

			foreach (var marker in Document.Current.Container.Markers) {
				if (timeline.IsColumnVisible(marker.Frame)) {
					from.X = to.X = 0.5f + TimelineMetrics.ColWidth * marker.Frame;
					Renderer.DrawLine(from, to, ColorTheme.Current.TimelineGrid.Lines);
				}
			}
		}

		private void RenderCursor(Node node)
		{
			var x = TimelineMetrics.ColWidth * (timeline.CurrentColumn + 0.5f);
			ContentWidget.PrepareRendererState();
			Renderer.DrawLine(
				x, 0, x, ContentWidget.Height - 1,
				Document.Current.PreviewAnimation ?
				ColorTheme.Current.TimelineRuler.RunningCursor :
				ColorTheme.Current.TimelineRuler.Cursor);
		}

		void RenderSelection(Widget widget)
		{
			RenderSelection(widget, IntVector2.Zero);
		}

		public void RenderSelection(Widget widget, IntVector2 offset)
		{
			widget.PrepareRendererState();
			var gridSpans = new List<Components.GridSpanList>();
			foreach (var row in Document.Current.Rows) {
				gridSpans.Add(row.Components.GetOrAdd<Components.GridSpanListComponent>().Spans.GetNonOverlappedSpans());
			}

			for (var row = 0; row < Document.Current.Rows.Count; row++) {
				var spans = gridSpans[row];
				var gridWidgetBottom = Document.Current.Rows[row].GridWidget().Bottom();
				int? lastColumn = null;
				var topSpans = row > 0 ? gridSpans[row - 1].GetEnumerator() : (IEnumerator<Components.GridSpan>)null;
				var bottomSpans = row + 1 < Document.Current.Rows.Count ? gridSpans[row + 1].GetEnumerator() : (IEnumerator<Components.GridSpan>)null;
				Components.GridSpan? topSpan = null;
				Components.GridSpan? bottomSpan = null;
				for (var i = 0; i < spans.Count; i++) {
					var span = spans[i];
					var isLastSpan = i + 1 == spans.Count;
					for (var column = span.A; column < span.B; column++) {
						var absentLeftCell = !lastColumn.HasValue || column - 1 > lastColumn.Value;
						if (topSpans != null && (!topSpan.HasValue || column >= topSpan.Value.B)) {
							do {
								if (!topSpans.MoveNext()) {
									topSpans = null;
									topSpan = null;
									break;
								}
								topSpan = topSpans.Current;
							} while (column >= topSpan.Value.B);
						}
						var absentTopCell = !topSpan.HasValue || column < topSpan.Value.A;
						var absentRightCell = column + 1 == span.B && (isLastSpan || column + 1 < spans[i + 1].A);
						if (bottomSpans != null && (!bottomSpan.HasValue || column >= bottomSpan.Value.B)) {
							do {
								if (!bottomSpans.MoveNext()) {
									bottomSpans = null;
									bottomSpan = null;
									break;
								}
								bottomSpan = bottomSpans.Current;
							} while (column >= bottomSpan.Value.B);
						}
						var absentBottomCell = !bottomSpan.HasValue || column < bottomSpan.Value.A;
						lastColumn = column;

						var a = CellToGridCoordinates(new IntVector2(column, row) + offset);
						var b = CellToGridCoordinates(new IntVector2(column + 1, row + 1) + offset);
						a = new Vector2(a.X + 1.5f, a.Y + 0.5f);
						b = new Vector2(b.X - 0.5f, gridWidgetBottom - 0.5f);
						Renderer.DrawRect(a + Vector2.Up * 0.5f, b + new Vector2(1f + (absentRightCell ? 0 : 1), (absentBottomCell ? 0 : 1)), ColorTheme.Current.TimelineGrid.Selection);
						if (absentLeftCell) {
							Renderer.DrawLine(a.X, a.Y - (absentTopCell ? 0 : 1), a.X, b.Y + (absentBottomCell ? 0 : 1), ColorTheme.Current.TimelineGrid.SelectionBorder, cap: LineCap.Square);
						}
						if (absentTopCell) {
							Renderer.DrawLine(a.X - (absentLeftCell ? 0 : 1), a.Y, b.X + (absentRightCell ? 0 : 1), a.Y, ColorTheme.Current.TimelineGrid.SelectionBorder, cap: LineCap.Square);
						}
						if (absentRightCell) {
							Renderer.DrawLine(b.X, a.Y - (absentTopCell ? 0 : 1), b.X, b.Y + (absentBottomCell ? 0 : 1), ColorTheme.Current.TimelineGrid.SelectionBorder, cap: LineCap.Square);
						}
						if (absentBottomCell) {
							Renderer.DrawLine(a.X - (absentLeftCell ? 0 : 1), b.Y, b.X + (absentRightCell ? 0 : 1), b.Y, ColorTheme.Current.TimelineGrid.SelectionBorder, cap: LineCap.Square);
						}
					}
				}
				topSpans = spans.GetEnumerator();
			}
		}

		public Vector2 CellToGridCoordinates(IntVector2 cell)
		{
			return CellToGridCoordinates(cell.Y, cell.X);
		}

		public Vector2 CellToGridCoordinates(int row, int col)
		{
			var rows = Document.Current.Rows;
			var y = row < rows.Count ? rows[Math.Max(row, 0)].GridWidget().Top() : rows[rows.Count - 1].GridWidget().Bottom();
			return new Vector2(col * TimelineMetrics.ColWidth, y);
		}

		public void TryDropFiles(IEnumerable<string> files)
		{
			if (!RootWidget.IsMouseOverThisOrDescendant() || Document.Current.Rows.Count == 0) {
				return;
			}
			var cell = CellUnderMouse();
			var widget = Document.Current.Rows[cell.Y].Components.Get<Core.Components.NodeRow>()?.Node as Widget;
			if (widget == null) {
				return;
			}
			Document.Current.History.BeginTransaction();
			try {
				foreach (var file in files) {
					string assetPath, assetType;
					if (Utils.ExtractAssetPathOrShowAlert(file, out assetPath, out assetType) && assetType == ".png") {
						var key = new Keyframe<ITexture> {
							Frame = cell.X,
							Value = new SerializableTexture(assetPath)
						};
						Core.Operations.SetKeyframe.Perform(widget, nameof(Widget.Texture), Document.Current.AnimationId, key);
						cell.X++;
					}
				}
			} finally {
				Document.Current.History.EndTransaction();
			}
		}

		public IntVector2 CellUnderMouse()
		{
			var mousePos = RootWidget.Input.MousePosition - ContentWidget.GlobalPosition;
			var r = new IntVector2((int)(mousePos.X / TimelineMetrics.ColWidth), 0);
			if (mousePos.Y >= ContentSize.Y) {
				r.Y = Math.Max(0, Document.Current.Rows.Count - 1);
				return r;
			}
			foreach (var row in Document.Current.Rows) {
				if (mousePos.Y >= row.GridWidget().Top() && mousePos.Y < row.GridWidget().Bottom() + TimelineMetrics.RowSpacing) {
					r.Y = row.Index;
					break;
				}
			}
			return r;
		}
	}
}
