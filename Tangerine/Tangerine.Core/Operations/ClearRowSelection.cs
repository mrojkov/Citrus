using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.Core.Operations
{
	public static class ClearRowSelection
	{
		public static void Perform()
		{
			var rows = Document.Current.Rows.ToList();
			// Use temporary row list to avoid 'Collection was modified' exception
			foreach (var row in rows) {
				if (row.Selected) {
					SelectRow.Perform(row, false);
				}
			}
		}
	}
}
