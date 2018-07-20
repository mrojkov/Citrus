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

		public GridSpan(int a, int b)
		{
			A = a;
			B = b;
		}

		public bool Contains(int cell) => cell >= A && cell < B;
	}

	public class GridSpanList : List<GridSpan>
	{
		public GridSpanList GetNonOverlappedSpans()
		{
			var result = new GridSpanList();
			if (Count == 0) {
				return result;
			}
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
			foreach (var s in this) {
				if (s.Contains(cell)) {
					return true;
				}
			}
			return false;
		}

		public void DeselectGridSpan(GridSpan span)
		{
			if (span.B - span.A != 1) {
				throw new NotSupportedException("Deselection of areas, longer than 1, is not supported");
			} 
			var result = new GridSpanList();
			foreach (var s in this) {
				if (s.B < span.A || s.A >= span.B) {
					result.Add(s);
					continue;
				}
				if (s.A <= span.A) {
					result.Add(s.A, span.A);
				}
				if (s.B >= span.B) {
					result.Add(span.B, s.B);
				}
			}
			Clear();
			AddRange(result);
		}

		private void Add(int a, int b)
		{
			if (a < b) {
				Add(new GridSpan(a, b));
			}
		}

		public void UndoDeselectGridSpan(GridSpan span)
		{
			Add(span);
			var temp = GetNonOverlappedSpans();
			Clear();
			AddRange(temp);
		}
	}

	public sealed class GridSpanListComponent : Component
	{
		public readonly GridSpanList Spans;

		public GridSpanListComponent() { Spans = new GridSpanList(); }
		public GridSpanListComponent(GridSpanList spans) { Spans = spans; }
	}
}
