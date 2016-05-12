using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Commands
{
	public class ClearRowSelection : InteractiveCommand
	{
		public override void Do()
		{
			foreach (var i in Timeline.Instance.SelectedRows.ToList()) {
				Execute(new SelectRow(i, false));
			}
		}
	}
}