using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Operations
{
	public class SelectGridSpan : IOperation
	{
		Timeline timeline => Timeline.Instance;
		readonly GridSpan span;
		readonly int row;

		public bool IsChangingDocument => false;
		public DateTime Timestamp { get; set; }

		public static void Perform(int row, GridSpan span)
		{
			Document.Current.History.Perform(new SelectGridSpan(row, span));
		}

		private SelectGridSpan(int row, GridSpan span)
		{
			this.row = row;
			this.span = span;
		}

		public void Do()
		{
			Document.Current.Rows[row].Components.GetOrAdd<GridSpanList>().Add(span);
		}

		public void Undo()
		{
			Document.Current.Rows[row].Components.GetOrAdd<GridSpanList>().Remove(span);
		}
	}
}