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
					yield return null;
					Operations.SetCurrentColumn.Processor.CacheAnimationsStates = true;
					using (Document.Current.History.BeginTransaction()) {
						int initialCurrentColumn = CalcColumn(rulerWidget.LocalMousePosition().X);
						Document.Current.AnimationFrame = initialCurrentColumn;
						Operations.SetCurrentColumn.Perform(initialCurrentColumn);
						int previousColumn = -1;
						var marker = Document.Current.Animation.Markers.GetByFrame(initialCurrentColumn);
						bool isShifting = false;
						while (input.IsMousePressed()) {
							bool isEditing = input.IsKeyPressed(Key.Control);
							bool startShifting = isEditing && input.IsKeyPressed(Key.Shift);
							isShifting = isShifting && startShifting;

							var cw = TimelineMetrics.ColWidth;
							var mp = rulerWidget.LocalMousePosition().X;
							if (mp > rulerWidget.Width - cw / 2) {
								timeline.OffsetX += cw;
							} else if (mp < cw / 2) {
								timeline.OffsetX = Math.Max(0, timeline.OffsetX - cw);
							}
							int newColumn = CalcColumn(mp);
							if (newColumn == previousColumn) {
								yield return null;
								continue;
							}
							// Evgenii Polikutin: don't Undo to avoid animation cache invalidation when just scrolling
							// Evgenii Polikutin: yet we have to sacrifice performance when editing document
							SetCurrentColumn.IsFrozen = !isEditing;
							SetCurrentColumn.RollbackHistoryWithoutScrolling();
							SetCurrentColumn.IsFrozen = false;

							if (isEditing && !input.WasMousePressed()) {
								if (isShifting) {
									ShiftTimeline(CalcColumn(mp));
								} else if (startShifting && CalcColumn(mp) == initialCurrentColumn) {
									isShifting = true;
								} else if (!startShifting && marker != null) {
									DragMarker(marker, CalcColumn(mp));
								}
							}
							// Evgenii Polikutin: we need operation to backup the value we need, not the previous one
							Document.Current.AnimationFrame = initialCurrentColumn;
							Operations.SetCurrentColumn.Perform(CalcColumn(mp));
							timeline.Ruler.MeasuredFrameDistance = timeline.CurrentColumn - initialCurrentColumn;
							if (newColumn == initialCurrentColumn && previousColumn != initialCurrentColumn) {
								Document.ForceAnimationUpdate();
							}
							previousColumn = newColumn;
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
			if (delta > 0) {
				Core.Operations.TimelineHorizontalShift.Perform(timeline.CurrentColumn, delta);
			} else if (delta < 0) {
				foreach (var node in Document.Current.Container.Nodes) {
					foreach (var animator in node.Animators.Where(i => i.AnimationId == Document.Current.AnimationId).ToList()) {
						Core.Operations.RemoveKeyframeRange.Perform(animator, destColumn, timeline.CurrentColumn - 1);
					}
				}
				foreach (var marker in Document.Current.Animation.Markers.Where(m => m.Frame >= destColumn && m.Frame < timeline.CurrentColumn).ToList()) {
					Core.Operations.DeleteMarker.Perform(marker, removeDependencies: false);
				}
				Core.Operations.TimelineHorizontalShift.Perform(destColumn, delta);
			}
		}

		void DragMarker(Marker marker, int destColumn)
		{
			if (Document.Current.Animation.Markers.Any(m => m.Frame == destColumn)) {
				// The place is taken by another marker.
				return;
			}
			// Delete and add marker again, because we want to maintain the markers order.
			Core.Operations.DeleteMarker.Perform(marker, false);
			Core.Operations.SetProperty.Perform(marker, "Frame", destColumn);
			Core.Operations.SetMarker.Perform(marker, true);
		}

		public static int CalcColumn(float mouseX)
		{
			return ((mouseX + timeline.Offset.X) / TimelineMetrics.ColWidth).Floor().Max(0);
		}
	}
}
