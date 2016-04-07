using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Commands
{
	public class SetCurrentColumn : ICommand
	{
		readonly int previousColumn;
		int currentColumn;

		public SetCurrentColumn(int currentColumn)
		{
			previousColumn = Timeline.Instance.CurrentColumn;
			this.currentColumn = currentColumn;
		}

		public void Do()
		{
			Timeline.Instance.CurrentColumn = Math.Max(0, currentColumn);
		}

		public void Undo()
		{
			Timeline.Instance.CurrentColumn = previousColumn;
		}
	}
}