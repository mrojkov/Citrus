using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.Core.Operations
{
	public class SelectRow : IOperation
	{
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
			var sr = Document.Current.SelectedRows;
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
			var sr = Document.Current.SelectedRows;
			if (select) {
				System.Diagnostics.Debug.Assert(sr[0] == row);
				sr.RemoveAt(0);
			} else if (lastIndex >= 0) {
				sr.Insert(lastIndex, row);
			}
		}
	}

	public class SelectNode
	{
		public static void Perform(Node node, bool select = true)
		{
			var row = Document.Current.GetRowById(node.EditorState().Uid);
			SelectRow.Perform(row, select);
		}
	}
}