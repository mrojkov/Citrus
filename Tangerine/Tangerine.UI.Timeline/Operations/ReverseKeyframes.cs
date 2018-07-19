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
			var test = GetTimelineBoundaries();
			var Boundaries = GetSelectionBoundaries();
			if (Boundaries == null) {
				return;
			}
			for (int i = Boundaries.Value.Top; i <= Boundaries.Value.Bottom; ++i) {
				var animable =
					Document.Current.Rows[i].Components.Get<NodeRow>()?.Node as IAnimable;
				if (animable == null) {
					continue;
				}
				Document.Current.History.DoTransaction(() => {
					foreach (var animator in animable.Animators) {
						var saved = animator.Keys.Where(k =>
							Boundaries.Value.Left <= k.Frame &&
							k.Frame < Boundaries.Value.Right).ToList();
						foreach (var key in saved) {
							RemoveKeyframe.Perform(animator, key.Frame);
						}
						foreach (var key in saved) {
							SetProperty.Perform(key, "Frame", Boundaries.Value.Left + Boundaries.Value.Right - key.Frame - 1);
							SetKeyframe.Perform(animable, animator.TargetProperty, animator.AnimationId, key);
						}
					}
				});
			}
		}

		private static Boundaries? GetTimelineBoundaries()
		{
			int left = 0;
			int right = 0;
			int top = 0;
			int bottom = Document.Current.Rows.Last().Index;
			foreach (var row in Document.Current.Rows) {
				var animable = row.Components.Get<NodeRow>()?.Node as IAnimable;
				if (animable == null) {
					continue;
				}
				foreach (var animator in animable.Animators) {
					foreach (var keyframe in animator.Keys) {
						right = Math.Max(keyframe.Frame, right);
					}
				}
			}
			return new Boundaries {
				Left = left,
				Right = right,
				Top = top,
				Bottom = bottom
			};
		}

		private static GridSpan? UniteSpans(GridSpanList spans)
		{
			if (!spans.Any()) {
				return null;
			}
			var current = spans.First();
			foreach (var span in spans.Skip(1)) {
				if (current.B == span.A) {
					current.B = span.B;
				}
				else {
					return null;
				}
			}
			return current;
		}

		private static Boundaries? GetSelectionBoundaries()
		{
			if (!Document.Current.SelectedRows().Any()) {
				return GetTimelineBoundaries();
			}
			int prevRowIndex = Document.Current.SelectedRows().First().Index;
			var spans = Document.Current.Rows[prevRowIndex].Components.Get<GridSpanListComponent>()?.Spans.GetNonOverlappedSpans();
			var unitedSpan = UniteSpans(spans);
			if (unitedSpan == null) {
				return null;
			}
			var span = (GridSpan)unitedSpan;
			int left = span.A;
			int right = span.B;
			foreach (var row in Document.Current.SelectedRows().Skip(1)) {
				if (row.Index != prevRowIndex + 1) {
					return null;
				}
				prevRowIndex = row.Index;
				spans = Document.Current.Rows[row.Index].Components.Get<GridSpanListComponent>()?.Spans.GetNonOverlappedSpans();
				unitedSpan = UniteSpans(spans);
				if (
					unitedSpan == null ||
					unitedSpan?.A != left ||
					unitedSpan?.B != right
				) {
					return null;
				}
			}
			return new Boundaries {
				Left = left,
				Right = right,
				Top = Document.Current.SelectedRows().First().Index,
				Bottom = prevRowIndex
			};
		}
	}
}
