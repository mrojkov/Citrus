using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	class ScrollableLayout : CommonLayout, ILayout
	{
		public override void MeasureSizeConstraints(Widget widget)
		{
			ConstraintsValid = true;
		}

		public override void ArrangeChildren(Widget widget)
		{
			ArrangementValid = true;
			foreach (var w in GetChildren(widget)) {
				w.Size = w.EffectiveMinSize;
			}
		}
	}

	public class GridPane
	{
		Timeline timeline => Timeline.Instance;

		public readonly Widget RootWidget;
		public readonly Widget OverlayWidget;
		public readonly Widget ContentWidget;
		public event Action<Widget> OnRenderOverlay;

		public Vector2 Size => RootWidget.Size;
		public Vector2 ContentSize => ContentWidget.Size;

		public GridPane()
		{
			RootWidget = new Frame {
				Layout = new ScrollableLayout(),
				ClipChildren = ClipMethod.ScissorTest,
			};
			ContentWidget = new Widget {
				Padding = new Thickness { Top = 1, Bottom = 1 },
				Layout = new VBoxLayout { Spacing = Metrics.RowSpacing },
				Presenter = new WidgetPresenter(RenderBackground)
			};
			OverlayWidget = new Widget { Presenter = new WidgetPresenter(w => OnRenderOverlay(w)) };
			RootWidget.AddNode(OverlayWidget);
			RootWidget.AddNode(ContentWidget);
			RootWidget.Updating += delta => ContentWidget.Position = -timeline.ScrollOrigin;
			OnRenderOverlay += RenderGrid;
			OnRenderOverlay += RenderSelection;
		}
		
		private void RenderBackground(Widget widget)
		{
			RootWidget.PrepareRendererState();
			Renderer.DrawRect(Vector2.Zero, RootWidget.Size, Colors.GridLines);
		}
		
		private void RenderGrid(Widget widget)
		{
			ContentWidget.PrepareRendererState();
			int numCols = timeline.ColumnCount;
			float x = 0.5f;
			for (int i = 0; i <= numCols; i++) {
				if (timeline.IsColumnVisible(i)) {
					Renderer.DrawLine(x, 0, x, ContentWidget.Height, Colors.GridLines);
				}
				x += Metrics.ColWidth;
			}
			x = Metrics.ColWidth * (timeline.CurrentColumn + 0.5f);
			Renderer.DrawLine(x, 0, x, ContentWidget.Height, Color4.Red);
		}

		private void RenderSelection(Widget widget)
		{
			widget.PrepareRendererState();
			foreach (var rect in timeline.GridSelection.GetNonOverlappedRects()) {
				Renderer.DrawRect(CellToGridCoordinates(rect.A), CellToGridCoordinates(rect.B), Colors.GridSelection);
			}
		}

		public Vector2 CellToGridCoordinates(IntVector2 cell)
		{
			var rows = timeline.Rows;
			var y = cell.Y < rows.Count ? rows[Math.Max(cell.Y, 0)].Top : rows[rows.Count - 1].Bottom;
			return new Vector2(cell.X * Metrics.ColWidth, y) + ContentWidget.Position;
		}
	}
}