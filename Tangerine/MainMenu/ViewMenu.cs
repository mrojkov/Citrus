using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class DefaultLayoutCommand : Command
	{
		public DefaultLayoutCommand()
		{
			Text = "Default Layout";
		}

		public override void Execute()
		{
			TangerineApp.Instance.DockManager.ImportState(TangerineApp.Instance.DockManagerInitialState, resizeMainWindow: false);
		}
	}
}
