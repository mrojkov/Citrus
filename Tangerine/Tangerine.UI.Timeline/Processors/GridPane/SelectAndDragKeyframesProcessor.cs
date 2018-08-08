using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class SelectAndDragKeyframesProcessor : ITaskProvider
	{
		private static Timeline Timeline => Timeline.Instance;
		private static GridPane Grid => Timeline.Instance.Grid;

		private IntRectangle selectionRectangle;

		public IEnumerator<object> Task()
		{
			var input = Grid.RootWidget.Input;
			var lastSelectedCell = IntVector2.Zero;
			Node lastSelectionContainer = null;
			while (true) {
				if (input.WasMousePressed()) {
					using (Document.Current.History.BeginTransaction()) {
						var initialCell = Grid.CellUnderMouse(ignoreBounds: false);
						if (initialCell.Y >= 0 && initialCell.Y < Document.Current.Rows.Count) {
							if (IsCellSelected(initialCell)) {
								yield return DragSelectionTask(initialCell);
							} else {
								var r = new HasKeyframeRequest(initialCell);
								Timeline.Globals.Add(r);
								yield return null;
								Timeline.Globals.Remove<HasKeyframeRequest>();
								var isInMultiselectMode = input.IsKeyPressed(Key.Control);
								var isSelectRangeMode = input.IsKeyPressed(Key.Shift);

								if (isSelectRangeMode && lastSelectionContainer == Document.Current.Container) {
									yield return SelectRangeTask(lastSelectedCell, initialCell);
								} else if (!r.Result || isInMultiselectMode) {
									yield return SelectTask(initialCell);
									lastSelectedCell = initialCell;
									lastSelectionContainer = Document.Current.Container;
								} else {
									yield return DragSingleKeyframeTask(initialCell);
									lastSelectedCell = initialCell;
									lastSelectionContainer = Document.Current.Container;
								}
							}
						}
						Document.Current.History.CommitTransaction();
					}
				}
				yield return null;
			}
		}

		private static object SelectRangeTask(IntVector2 a, IntVector2 b)
		{
			Operations.ClearGridSelection.Perform();
			Core.Operations.ClearRowSelection.Perform();
			var r = new IntRectangle {
				A = {
					X = Math.Min(a.X, b.X),
					Y = Math.Min(a.Y, b.Y)
				},
				B = {
					X = Math.Max(a.X, b.X),
					Y = Math.Max(a.Y, b.Y)
				}
			};
			for (var i = r.A.Y; i <= r.B.Y; i++) {
				Operations.SelectGridSpan.Perform(i, r.A.X, r.B.X + 1);
			}
			return null;
		}

		private static bool IsCellSelected(IntVector2 cell)
		{
			return Document.Current.Rows[cell.Y].Components.GetOrAdd<GridSpanListComponent>().Spans.IsCellSelected(cell.X);
		}

		private static IEnumerator<object> DragSelectionTask(IntVector2 initialCell)
		{
			var input = Grid.RootWidget.Input;
			var offset = IntVector2.Zero;
			void Action(Widget widget) => Timeline.Grid.RenderSelection(widget, offset);
			Grid.OnPostRender += Action;
			float time = 0;

			while (input.IsMousePressed()) {
				time += Lime.Task.Current.Delta;
				offset = Grid.CellUnderMouse() - initialCell;
				Timeline.Ruler.MeasuredFrameDistance = Timeline.CurrentColumn - initialCell.X;

				if (!input.IsKeyPressed(Key.Shift)) {
					offset.Y = 0;
				}

				Window.Current.Invalidate();
				yield return null;
			}
			// If a user has clicked with control on a keyframe, try to deselect it [CIT-125].
			if (input.IsKeyPressed(Key.Control) && time < 0.2f) {
				var kfr = new HasKeyframeRequest(initialCell);
				Timeline.Globals.Add(kfr);
				yield return null;
				Timeline.Globals.Remove<HasKeyframeRequest>();
				if (kfr.Result) {
					Operations.DeselectGridSpan.Perform(kfr.Cell.Y, kfr.Cell.X, kfr.Cell.X + 1);
				}
			}
			Grid.OnPostRender -= Action;
			Window.Current.Invalidate();
			if (offset != IntVector2.Zero) {
				Timeline.Globals.Add(new DragKeyframesRequest(offset, !input.IsKeyPressed(Key.Alt)));
				Timeline.Ruler.MeasuredFrameDistance = 0;
			}
		}

		private static IEnumerator<object> DragSingleKeyframeTask(IntVector2 cell)
		{
			Core.Operations.ClearRowSelection.Perform();
			Operations.ClearGridSelection.Perform();
			Operations.SelectGridSpan.Perform(cell.Y, cell.X, cell.X + 1);
			yield return DragSelectionTask(cell);
		}

		private IEnumerator<object> SelectTask(IntVector2 initialCell)
		{
			var input = Grid.RootWidget.Input;
			if (!input.IsKeyPressed(Key.Control)) {
				Operations.ClearGridSelection.Perform();
				Core.Operations.ClearRowSelection.Perform();
				selectionRectangle = new IntRectangle();
			}
			Grid.OnPostRender += RenderSelectionRect;
			var showMeasuredFrameDistance = false;
			while (input.IsMousePressed()) {
				selectionRectangle.A = initialCell;
				selectionRectangle.B = Grid.CellUnderMouse();
				if (selectionRectangle.Width >= 0) {
					selectionRectangle.B.X++;
				} else {
					selectionRectangle.A.X++;
				}
				if (selectionRectangle.Height >= 0) {
					selectionRectangle.B.Y++;
				} else {
					selectionRectangle.A.Y++;
				}
				selectionRectangle = selectionRectangle.Normalized;
				showMeasuredFrameDistance |= selectionRectangle.Width != 1;
				if (showMeasuredFrameDistance) {
					Timeline.Instance.Ruler.MeasuredFrameDistance = selectionRectangle.Width;
				}
				Window.Current.Invalidate();
				yield return null;
			}
			Timeline.Instance.Ruler.MeasuredFrameDistance = 0;
			Grid.OnPostRender -= RenderSelectionRect;
			for (var r = selectionRectangle.A.Y; r < selectionRectangle.B.Y; r++) {
				Operations.SelectGridSpan.Perform(r, selectionRectangle.A.X, selectionRectangle.B.X);
			}
		}

		private void RenderSelectionRect(Widget widget)
		{
			widget.PrepareRendererState();
			var a = Grid.CellToGridCoordinates(selectionRectangle.A);
			var b = Grid.CellToGridCoordinates(selectionRectangle.B);
			Renderer.DrawRect(a, b, ColorTheme.Current.TimelineGrid.Selection);
			Renderer.DrawRectOutline(a, b, ColorTheme.Current.TimelineGrid.SelectionBorder);
		}
	}
}
