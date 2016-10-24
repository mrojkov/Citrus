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

		public GridSpan(int a, int b)
		{
			A = a;
			B = b;
		}

		public bool Contains(int cell)
		{
			return cell >= A && cell < B;
		}
	}

	public class GridSpanList : List<GridSpan>, IComponent
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
						result.Add(new GridSpan(prevCol.Value, col));
					}
				}
				prevCol = col;
			}
			return result;
		}

		public bool IsCellSelected(int cell)
		{
			foreach (var r in this) {
				if (r.Contains(cell)) {
					return true;
				}
			}
			return false;
		}
	}
}
