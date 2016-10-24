using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Operations
{
	public class ClearGridSelection : IOperation
	{
		public bool IsChangingDocument => false;
		public DateTime Timestamp { get; set; }
		List<GridSpanList> savedSpans;

		public static void Perform()
		{
			Document.Current.History.Perform(new ClearGridSelection());
		}

		private ClearGridSelection() {}

		public void Do()
		{
			savedSpans = Document.Current.Rows.Select(r => r.Components.GetOrAdd<GridSpanList>()).ToList();
			foreach (var row in Document.Current.Rows) {
				row.Components.Remove<GridSpanList>();
			}
		}

		public void Undo()
		{
			foreach (var row in Document.Current.Rows) {
				row.Components.Remove<GridSpanList>();
				row.Components.Add(savedSpans[row.Index]);
			}
			savedSpans = null;
		}
	}
}