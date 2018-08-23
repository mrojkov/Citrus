using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Operations;

namespace Tangerine.UI.Timeline
{
	public class RulerbarMouseScrollProcessor : ITaskProvider
	{
		static Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Task()
		{
			var rulerWidget = timeline.Ruler.RootWidget;
			var input = rulerWidget.Input;
			while (true) {
				if (input.WasMousePressed()) {
					Operations.SetCurrentColumn.Processor.CacheAnimationsStates = true;
					using (Document.Current.History.BeginTransaction()) {
						int initialCol = CalcColumn(rulerWidget.LocalMousePosition().X);
						int backup = timeline.CurrentColumn;
						var marker = Document.Current.Container.Markers.GetByFrame(initialCol);
						while (input.IsMousePressed()) {
							bool isEditing = input.IsKeyPressed(Key.Control);
							bool isShifting = isEditing && input.IsKeyPressed(Key.Shift);
							// Evgenii Polikutin: don't Undo to avoid animation cache invalidation when just scrolling
							// Evgenii Polikutin: yet we have to sacrifice performance when editing document
							SetCurrentColumn.IsFrozen = !isEditing;
							SetCurrentColumn.RollbackHistoryWithoutScrolling();
							SetCurrentColumn.IsFrozen = false;

							var cw = TimelineMetrics.ColWidth;
							var mp = rulerWidget.LocalMousePosition().X;
							if (mp > rulerWidget.Width - cw / 2) {
								timeline.OffsetX += cw;
							} else if (mp < cw / 2) {
								timeline.OffsetX = Math.Max(0, timeline.OffsetX - cw);
							}
							if (isEditing && !input.WasMousePressed()) {
								if (isShifting) {
									ShiftTimeline(CalcColumn(mp));
								} else if (marker != null) {
									DragMarker(marker, CalcColumn(mp));
								}
							}
							// Evgenii Polikutin: we need operation to backup the value we need, not the previous one
							Document.Current.AnimationFrame = backup;
							Operations.SetCurrentColumn.Perform(CalcColumn(mp));
							timeline.Ruler.MeasuredFrameDistance = timeline.CurrentColumn - initialCol;
							Window.Current.Invalidate();
							yield return null;
						}
						timeline.Ruler.MeasuredFrameDistance = 0;
						Operations.SetCurrentColumn.Processor.CacheAnimationsStates = false;
						Document.Current.History.CommitTransaction();
					}
				}
				yield return null;
			}
		}

		void ShiftTimeline(int destColumn)
		{
			var delta = destColumn - timeline.CurrentColumn;
			for (int i = 0; i < delta.Abs(); i++) {
				if (delta > 0) {
					Core.Operations.TimelineHorizontalShift.Perform(timeline.CurrentColumn, 1);
				} else {
					Core.Operations.TimelineHorizontalShift.Perform(destColumn, -1);
				}
			}
		}

		void DragMarker(Marker marker, int destColumn)
		{
			if (Document.Current.Container.Markers.Any(m => m.Frame == destColumn)) {
				// The place is taken by another marker.
				return;
			}
			// Delete and add marker again, because we want to maintain the markers order.
			Core.Operations.DeleteMarker.Perform(Document.Current.Container, marker, false);
			Core.Operations.SetProperty.Perform(marker, "Frame", destColumn);
			Core.Operations.SetMarker.Perform(Document.Current.Container, marker, true);
		}

		public static int CalcColumn(float mouseX)
		{
			return ((mouseX + timeline.Offset.X) / TimelineMetrics.ColWidth).Floor().Max(0);
		}
	}
}
