using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public class SelectRow : IOperation
	{
		Timeline timeline => Timeline.Instance;

		private int lastIndex;
		readonly Row row;
		readonly bool select;

		public bool IsChangingDocument => false;
		public DateTime Timestamp { get; set; }

		public static void Perform(Row row, bool select = true)
		{
			Document.Current.History.Perform(new SelectRow(row, select));
		}

		private SelectRow(Row row, bool select)
		{
			this.select = select;
			this.row = row;
		}

		public void Do()
		{
			var sr = timeline.SelectedRows;
			lastIndex = sr.IndexOf(row);
			if (lastIndex >= 0) {
				sr.RemoveAt(lastIndex);
			}
			if (select) {
				sr.Insert(0, row);
			}
		}

		public void Undo()
		{
			var sr = timeline.SelectedRows;
			if (select) {
				sr.RemoveAt(0);
			} else if (lastIndex >= 0) {
				sr.Insert(lastIndex, row);
			}
		}
	}
}