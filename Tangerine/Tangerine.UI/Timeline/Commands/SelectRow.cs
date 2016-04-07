using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Commands
{
	public class SelectRow : ICommand
	{
		Timeline timeline => Timeline.Instance;

		private int lastIndex;
		readonly Row row;
		readonly bool select;

		public SelectRow(Row row, bool select = true)
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

	public class SelectRowRange : CompoundCommand
	{
		public SelectRowRange(Row startRow, Row endRow)
		{
			if (endRow.Index >= startRow.Index) {
				for (int i = startRow.Index; i <= endRow.Index; i++) {
					Add(new SelectRow(Timeline.Instance.Rows[i]));
				}
			} else {
				for (int i = startRow.Index; i >= endRow.Index; i--) {
					Add(new SelectRow(Timeline.Instance.Rows[i]));
				}
			}
		}
	}
}