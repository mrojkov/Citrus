using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Tangerine.UI.Timeline.Components;
using Tangerine.UI.Timeline.Operations;

namespace Tangerine.UI.Timeline
{
	public class AnimationStretchProcessor : ITaskProvider
	{
		private Timeline timeline => Timeline.Instance;
		private GridPane grid => Timeline.Instance.Grid;
		private WidgetInput input => grid.RootWidget.Input;
		private Dictionary<IKeyframe, double> savedPositions = new Dictionary<IKeyframe, double>();
		private Dictionary<IAnimator, List<IKeyframe>> savedKeyframes = new Dictionary<IAnimator, List<IKeyframe>>();
		private Dictionary<Marker, double> savedMarkerPositions = new Dictionary<Marker, double>();
		private List<Marker> savedMarkers = new List<Marker>();

		public IEnumerator<object> Task() {
			while (true) {
				if (!TimelineUserPreferences.Instance.AnimationStretchMode) {
					yield return null;
					continue;
				}
				if (!GridSelection.GetSelectionBoundaries(out var boundaries) || boundaries.Right - boundaries.Left < 2) {
					yield return null;
					continue;
				}
				var topLeft = grid.CellToGridCoordinates(boundaries.Top, boundaries.Left);
				var bottomRight = grid.CellToGridCoordinates(boundaries.Bottom + 1, boundaries.Right);
				var mousePosition = grid.ContentWidget.LocalMousePosition();
				if (mousePosition.Y < topLeft.Y || mousePosition.Y > bottomRight.Y) {
					yield return null;
					continue;
				}
				if (mousePosition.X - topLeft.X < 0 && mousePosition.X - topLeft.X > -10) {
					Utils.ChangeCursorIfDefault(MouseCursor.SizeWE);
					if (input.ConsumeKeyPress(Key.Mouse0)) {
						yield return Drag(boundaries, DragSide.Left);
					}
				} else if (mousePosition.X - bottomRight.X > 0 && mousePosition.X - bottomRight.X < 10) {
					Utils.ChangeCursorIfDefault(MouseCursor.SizeWE);
					if (input.ConsumeKeyPress(Key.Mouse0)) {
						yield return Drag(boundaries, DragSide.Right);
					}
				}
				yield return null;
			}
		}

		private enum DragSide { Left, Right }

		private IEnumerator<object> Drag(IntRectangle boundaries, DragSide side)
		{
			IntVector2? last = null;
			Save(boundaries);
			bool isStretchingMarkers = input.IsKeyPressed(Key.Control);
			using (Document.Current.History.BeginTransaction()) {
				while (input.IsMousePressed()) {
					Utils.ChangeCursorIfDefault(MouseCursor.SizeWE);
					var current = grid.CellUnderMouse();
					if (current == last) {
						yield return null;
						continue;
					}
					Document.Current.History.RollbackTransaction();
					current.X = Math.Max(current.X, 0);
					if (side == DragSide.Left) {
						current.X = Math.Min(current.X, boundaries.Right - 1);
					} else {
						current.X = Math.Max(current.X, boundaries.Left + 1);
					}
					Stretch(boundaries, side, current.X, stretchMarkers: isStretchingMarkers);
					if (side == DragSide.Left) {
						boundaries.Left = current.X;
					} else {
						boundaries.Right = current.X;
					}
					ClearGridSelection.Perform();
					for (int i = boundaries.Top; i <= boundaries.Bottom; ++i) {
						SelectGridSpan.Perform(i, boundaries.Left, boundaries.Right);
					}
					SetCurrentColumn.Perform(boundaries.Right);
					last = current;
					yield return null;
				}
				Document.Current.History.CommitTransaction();
			}
			yield return null;
		}

		private void Stretch(IntRectangle boundaries, DragSide side, int newPos, bool stretchMarkers)
		{
			int length;
			if (side == DragSide.Left) {
				length = boundaries.Right - newPos - 1;
			} else {
				length = newPos - boundaries.Left - 1;
			}
			int oldLength = boundaries.Right - boundaries.Left - 1;
			for (int i = boundaries.Top; i <= boundaries.Bottom; ++i) {
				if (!(Document.Current.Rows[i].Components.Get<NodeRow>()?.Node is IAnimationHost animable)) {
					continue;
				}
				foreach (var animator in animable.Animators.ToList()) {
					IEnumerable<IKeyframe> saved = savedKeyframes[animator];
					if (
						side == DragSide.Left && length < oldLength ||
						side == DragSide.Right && length > oldLength
					) {
						saved = saved.Reverse();
					}
					foreach (var key in saved) {
						RemoveKeyframe.Perform(animator, key.Frame);
					}
					foreach (var key in saved) {
						double relpos = savedPositions[key];
						int newFrame;
						if (side == DragSide.Left) {
							newFrame = (int)Math.Round(newPos + relpos * length);
						} else {
							newFrame = (int)Math.Round(boundaries.Left + relpos * length);
						}
						var newKey = key.Clone();
						newKey.Frame = newFrame;
						SetAnimableProperty.Perform(
							animable, animator.TargetPropertyPath, newKey.Value,
							createAnimatorIfNeeded: true,
							createInitialKeyframeForNewAnimator: false,
							newKey.Frame
						);
						SetKeyframe.Perform(animable, animator.TargetPropertyPath, Document.Current.AnimationId, newKey);
					}
				}
			}
			if (stretchMarkers) {
				foreach (var marker in savedMarkers) {
					DeleteMarker.Perform(marker, removeDependencies: false);
				}
				foreach (var marker in savedMarkers) {
					double relpos = savedMarkerPositions[marker];
					int newFrame;
					if (side == DragSide.Left) {
						newFrame = (int)Math.Round(newPos + relpos * length);
					} else {
						newFrame = (int)Math.Round(boundaries.Left + relpos * length);
					}
					var newMarker = marker.Clone();
					newMarker.Frame = newFrame;
					SetMarker.Perform(newMarker, removeDependencies: false);
				}
			}
		}

		private void Save(IntRectangle boundaries)
		{
			savedPositions.Clear();
			savedKeyframes.Clear();
			savedMarkerPositions.Clear();
			savedMarkers.Clear();
			var length = boundaries.Right - boundaries.Left - 1;
			for (int i = boundaries.Top; i <= boundaries.Bottom; ++i) {
				if (!(Document.Current.Rows[i].Components.Get<NodeRow>()?.Node is IAnimationHost animable)) {
					continue;
				}
				foreach (var animator in animable.Animators) {
					savedKeyframes.Add(animator, new List<IKeyframe>());
					var keys = animator.Keys.Where(k =>
						boundaries.Left <= k.Frame &&
						k.Frame < boundaries.Right
					);
					foreach (var key in keys) {
						savedPositions.Add(key, ((double)key.Frame - boundaries.Left) / length);
						savedKeyframes[animator].Add(key);
					}
				}
			}
			var markers = Document.Current.Animation.Markers.Where(k =>
				boundaries.Left <= k.Frame &&
				k.Frame < boundaries.Right);
			foreach (var marker in markers) {
				savedMarkerPositions.Add(marker, ((double)marker.Frame - boundaries.Left) / length);
				savedMarkers.Add(marker);
			}
		}
	}
}
