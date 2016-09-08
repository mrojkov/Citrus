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
				Layout = new VBoxLayout { Spacing = Metrics.TimelineRowSpacing },
				Presenter = new DelegatePresenter<Node>(RenderBackground),
				PostPresenter = new DelegatePresenter<Widget>(w => OnPostRender(w))
			};
			RootWidget.AddNode(ContentWidget);
			RootWidget.Updating += delta => {
				ContentWidget.Position = -timeline.ScrollOrigin;
			};
			OnPostRender += RenderGrid;
			OnPostRender += RenderSelection;
		}
		
		private void RenderBackground(Node node)
		{
			RootWidget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, RootWidget.Size, TimelineGridColors.Lines);
		}
		
		private void RenderGrid(Widget widget)
		{
			ContentWidget.PrepareRendererState();
			int numCols = timeline.ColumnCount;
			float x = 0.5f;
			for (int i = 0; i <= numCols; i++) {
				if (timeline.IsColumnVisible(i)) {
					Renderer.DrawLine(x, 0, x, ContentWidget.Height, TimelineGridColors.Lines);
				}
				x += Metrics.TimelineColWidth;
			}
			x = Metrics.TimelineColWidth * (timeline.CurrentColumn + 0.5f);
			Renderer.DrawLine(x, 0, x, ContentWidget.Height, Document.Current.Container.IsRunning ? TimelineRulerColors.RunningCursor : TimelineRulerColors.Cursor);
		}

		private void RenderSelection(Widget widget)
		{
			widget.PrepareRendererState();
			foreach (var rect in timeline.GridSelection.GetNonOverlappedRects()) {
				Renderer.DrawRect(CellToGridCoordinates(rect.A), CellToGridCoordinates(rect.B), TimelineGridColors.Selection);
			}
		}

		public Vector2 CellToGridCoordinates(IntVector2 cell)
		{
			var rows = Document.Current.Rows;
			var y = cell.Y < rows.Count ? rows[Math.Max(cell.Y, 0)].GetGridWidget().Top : rows[rows.Count - 1].GetGridWidget().Bottom;
			return new Vector2(cell.X * Metrics.TimelineColWidth, y);
		}
	}
}