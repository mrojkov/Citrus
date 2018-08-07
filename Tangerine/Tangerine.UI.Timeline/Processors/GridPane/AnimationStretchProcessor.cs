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

		public IEnumerator<object> Task() {
			while (true) {
				if (!TimelineUserPreferences.Instance.AnimationStretchMode) {
					yield return null;
					continue;
				}
				var boundaries = GetSelectionBoundaries();
				if (boundaries == null || boundaries.Value.Right - boundaries.Value.Left < 2) {
					yield return null;
					continue;
				}
				var topLeft = grid.CellToGridCoordinates(boundaries.Value.Top, boundaries.Value.Left);
				var bottomRight = grid.CellToGridCoordinates(boundaries.Value.Bottom + 1, boundaries.Value.Right);

				if (Mathf.Abs(grid.ContentWidget.LocalMousePosition().X - topLeft.X) < 10) {
					Utils.ChangeCursorIfDefault(MouseCursor.SizeWE);
					if (input.ConsumeKeyPress(Key.Mouse0)) {
						yield return Drag(boundaries.Value, DragSide.Left);
					}
				} else if (Mathf.Abs(grid.ContentWidget.LocalMousePosition().X - bottomRight.X) < 10) {
					Utils.ChangeCursorIfDefault(MouseCursor.SizeWE);
					if (input.ConsumeKeyPress(Key.Mouse0)) {
						yield return Drag(boundaries.Value, DragSide.Right);
					}
				}
				yield return null;
			}
		}

		private enum DragSide { Left, Right }

		private IEnumerator<object> Drag(Boundaries boundaries, DragSide side)
		{
			IntVector2? last = null;
			Save(boundaries);
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
					Stretch(boundaries, side, current.X);
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

		private void Stretch(Boundaries boundaries, DragSide side, int newPos)
		{
			int length;
			if (side == DragSide.Left) {
				length = boundaries.Right - newPos - 1;
			} else {
				length = newPos - boundaries.Left - 1;
			}
			for (int i = boundaries.Top; i <= boundaries.Bottom; ++i) {
				if (!(Document.Current.Rows[i].Components.Get<NodeRow>()?.Node is IAnimable animable)) {
					continue;
				}
				foreach (var animator in animable.Animators.ToList()) {
					IEnumerable<IKeyframe> saved = savedKeyframes[animator];
					int oldLength = boundaries.Right - boundaries.Left - 1;
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
						var k1 = key.Clone();
						k1.Frame = newFrame;
						SetAnimableProperty.Perform(animable, animator.TargetProperty, k1.Value,  true, false, k1.Frame);
						SetKeyframe.Perform(animable, animator.TargetProperty, Document.Current.AnimationId, k1);
					}
				}
			}
		}

		private void Save(Boundaries boundaries)
		{
			savedPositions.Clear();
			savedKeyframes.Clear();
			var length = boundaries.Right - boundaries.Left - 1;
			for (int i = boundaries.Top; i <= boundaries.Bottom; ++i) {
				if (!(Document.Current.Rows[i].Components.Get<NodeRow>()?.Node is IAnimable animable)) {
					continue;
				}
				foreach (var animator in animable.Animators) {
					savedKeyframes.Add(animator, new List<IKeyframe>());
					foreach (
						var key in animator.Keys.Where(k =>
						boundaries.Left <= k.Frame &&
						k.Frame < boundaries.Right)
					) {
						savedPositions.Add(key, ((double)key.Frame - boundaries.Left) / length);
						savedKeyframes[animator].Add(key);
					}
				}
			}
		}

		private struct Boundaries
		{
			public int Top;
			public int Bottom;
			public int Left;
			public int Right;
		}

		private static GridSpan? SingleSpan(GridSpanList spans)
		{
			if (spans == null) {
				return null;
			}
			if (spans.Count != 1) {
				return null;
			} else {
				return spans[0];
			}
		}

		private static Boundaries? GetSelectionBoundaries()
		{
			var rows = Document.Current.SelectedRows().ToList();
			if (rows.Count == 0) {
				return null;
			}
			var span = SingleSpan(
				rows[0].Components.Get<GridSpanListComponent>()
				?.Spans.GetNonOverlappedSpans());
			var index = rows[0].Index;
			if (span == null) {
				return null;
			}
			for (int i = 1; i < rows.Count; ++i) {
				var newSpan = SingleSpan(
					rows[i].Components.Get<GridSpanListComponent>()
					?.Spans.GetNonOverlappedSpans());
				if (
					newSpan == null ||
					span?.A != newSpan?.A ||
					span?.B != newSpan?.B ||
					++index != rows[i].Index
				) {
					return null;
				}
				span = newSpan;
			}
			return new Boundaries {
				Left = Math.Max(span.Value.A, 0),
				Right = span.Value.B,
				Top = rows[0].Index,
				Bottom = index
			};
		}
	}
}
