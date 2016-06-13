using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class SelectAndDragKeyframesTask
	{
		IntRectangle rect;

		Timeline timeline => Timeline.Instance;
		GridPane grid => Timeline.Instance.Grid;

		public IEnumerator<object> Main()
		{
			var input = grid.RootWidget.Input;
			while (true) {
				if (input.WasMousePressed() && grid.RootWidget.IsMouseOver()) {
					var initialCell = MousePositionToCell(input.MousePosition);
					input.CaptureMouse();
					if (timeline.GridSelection.IsCellSelected(initialCell)) {
						yield return DragSelectionTask(initialCell);
					} else {
						var r = new HasKeyframeRequest(initialCell);
						timeline.Globals.Components.Add(r);
						yield return null;
						timeline.Globals.Components.Remove<HasKeyframeRequest>();
						if (r.Result) {
							yield return DragKeyframeTask(initialCell);
						} else {
							yield return SelectTask(initialCell);
						}
					}
					input.Release();
				}
				yield return null;
			}
		}

		private IEnumerator<object> DragSelectionTask(IntVector2 initialCell)
		{
			var selection = new GridSelection(timeline.GridSelection);
			var input = grid.RootWidget.Input;
			var offset = IntVector2.Zero;
			Action<Widget> r = widget => RenderSelection(widget, selection, offset);
			grid.OnPostRender += r;
			while (input.IsMousePressed()) {
				offset = MousePositionToCell(input.MousePosition) - initialCell;
				Window.Current.Invalidate();
				yield return null;
			}
			grid.OnPostRender -= r;
			timeline.Globals.Components.Add(new DragKeyframesRequest(offset, selection));
		}

		private IEnumerator<object> DragKeyframeTask(IntVector2 cell)
		{
			var rect = new IntRectangle { A = cell, B = cell + IntVector2.One };
			Document.Current.History.Add(new Operations.ClearGridSelection());
			Document.Current.History.Add(new Operations.SelectRectangleOnGrid(rect));
			Document.Current.History.Commit();
			yield return DragSelectionTask(cell);
		}

		private IEnumerator<object> SelectTask(IntVector2 initialCell)
		{
			Document.Current.History.Execute(new Operations.ClearGridSelection());
			var input = grid.RootWidget.Input;
			grid.OnPostRender += RenderSelectionRect;
			while (input.IsMousePressed()) {
				rect.A = initialCell;
				rect.B = MousePositionToCell(input.MousePosition);
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
			Document.Current.History.Execute(new Operations.SelectRectangleOnGrid(rect));
		}

		IntVector2 MousePositionToCell(Vector2 position)
		{
			position -= grid.ContentWidget.GlobalPosition;
			var r = new IntVector2((int)(position.X / Metrics.TimelineColWidth), 0);
			if (position.Y >= grid.ContentSize.Y) {
				r.Y = Math.Max(0, timeline.Rows.Count - 1);
				return r;
			}
			foreach (var row in timeline.Rows) {
				if (position.Y >= row.Top && position.Y < row.Bottom + Metrics.TimelineRowSpacing) {
					r.Y = row.Index;
					break;
				}
			}
			return r;
		}

		private void RenderSelection(Widget widget, GridSelection selection, IntVector2 offset)
		{
			widget.PrepareRendererState();
			foreach (var r in selection.GetNonOverlappedRects()) {
				Renderer.DrawRect(grid.CellToGridCoordinates(r.A + offset), grid.CellToGridCoordinates(r.B + offset), Colors.GridSelection);
			}
		}

		void RenderSelectionRect(Widget widget)
		{
			widget.PrepareRendererState();
			Renderer.DrawRect(grid.CellToGridCoordinates(rect.A), grid.CellToGridCoordinates(rect.B), Colors.GridSelection);
		}
	}
}
