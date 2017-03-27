using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class GridPane
	{
		Timeline timeline => Timeline.Instance;

		public readonly Widget RootWidget;
		public readonly Widget ContentWidget;
		public event Action<Widget> OnPostRender;

		public Vector2 Size => RootWidget.Size;
		public Vector2 ContentSize => ContentWidget.Size;

		public GridPane()
		{
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
				Presenter = new DelegatePresenter<Node>(RenderBackground),
				PostPresenter = new DelegatePresenter<Widget>(w => OnPostRender(w))
			};
			RootWidget.AddNode(ContentWidget);
			RootWidget.AddChangeWatcher(() => RootWidget.Size, 
				// Some document operation processors (e.g. ColumnCountUpdater) require up-to-date timeline dimensions.
				_ => Core.Operations.Dummy.Perform());
			OnPostRender += RenderGrid;
			OnPostRender += RenderSelection;
		}
		
		private void RenderBackground(Node node)
		{
			RootWidget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, RootWidget.Size, ColorTheme.Current.TimelineGrid.Lines);
		}

		public void SetOffset(Vector2 value)
		{
			ContentWidget.Position = -value;
		}
		
		private void RenderGrid(Widget widget)
		{
			ContentWidget.PrepareRendererState();
			int numCols = timeline.ColumnCount;
			float x = 0.5f;
			for (int i = 0; i <= numCols; i++) {
				if (timeline.IsColumnVisible(i)) {
					Renderer.DrawLine(x, 0, x, ContentWidget.Height, ColorTheme.Current.TimelineGrid.Lines);
				}
				x += TimelineMetrics.ColWidth;
			}
			x = TimelineMetrics.ColWidth * (timeline.CurrentColumn + 0.5f);
			Renderer.DrawLine(
				x, 0, x, ContentWidget.Height,
				Document.Current.Container.IsRunning ? 
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
			foreach (var row in Document.Current.Rows) {
				var s = row.Components.GetOrAdd<Components.GridSpanList>();
				foreach (var i in s.GetNonOverlappedSpans()) {
					var a = CellToGridCoordinates(new IntVector2(i.A, row.Index) + offset);
					var b = CellToGridCoordinates(new IntVector2(i.B, row.Index + 1) + offset);
					Renderer.DrawRect(a, b, ColorTheme.Current.TimelineGrid.Selection);
				}
			}
		}

		public Vector2 CellToGridCoordinates(IntVector2 cell)
		{
			return CellToGridCoordinates(cell.Y, cell.X);
		}

		public Vector2 CellToGridCoordinates(int row, int col)
		{
			var rows = Document.Current.Rows;
			var y = row < rows.Count ? rows[Math.Max(row, 0)].GetGridWidget().Top : rows[rows.Count - 1].GetGridWidget().Bottom;
			return new Vector2(col * TimelineMetrics.ColWidth, y);
		}
	}
}