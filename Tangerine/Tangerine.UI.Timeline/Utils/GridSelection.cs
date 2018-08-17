using System;
using System.Linq;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public struct Boundaries
	{
		public int Top;
		public int Bottom;
		public int Left;
		public int Right;
	}

	public static class GridSelection
	{
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

		public static Boundaries? GetSelectionBoundaries()
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
