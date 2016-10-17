using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class SelectAndDragRowsProcessor : IProcessor
	{
		int dragPosition;

		Timeline timeline => Timeline.Instance;
		RollPane roll => Timeline.Instance.Roll;

		public IEnumerator<object> Loop()
		{
			var input = roll.RootWidget.Input;
			input.AcceptMouseThroughDescendants = true;
			while (true) {
				yield return null;
				if (!input.WasMousePressed() || input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					continue;
				}
				var initialMousePosition = input.MousePosition;
				var row = MousePositionToRow(initialMousePosition);
				if (input.IsKeyPressed(Key.Shift)) {
					if (Document.Current.SelectedRows.Count > 0) {
						var firstRow = Document.Current.SelectedRows[0];
						Core.Operations.ClearRowSelection.Perform();
						Core.Operations.SelectRowRange.Perform(firstRow, row);
					} else {
						Core.Operations.ClearRowSelection.Perform();
						Core.Operations.SelectRow.Perform(row);
					}
				} else {
					input.CaptureMouse();
					if (!Document.Current.SelectedRows.Contains(row)) {
						Core.Operations.ClearRowSelection.Perform();
						Core.Operations.SelectRow.Perform(row);
					}
					while (input.IsMousePressed() && Math.Abs(initialMousePosition.Y - input.MousePosition.Y) < TimelineMetrics.DefaultRowHeight / 4) {
						yield return null;
					}
					if (input.IsMousePressed()) {
						yield return DragTask();
					}
					input.ReleaseMouse();
				}
			}
		}

		private IEnumerator<object> DragTask()
		{
			roll.OnRenderOverlay += RenderDragCursor;
			var input = roll.RootWidget.Input;
			while (input.IsMousePressed()) {
				dragPosition = MouseToDragPosition(input.MousePosition);
				Window.Current.Invalidate();
				yield return null;
			}
			roll.OnRenderOverlay -= RenderDragCursor;
			Window.Current.Invalidate();
			Core.Operations.DragRows.Perform(dragPosition);
		}

		private void RenderDragCursor(Widget widget)
		{
			roll.ContentWidget.PrepareRendererState();
			var y = dragPosition == 0 ? 0 : Document.Current.Rows[dragPosition - 1].GetGridWidget().Bottom;
			Renderer.DrawRect(new Vector2(0, y - 1), new Vector2(roll.ContentWidget.Width, y + 1), Colors.DragCursor);
		}

		Row MousePositionToRow(Vector2 position)
		{
			position -= roll.ContentWidget.GlobalPosition;
			if (position.Y < 0) {
				return Document.Current.Rows[0];
			}
			foreach (var row in Document.Current.Rows) {
				if (position.Y >= row.GetGridWidget().Top && position.Y < row.GetGridWidget().Bottom + TimelineMetrics.RowSpacing) {
					return Document.Current.Rows[row.Index];
				}
			}
			return Document.Current.Rows[Math.Max(0, Document.Current.Rows.Count - 1)];
		}

		int MouseToDragPosition(Vector2 position)
		{
			position -= roll.ContentWidget.GlobalPosition;
			if (position.Y < 0) {
				return 0;
			}
			foreach (var row in Document.Current.Rows) {
				var gw = row.GetGridWidget();
				if (position.Y >= gw.Top && position.Y < gw.Bottom + TimelineMetrics.RowSpacing) {
					return position.Y > (gw.Top + gw.Bottom) / 2 ? row.Index + 1 : row.Index;
				}
			}
			return Document.Current.Rows.Count;
		}
	}
}