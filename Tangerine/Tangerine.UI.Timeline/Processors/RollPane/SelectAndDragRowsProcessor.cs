using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;

namespace Tangerine.UI.Timeline
{
	public class SelectAndDragRowsProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			var roll = Timeline.Instance.Roll;
			var input = roll.RootWidget.Input;
			input.AcceptMouseThroughDescendants = true;
			while (true) {
				yield return null;
				if (!input.WasMousePressed() || input.WasKeyPressed(Key.Mouse0DoubleClick)) {
					continue;
				}
				var initialMousePosition = input.MousePosition;
				var row = RowUnderMouse(initialMousePosition);
				if (row == null) {
					continue;
				}
				if (input.IsKeyPressed(Key.Shift)) {
					if (Document.Current.SelectedRows().Any()) {
						var firstRow = Document.Current.SelectedRows().FirstOrDefault();
						ClearRowSelection.Perform();
						SelectRowRange.Perform(firstRow, row);
					} else {
						ClearRowSelection.Perform();
						SelectRow.Perform(row);
					}
				} else {
					input.CaptureMouse();
					if (!row.Selected) {
						ClearRowSelection.Perform();
						SelectRow.Perform(row);
					}
					while (
						input.IsMousePressed() && 
						Math.Abs(initialMousePosition.Y - input.MousePosition.Y) < TimelineMetrics.DefaultRowHeight / 4
					) {
						yield return null;
					}
					if (input.IsMousePressed()) {
						yield return DragTask();
					}
					input.ReleaseMouse();
				}
			}
		}

		static IEnumerator<object> DragTask()
		{
			var roll = Timeline.Instance.Roll;
			var dragLocation = new RowLocation(Document.Current.RowTree, 0);
			Action<Widget> a = _ => RenderDragCursor(dragLocation);
			roll.OnRenderOverlay += a;
			var input = roll.RootWidget.Input;
			while (input.IsMousePressed()) {
				dragLocation = MouseToRowLocation(input.MousePosition);
				CommonWindow.Current.Invalidate();
				yield return null;
			}
			roll.OnRenderOverlay -= a;
			CommonWindow.Current.Invalidate();
			DragRows(dragLocation);
		}

		static void DragRows(RowLocation dragLocation)
		{
			if (!CanDrag(dragLocation)) {
				// Do not allow to drag the rows into their own guts.
				return;
			}
			var c = Document.Current.TopLevelSelectedRows().Count(
				r => r.Parent == dragLocation.ParentRow && dragLocation.ParentRow.Rows.IndexOf(r) < dragLocation.Index);
			dragLocation.Index -= c;
			var data = Copy.CopyToString();
			if (Paste.CanPaste(data, dragLocation)) {
				Delete.Perform();
				Paste.Perform(data, dragLocation);
			}
		}

		static bool CanDrag(RowLocation dragLocation)
		{
			return !Document.Current.TopLevelSelectedRows().Any(i => i == dragLocation.ParentRow || i.Rows.Contains(dragLocation.ParentRow));
		}

		static void RenderDragCursor(RowLocation rowLocation)
		{
			float y = 1;
			var pr = rowLocation.ParentRow;
			if (rowLocation.Index < pr.Rows.Count) {
				y = pr.Rows[rowLocation.Index].GetGridWidget().Top;
			} else if (pr.Rows.Count > 0) {
				var lastRow = pr.Rows[rowLocation.Index - 1];
				y = lastRow.GetGridWidget().Bottom + CalcSubtreeHeight(lastRow) + TimelineMetrics.RowSpacing;
			} else if (pr != Document.Current.RowTree) {
				y = pr.GetGridWidget().Bottom + TimelineMetrics.RowSpacing;
			}
			Timeline.Instance.Roll.ContentWidget.PrepareRendererState();
			Renderer.DrawRect(
				new Vector2(TimelineMetrics.RollIndentation * CalcIndentation(pr), y - 1), 
				new Vector2(Timeline.Instance.Roll.ContentWidget.Width, y + 1), Colors.DragCursor);
		}

		static int CalcIndentation(Row row)
		{
			int i = 0;
			for (var r = row.Parent; r != null; r = r.Parent) {
				i++;
			}
			return i;
		}

		static float CalcSubtreeHeight(Row row)
		{
			float r = 0;
			foreach (var i in row.Rows) {
				r += i.GetGridWidget().Height + TimelineMetrics.RowSpacing + CalcSubtreeHeight(i);
			}
			return r;
		}

		static Row RowUnderMouse(Vector2 position)
		{
			var doc = Document.Current;
			if (doc.Rows.Count == 0) {
				return null;
			}
			position -= Timeline.Instance.Roll.ContentWidget.GlobalPosition;
			if (position.Y < 0) {
				return doc.Rows[0];
			}
			foreach (var row in doc.Rows) {
				var gw = row.GetGridWidget();
				if (position.Y >= gw.Top && position.Y < gw.Bottom + TimelineMetrics.RowSpacing) {
					return doc.Rows[row.Index];
				}
			}
			return doc.Rows[doc.Rows.Count - 1];
		}

		static RowLocation MouseToRowLocation(Vector2 position)
		{
			position -= Timeline.Instance.Roll.ContentWidget.GlobalPosition;
			if (position.Y <= 0) {
				return new RowLocation(Document.Current.RowTree, 0);
			}
			foreach (var row in Document.Current.Rows) {
				var gw = row.GetGridWidget();
				if (position.Y >= gw.Top && position.Y < gw.Bottom + TimelineMetrics.RowSpacing) {
					var index = row.Parent.Rows.IndexOf(row);
					if (position.Y < gw.Top + gw.Height * 0.5f) {
						return new RowLocation(row.Parent, index);
					} else if (row.Rows.Count > 0) {
						return new RowLocation(row, 0);
					} else if (position.Y < gw.Top + gw.Height * 0.75f && row.CanHaveChildren) {
						return new RowLocation(row, 0);
					} else {
						return new RowLocation(row.Parent, index + 1);
					}
				}
			}
			return new RowLocation(Document.Current.RowTree, Document.Current.RowTree.Rows.Count);
		}
	}
}