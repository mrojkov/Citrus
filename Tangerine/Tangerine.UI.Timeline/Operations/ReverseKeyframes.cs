using Lime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Operations
{
	public static class ReverseKeyframes
	{
		private struct Boundaries
		{
			public int Top;
			public int Bottom;
			public int Left;
			public int Right;
		}

		public static void Perform()
		{
			var Boundaries = GetSelectionBoundaries();
			if (Boundaries == null) {
				AlertDialog.Show("Can't invert animation in a non-rectangular selection. The selection must be a single rectangle.");
				return;
			}
			for (int i = Boundaries.Value.Top; i <= Boundaries.Value.Bottom; ++i) {
				var animable =
					Document.Current.Rows[i].Components.Get<NodeRow>()?.Node as IAnimationHost;
				if (animable == null) {
					continue;
				}
				Document.Current.History.DoTransaction(() => {
					foreach (var animator in animable.Animators.ToList()) {
						var saved = animator.Keys.Where(k =>
							Boundaries.Value.Left <= k.Frame &&
							k.Frame < Boundaries.Value.Right).ToList();
						foreach (var key in saved) {
							RemoveKeyframe.Perform(animator, key.Frame);
						}
						foreach (var key in saved) {
							SetProperty.Perform(key, nameof(IKeyframe.Frame), Boundaries.Value.Left + Boundaries.Value.Right - key.Frame - 1);
							SetKeyframe.Perform(animable, animator.TargetPropertyPath, animator.AnimationId, key);
						}
					}
				});
			}
		}

		private static Boundaries? GetSelectionBoundaries()
		{
			var rows = Document.Current.SelectedRows().ToList();
			if (rows.Count == 0) {
				return GetTimelineBoundaries();
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
				Left = span.Value.A,
				Right = span.Value.B,
				Top = rows[0].Index,
				Bottom = index
			};
		}

		private static GridSpan? SingleSpan(GridSpanList spans)
		{
			if (spans.Count != 1) {
				return null;
			} else {
				return spans[0];
			}
		}

		private static Boundaries? GetTimelineBoundaries()
		{
			int right = 0;
			foreach (var row in Document.Current.Rows) {
				var animable = row.Components.Get<NodeRow>()?.Node as IAnimationHost;
				if (animable == null) {
					continue;
				}
				foreach (var animator in animable.Animators) {
					foreach (var keyframe in animator.ReadonlyKeys) {
						right = Math.Max(keyframe.Frame, right);
					}
				}
			}
			return new Boundaries {
				Left = 0,
				Right = right,
				Top = 0,
				Bottom = Document.Current.Rows.Last().Index
			};
		}
	}
}
