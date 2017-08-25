using System;
using System.Collections.Generic;
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
			var hSpans = new SortedSet<int>();
			foreach (var r in this) {
				hSpans.Add(r.A);
				hSpans.Add(r.B);
			}
			int? prevCol = null;
			foreach (int col in hSpans) {
				if (prevCol.HasValue) {
					var cell = (col + prevCol.Value) / 2;
					if (IsCellSelected(cell)) {
						result.Add(new GridSpan(prevCol.Value, col, true));
					}
				}
				prevCol = col;
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
