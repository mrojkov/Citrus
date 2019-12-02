using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Operations;
using Tangerine.UI.Docking;
using Tangerine.UI.Timeline.Operations;

namespace Tangerine.UI.Timeline
{
	public class RulerbarMouseScrollProcessor : ITaskProvider
	{
		private static Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Task()
		{
			var rulerWidget = timeline.Ruler.RootWidget;
			var input = rulerWidget.Input;
			while (true) {
				if (input.WasMousePressed()) {
					yield return null;
					SetCurrentColumn.Processor.CacheAnimationsStates = true;
					using (Document.Current.History.BeginTransaction()) {
						int initialCurrentColumn = CalcColumn(rulerWidget.LocalMousePosition().X);
						Document.Current.AnimationFrame = initialCurrentColumn;
						var saved = CoreUserPreferences.Instance.StopAnimationOnCurrentFrame;
						// Dirty hack: prevent creating RestoreAnimationsTimesComponent
						// in order to stop running animation on clicked frame (RBT-2887)
						CoreUserPreferences.Instance.StopAnimationOnCurrentFrame = true;
						SetCurrentColumn.Perform(initialCurrentColumn);
						CoreUserPreferences.Instance.StopAnimationOnCurrentFrame = saved;
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
							SetCurrentColumn.Perform(newColumn);
							timeline.Ruler.MeasuredFrameDistance = timeline.CurrentColumn - initialCurrentColumn;
							previousColumn = newColumn;
							DockHierarchy.Instance.InvalidateWindows();
							yield return null;
						}
						Document.Current.History.CommitTransaction();
						timeline.Ruler.MeasuredFrameDistance = 0;
						SetCurrentColumn.Processor.CacheAnimationsStates = false;
					}
				}
				yield return null;
			}
		}

		void ShiftTimeline(int destColumn)
		{
			var delta = destColumn - timeline.CurrentColumn;
			if (delta > 0) {
				TimelineHorizontalShift.Perform(timeline.CurrentColumn, delta);
			} else if (delta < 0) {
				foreach (var node in Document.Current.Container.Nodes) {
					foreach (var animator in node.Animators.Where(i => i.AnimationId == Document.Current.AnimationId).ToList()) {
						RemoveKeyframeRange.Perform(animator, destColumn, timeline.CurrentColumn - 1);
					}
				}
				foreach (var marker in Document.Current.Animation.Markers.Where(m => m.Frame >= destColumn && m.Frame < timeline.CurrentColumn).ToList()) {
					DeleteMarker.Perform(marker, removeDependencies: false);
				}
				TimelineHorizontalShift.Perform(destColumn, delta);
			}
		}

		void DragMarker(Marker marker, int destColumn)
		{
			var markerToRemove = Document.Current.Animation.Markers.FirstOrDefault(m => m.Frame == destColumn);
			if (marker.Frame != destColumn && markerToRemove != null) {
				DeleteMarker.Perform(markerToRemove, false);
			}
			// Delete and add marker again, because we want to maintain the markers order.
			DeleteMarker.Perform(marker, false);
			SetProperty.Perform(marker, "Frame", destColumn);
			SetMarker.Perform(marker, true);
		}

		public static int CalcColumn(float mouseX)
		{
			return ((mouseX + timeline.Offset.X) / TimelineMetrics.ColWidth).Floor().Max(0);
		}
	}
}
