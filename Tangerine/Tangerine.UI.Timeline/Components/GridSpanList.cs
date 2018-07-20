using System;
using System.Collections.Generic;
using System.Linq;
using Tangerine.Core;
using Lime;

namespace Tangerine.UI.Timeline.Components
{
	public struct GridSpan
	{
		public int A;
		public int B;
		public bool Inclusive;

		public GridSpan(int a, int b, bool inclusive)
		{
			A = a;
			B = b;
			Inclusive = inclusive;
		}

		public bool Contains(int cell) => cell >= A && cell < B;
	}

	public class GridSpanList : List<GridSpan>
	{
		public GridSpanList GetNonOverlappedSpans()
		{
			if (Count == 0) {
				return this;
			}
			var result = new GridSpanList();
			foreach (var span in this.OrderBy(s => s.A)) {
				int last = result.Count - 1;
				if (
					result.Count > 0 &&
					result[last].B >= span.A
				) {
					result[last] = new GridSpan {
						A = result[last].A,
						B = Math.Max(result[last].B, span.B)
					};
				} else {
					result.Add(span);
				}
			}
			return result;
		}

		public bool IsCellSelected(int cell)
		{
			var r = false;
			foreach (var s in this) {
				if (s.Contains(cell)) {
					r = s.Inclusive;
				}
			}
			return r;
		}
	}

	public sealed class GridSpanListComponent : Component
	{
		public readonly GridSpanList Spans;

		public GridSpanListComponent() { Spans = new GridSpanList(); }
		public GridSpanListComponent(GridSpanList spans) { Spans = spans; }
	}
}
