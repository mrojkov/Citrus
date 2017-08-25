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
							var isInMultiselectMode = input.IsKeyPressed(Key.Control);
							if (r.Result && !isInMultiselectMode) {
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
			return Document.Current.Rows[cell.Y].Components.GetOrAdd<GridSpanListComponent>().Spans.IsCellSelected(cell.X);
		}

		private IEnumerator<object> DragSelectionTask(IntVector2 initialCell)
		{
			var input = grid.RootWidget.Input;
			var offset = IntVector2.Zero;
			Action<Widget> r = widget => timeline.Grid.RenderSelection(widget, offset);
			grid.OnPostRender += r;
			float time = 0;
			while (input.IsMousePressed()) {
				time += Lime.Task.Current.Delta;
				offset = grid.CellUnderMouse() - initialCell;
				Window.Current.Invalidate();
				yield return null;
			}
			// If a user has clicked with control on a keyframe, try to deselect it [CIT-125].
			if (input.IsKeyPressed(Key.Control) && time < 0.2f) {
				var kfr = new HasKeyframeRequest(initialCell);
				timeline.Globals.Add(kfr);
				yield return null;
				timeline.Globals.Remove<HasKeyframeRequest>();
				if (kfr.Result) {
					Operations.DeselectGridSpan.Perform(kfr.Cell.Y, kfr.Cell.X, kfr.Cell.X + 1);
				}
			}
			grid.OnPostRender -= r;
			Window.Current.Invalidate();
			if (offset != IntVector2.Zero) {
				timeline.Globals.Add(new DragKeyframesRequest(offset, !input.IsKeyPressed(Key.Alt)));
			}
		}

		private IEnumerator<object> DragKeyframeTask(IntVector2 cell)
		{
			Core.Operations.ClearRowSelection.Perform();
			Operations.ClearGridSelection.Perform();
			Operations.SelectGridSpan.Perform(cell.Y, cell.X, cell.X + 1);
			yield return DragSelectionTask(cell);
		}

		private IEnumerator<object> SelectTask(IntVector2 initialCell)
		{
			var input = grid.RootWidget.Input;
			if (!input.IsKeyPressed(Key.Control)) {
				Operations.ClearGridSelection.Perform();
				Core.Operations.ClearRowSelection.Perform();
			}
			grid.OnPostRender += RenderSelectionRect;
			var showMeasuredFrameDistance = false;
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
				showMeasuredFrameDistance |= rect.Width != 1;
				if (showMeasuredFrameDistance) {
					Timeline.Instance.Ruler.MeasuredFrameDistance = rect.Width;
				}
				Window.Current.Invalidate();
				yield return null;
			}
			Timeline.Instance.Ruler.MeasuredFrameDistance = 0;
			grid.OnPostRender -= RenderSelectionRect;
			for (var r = rect.A.Y; r < rect.B.Y; r++) {
				Operations.SelectGridSpan.Perform(r, rect.A.X, rect.B.X);
			}
		}

		void RenderSelectionRect(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(grid.CellToGridCoordinates(rect.A), grid.CellToGridCoordinates(rect.B), ColorTheme.Current.TimelineGrid.Selection);
		}
	}
}
