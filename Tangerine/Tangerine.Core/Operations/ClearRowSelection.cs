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
			foreach (var i in Document.Current.SelectedRows.ToList()) {
				SelectRow.Perform(i, false);
			}
		}
	}
}