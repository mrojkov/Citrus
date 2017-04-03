using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class SelectAndDragKeyframesProcessor : Core.ITaskProvider
	{
		IntRectangle rect;

		Timeline timeline => Timeline.Instance;
		GridPane grid => Timeline.Instance.Grid;

		public IEnumerator<object> Task()
		{
			var input = grid.RootWidget.Input;
			while (true) {
				if (input.WasMousePressed()) {
					var initialCell = grid.CellUnderMouse();
					if (initialCell.Y < Document.Current.Rows.Count) {
						input.CaptureMouse();
						if (IsCellSelected(initialCell)) {
							yield return DragSelectionTask(initialCell);
						} else {
							var r = new HasKeyframeRequest(initialCell);
							timeline.Globals.Add(r);
							yield return null;
							timeline.Globals.Remove<HasKeyframeRequest>();
							if (r.Result) {
								yield return DragKeyframeTask(initialCell);
							} else {
								yield return SelectTask(initialCell);
							}
						}
						input.ReleaseMouse();
					}
				}
				yield return null;
			}
		}

		bool IsCellSelected(IntVector2 cell)
		{
			return Document.Current.Rows[cell.Y].Components.GetOrAdd<Components.GridSpanList>().IsCellSelected(cell.X);
		}

		private IEnumerator<object> DragSelectionTask(IntVector2 initialCell)
		{
			var input = grid.RootWidget.Input;
			var offset = IntVector2.Zero;
			Action<Widget> r = widget => timeline.Grid.RenderSelection(widget, offset);
			grid.OnPostRender += r;
			while (input.IsMousePressed()) {
				offset = grid.CellUnderMouse() - initialCell;
				Window.Current.Invalidate();
				yield return null;
			}
			grid.OnPostRender -= r;
			if (offset != IntVector2.Zero) {
				timeline.Globals.Add(new DragKeyframesRequest(offset, !input.IsKeyPressed(Key.Alt)));
			}
		}

		private IEnumerator<object> DragKeyframeTask(IntVector2 cell)
		{
			Operations.ClearGridSelection.Perform();
			Operations.SelectGridSpan.Perform(cell.Y, new GridSpan(cell.X, cell.X + 1));
			yield return DragSelectionTask(cell);
		}

		private IEnumerator<object> SelectTask(IntVector2 initialCell)
		{
			Operations.ClearGridSelection.Perform();
			var input = grid.RootWidget.Input;
			grid.OnPostRender += RenderSelectionRect;
			while (input.IsMousePressed()) {
				rect.A = initialCell;
				rect.B = grid.CellUnderMouse();
				if (rect.Width >= 0) {
					rect.B.X++;
				} else {
					rect.A.X++;
				}
				if (rect.Height >= 0) {
					rect.B.Y++;
				} else {
					rect.A.Y++;
				}
				rect = rect.Normalized;
				Window.Current.Invalidate();
				yield return null;
			}
			grid.OnPostRender -= RenderSelectionRect;
			for (var r = rect.A.Y; r < rect.B.Y; r++) {
				Operations.SelectGridSpan.Perform(r, new GridSpan(rect.A.X, rect.B.X));
			}
		}

		void RenderSelectionRect(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(grid.CellToGridCoordinates(rect.A), grid.CellToGridCoordinates(rect.B), ColorTheme.Current.TimelineGrid.Selection);
		}
	}
}
