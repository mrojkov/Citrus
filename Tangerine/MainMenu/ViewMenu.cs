using System;
using Lime;
using Tangerine.Core;
using Tangerine.UI;

namespace Tangerine
{
	public class ViewDefaultLayout : CommandHandler
	{
		public override void Execute()
		{
			DockManager.Instance.ImportState(TangerineApp.Instance.DockManagerInitialState, resizeMainWindow: false);
		}
	}

	public class DeleteRulers : DocumentCommandHandler
	{
		public override bool GetEnabled()
		{
			return Project.Current.Rulers.Count > 0;
		}

		public override void Execute()
		{
			new DeleteRulerDialog();
		}
	}
}
