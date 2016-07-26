using System;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public static class SelectRowRange
	{
		public static void Perform(Row startRow, Row endRow)
		{
			if (endRow.Index >= startRow.Index) {
				for (int i = startRow.Index; i <= endRow.Index; i++) {
					SelectRow.Perform(Timeline.Instance.Rows[i]);
				}
			} else {
				for (int i = startRow.Index; i >= endRow.Index; i--) {
					SelectRow.Perform(Timeline.Instance.Rows[i]);
				}
			}
		}
	}
}

