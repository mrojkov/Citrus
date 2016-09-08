using System;
using Tangerine.Core;

namespace Tangerine.Core.Operations
{
	public static class SelectRowRange
	{
		public static void Perform(Row startRow, Row endRow)
		{
			if (endRow.Index >= startRow.Index) {
				for (int i = startRow.Index; i <= endRow.Index; i++) {
					SelectRow.Perform(Document.Current.Rows[i]);
				}
			} else {
				for (int i = startRow.Index; i >= endRow.Index; i--) {
					SelectRow.Perform(Document.Current.Rows[i]);
				}
			}
		}
	}
}

