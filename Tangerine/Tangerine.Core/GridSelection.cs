using System;
using System.Linq;
using Lime;
using Tangerine.Core.Components;

namespace Tangerine.Core
{
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

		public static bool GetSelectionBoundaries(out IntRectangle result)
		{
			result = new IntRectangle();
			var rows = Document.Current.SelectedRows().ToList();
			if (rows.Count == 0) {
				return false;
			}
			var span = SingleSpan(
				rows[0].Components.Get<GridSpanListComponent>()
				?.Spans.GetNonOverlappedSpans());
			var index = rows[0].Index;
			if (span == null) {
				return false;
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
					return false;
				}
				span = newSpan;
			}
			result = new IntRectangle {
				Left = Math.Max(span.Value.A, 0),
				Right = span.Value.B,
				Top = rows[0].Index,
				Bottom = index
			};
			return true;
		}
	}
}
