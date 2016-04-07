using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{	
	public class GridSelection : List<IntRectangle>
	{
		public GridSelection() { }

		public GridSelection(GridSelection origin)
		{
			AddRange(origin);
		}

		public GridSelection GetNonOverlappedRects()
		{
			var result = new GridSelection();
			var hSpans = new SortedSet<int>();
			var vSpans = new SortedSet<int>();
			foreach (var r in this) {
				hSpans.Add(r.A.X);
				hSpans.Add(r.B.X);
				vSpans.Add(r.A.Y);
				vSpans.Add(r.B.Y);
			}
			int? prevRow = null;
			foreach (int row in vSpans) {
				int? prevCol = null;
				foreach (int col in hSpans) {
					if (prevRow.HasValue && prevCol.HasValue) {
						var cell = new IntVector2((col + prevCol.Value) / 2, (row + prevRow.Value) / 2);
						if (IsCellSelected(cell)) {
							result.Add(new IntRectangle(prevCol.Value, prevRow.Value, col, row));
						}
					}
					prevCol = col;
				}
				prevRow = row;
			}
			return result;
		}

		public bool IsCellSelected(IntVector2 cell)
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
