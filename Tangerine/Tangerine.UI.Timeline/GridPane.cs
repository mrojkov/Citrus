using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class GridPane
	{
		readonly Timeline timeline;
		IntVector2 cellUnderMouseOnFilesDrop;
		RowLocation? rowLocationUnderMouseOnFilesDrop;
		int animateTextureCellOffset;

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
				Presenter = new SyncDelegatePresenter<Node>(RenderBackgroundAndGrid),
				PostPresenter = new SyncDelegatePresenter<Widget>(w => OnPostRender(w))
			};
			RootWidget.AddNode(ContentWidget);
			RootWidget.AddChangeWatcher(() => RootWidget.Size,
				// Some document operation processors (e.g. ColumnCountUpdater) require up-to-date timeline dimensions.
				_ => Core.Operations.Dummy.Perform(Document.Current.History));
			OnPostRender += RenderSelection;
			OnPostRender += RenderCursor;
			RootWidget.Tasks.Add(HandleRightClickTask);
			timeline.FilesDropHandler.Handling += FilesDropOnHandling;
			timeline.FilesDropHandler.NodeCreating += FilesDropOnNodeCreating;
			timeline.FilesDropHandler.NodeCreated += FilesDropOnNodeCreated;
		}

		IEnumerator<object> HandleRightClickTask()
		{
			while (true) {
				if (RootWidget.Input.WasMouseReleased(1)) {
					bool enabled = Document.Current.Rows.Count > 0;
					if (enabled) {
						var cell = CellUnderMouse();
						var row = Document.Current.Rows[cell.Y];
						var spans = row.Components.Get<Components.GridSpanListComponent>()?.Spans;
						if (!row.Selected || !spans.Any(i => i.Contains(cell.X))) {
							Document.Current.History.DoTransaction(() => {
								Core.Operations.ClearRowSelection.Perform();
								Operations.ClearGridSelection.Perform();
								Core.Operations.SelectRow.Perform(row);
								Operations.SelectGridSpan.Perform(cell.Y, cell.X, cell.X + 1);
							});
						}
					}
					var menu = new Menu {
						TimelineCommands.CutKeyframes,
						TimelineCommands.CopyKeyframes,
						TimelineCommands.PasteKeyframes,
						Command.MenuSeparator,
						TimelineCommands.ReverseKeyframes,
						Command.MenuSeparator,
						GenericCommands.InsertTimelineColumn,
						GenericCommands.RemoveTimelineColumn,
						TimelineCommands.DeleteKeyframes,
						Command.MenuSeparator,
						TimelineCommands.NumericMove,
						TimelineCommands.NumericScale
					};
					foreach (var i in menu) {
						i.Enabled = enabled;
					}
					menu.Popup();
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
				Renderer.DrawRect(Vector2.Zero, ContentWidget.Size, Theme.Colors.WhiteBackground);

				RenderAnimatedRangeBackground();
				RenderSelectedRowsBackground();
				RenderFramesInSelectedLevel();
				RenderVerticalLines();
				RenderHorizontalLines();
				RenderMarkerRulers();
			}
		}

		private void RenderFramesInSelectedLevel()
		{
			foreach (var row in Document.Current.Rows) {
				if (row.Selected) {
					var nodeRow = row.Components.Get<Core.Components.NodeRow>()?.Node;

					if (nodeRow == null) {
						continue;
					}
					var gridWidget = row.GridWidget();

					Renderer.DrawRect(
						0.0f, gridWidget.Top(), (Size.Length * TimelineMetrics.ColWidth), gridWidget.Bottom(),
						ColorTheme.Current.TimelineGrid.Backlight);
				}
			}
		}

		private void RenderAnimatedRangeBackground()
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
						ColorTheme.Current.TimelineGrid.AnimatedRangeBackground);
				}
			}
		}

		private void RenderSelectedRowsBackground()
		{
			foreach (var row in Document.Current.SelectedRows()) {
				var gridWidget = row.GridWidget();
				Renderer.DrawRect(
					0.0f, gridWidget.Top(), gridWidget.Right(), gridWidget.Bottom(),
					ColorTheme.Current.TimelineGrid.SelectedRowBackground);
			}
		}

		private void RenderVerticalLines()
		{
			var a = new Vector2(0.0f, 1.0f);
			var b = new Vector2(0.0f, ContentWidget.Height - 2.0f);
			timeline.GetVisibleColumnRange(out var minColumn, out var maxColumn);
			for (int columnIndex = minColumn; columnIndex <= maxColumn; columnIndex++) {
				a.X = b.X = 0.5f + columnIndex * TimelineMetrics.ColWidth;
				Renderer.DrawLine(a, b, ColorTheme.Current.TimelineGrid.LinesLight);
			}
		}

		private void RenderHorizontalLines()
		{
			var a = new Vector2(0.0f, 0.5f);
			var b = new Vector2(ContentWidget.Width, 0.5f);
			Renderer.DrawLine(a, b, ColorTheme.Current.TimelineGrid.Lines);
			foreach (var row in Document.Current.Rows) {
				a.Y = b.Y = 0.5f + row.GridWidget().Bottom();
				Renderer.DrawLine(a, b, ColorTheme.Current.TimelineGrid.Lines);
			}
		}

		private void RenderMarkerRulers()
		{
			var a = new Vector2(0.0f, 1.0f);
			var b = new Vector2(0.0f, ContentWidget.Height - 2.0f);
			foreach (var marker in Document.Current.Animation.Markers) {
				if (timeline.IsColumnVisible(marker.Frame)) {
					a.X = b.X = 0.5f + TimelineMetrics.ColWidth * marker.Frame;
					Renderer.DrawLine(a, b, ColorTheme.Current.TimelineGrid.Lines);
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
				int? lastColumn = null;
				var topSpans = row > 0 ? gridSpans[row - 1].GetEnumerator() : (IEnumerator<Components.GridSpan>)null;
				var bottomSpans = row + 1 < Document.Current.Rows.Count ? gridSpans[row + 1].GetEnumerator() : (IEnumerator<Components.GridSpan>)null;
				Components.GridSpan? topSpan = null;
				Components.GridSpan? bottomSpan = null;
				var offsetRow = row + offset.Y;
				var gridWidgetBottom = (0 <= offsetRow && offsetRow < Document.Current.Rows.Count) ? (float?)Document.Current.Rows[offsetRow].GridWidget().Bottom() : null;
				for (var i = 0; i < spans.Count; i++) {
					var span = spans[i];
					var isLastSpan = i + 1 == spans.Count;
					for (var column = span.A; column < span.B; column++) {
						var isLeftCellMissing = !lastColumn.HasValue || column - 1 > lastColumn.Value;
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
						var isTopCellMissing = !topSpan.HasValue || column < topSpan.Value.A;
						var isRightCellMissing = column + 1 == span.B && (isLastSpan || column + 1 < spans[i + 1].A);
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
						var isBottomCellMissing = !bottomSpan.HasValue || column < bottomSpan.Value.A;
						lastColumn = column;

						var a = CellToGridCoordinates(new IntVector2(column, row) + offset);
						var b = CellToGridCoordinates(new IntVector2(column + 1, row + 1) + offset);
						a = new Vector2(a.X + 1.5f, a.Y + 0.5f);
						b = new Vector2(b.X - 0.5f, (gridWidgetBottom ?? b.Y) - 0.5f);
						Renderer.DrawRect(a + Vector2.Up * 0.5f, b + new Vector2(1f + (isRightCellMissing ? 0 : 1), (isBottomCellMissing ? 0 : 1)), ColorTheme.Current.TimelineGrid.Selection);
						if (isLeftCellMissing) {
							Renderer.DrawLine(a.X, a.Y - (isTopCellMissing ? 0 : 1), a.X, b.Y + (isBottomCellMissing ? 0 : 1), ColorTheme.Current.TimelineGrid.SelectionBorder, cap: LineCap.Square);
						}
						if (isTopCellMissing) {
							Renderer.DrawLine(a.X - (isLeftCellMissing ? 0 : 1), a.Y, b.X + (isRightCellMissing ? 0 : 1), a.Y, ColorTheme.Current.TimelineGrid.SelectionBorder, cap: LineCap.Square);
						}
						if (isRightCellMissing) {
							Renderer.DrawLine(b.X, a.Y - (isTopCellMissing ? 0 : 1), b.X, b.Y + (isBottomCellMissing ? 0 : 1), ColorTheme.Current.TimelineGrid.SelectionBorder, cap: LineCap.Square);
						}
						if (isBottomCellMissing) {
							Renderer.DrawLine(a.X - (isLeftCellMissing ? 0 : 1), b.Y, b.X + (isRightCellMissing ? 0 : 1), b.Y, ColorTheme.Current.TimelineGrid.SelectionBorder, cap: LineCap.Square);
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

		private void FilesDropOnHandling()
		{
			animateTextureCellOffset = 0;
			cellUnderMouseOnFilesDrop = CellUnderMouse(RootWidget.Input.MousePosition - ContentWidget.GlobalPosition);
			rowLocationUnderMouseOnFilesDrop = SelectAndDragRowsProcessor.MouseToRowLocation(RootWidget.Input.MousePosition);
		}

		private void FilesDropOnNodeCreating(FilesDropHandler.NodeCreatingEventArgs nodeCreatingEventArgs)
		{
			var nodeUnderMouse = WidgetContext.Current.NodeUnderMouse;
			if (nodeUnderMouse == null || !nodeUnderMouse.SameOrDescendantOf(RootWidget)) {
				return;
			}

			switch (nodeCreatingEventArgs.AssetType) {
				case ".png": {
					if (Document.Current.Rows.Count == 0) {
						return;
					}
					var widget = Document.Current.Rows[cellUnderMouseOnFilesDrop.Y].Components.Get<Core.Components.NodeRow>()?.Node as Widget;
					if (widget == null) {
						return;
					}

					nodeCreatingEventArgs.Cancel = true;
					var key = new Keyframe<ITexture> {
						Frame = cellUnderMouseOnFilesDrop.X + animateTextureCellOffset,
						Value = new SerializableTexture(nodeCreatingEventArgs.AssetPath)
					};
					Core.Operations.SetKeyframe.Perform(widget, nameof(Widget.Texture), Document.Current.AnimationId, key);
					animateTextureCellOffset++;
					break;
				}
				case ".ogg": {
					nodeCreatingEventArgs.Cancel = true;
					var fileName = Path.GetFileNameWithoutExtension(nodeCreatingEventArgs.AssetPath);
					var node = Core.Operations.CreateNode.Perform(typeof(Audio));
					var sample = new SerializableSample(nodeCreatingEventArgs.AssetPath);
					Core.Operations.SetProperty.Perform(node, nameof(Audio.Sample), sample);
					Core.Operations.SetProperty.Perform(node, nameof(Node.Id), fileName);
					Core.Operations.SetProperty.Perform(node, nameof(Audio.Volume), 1);
					var key = new Keyframe<AudioAction> {
						Frame = cellUnderMouseOnFilesDrop.X,
						Value = AudioAction.Play
					};
					Core.Operations.SetKeyframe.Perform(node, nameof(Audio.Action), Document.Current.AnimationId, key);
					timeline.FilesDropHandler.OnNodeCreated(node);
					break;
				}
			}
		}

		private void FilesDropOnNodeCreated(Node node)
		{
			if (!rowLocationUnderMouseOnFilesDrop.HasValue) {
				return;
			}
			var location = rowLocationUnderMouseOnFilesDrop.Value;
			var row = Document.Current.Rows.FirstOrDefault(r => r.Components.Get<Core.Components.NodeRow>()?.Node == node);
			if (row != null) {
				if (location.Index >= row.Index) {
					location.Index++;
				}
				SelectAndDragRowsProcessor.Probers.Any(p => p.Probe(row, location));
			}
		}

		public IntVector2 CellUnderMouse(Vector2? position = null, bool ignoreBounds = true)
		{
			var mousePos = position ?? RootWidget.Input.MousePosition - ContentWidget.GlobalPosition;
			var r = new IntVector2((int)(mousePos.X / TimelineMetrics.ColWidth), 0);
			if (mousePos.Y >= ContentSize.Y) {
				r.Y = ignoreBounds ? Math.Max(0, Document.Current.Rows.Count - 1) : -1;
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
