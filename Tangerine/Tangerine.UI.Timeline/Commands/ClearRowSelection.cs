using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public static class ClearRowSelection
	{
		public static void Perform()
		{
			foreach (var i in Timeline.Instance.SelectedRows.ToList()) {
				SelectRow.Perform(i, false);
			}
		}
	}
}