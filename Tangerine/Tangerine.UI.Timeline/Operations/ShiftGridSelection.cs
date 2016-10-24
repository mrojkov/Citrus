using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Operations
{
	public class ShiftGridSelection : IOperation
	{
		public bool IsChangingDocument => false;
		public DateTime Timestamp { get; set; }

		readonly IntVector2 offset;
		List<GridSpanList> savedSpans;

		public static void Perform(IntVector2 offset)
		{
			Document.Current.History.Perform(new ShiftGridSelection(offset));
		}

		ShiftGridSelection(IntVector2 offset)
		{
			this.offset = offset;
		}

		public void Do()
		{
			ShiftX(offset.X);
			ShiftY();
		}

		public void Undo()
		{
			UnshiftY();
			ShiftX(-offset.X);
		}

		void ShiftX(int offset)
		{
			foreach (var row in Document.Current.Rows) {
				var spans = row.Components.GetOrAdd<GridSpanList>();
				for (int i = 0; i < spans.Count; i++) {
					var s = spans[i];
					s.A += offset;
					s.B += offset;
					spans[i] = s;
				}
			}
		}

		void ShiftY()
		{
			savedSpans = Document.Current.Rows.Select(r => r.Components.GetOrAdd<GridSpanList>()).ToList();
			if (offset.Y != 0) {
				foreach (var row in Document.Current.Rows) {
					var i = row.Index - offset.Y;
					row.Components.Remove<GridSpanList>();
					row.Components.Add(i >= 0 && i < Document.Current.Rows.Count ? savedSpans[i] : new GridSpanList());
				}
			}
		}

		void UnshiftY()
		{
			foreach (var row in Document.Current.Rows) {
				row.Components.Remove<GridSpanList>();
				row.Components.Add(savedSpans[row.Index]);
			}
			savedSpans = null;
		}
	}
}