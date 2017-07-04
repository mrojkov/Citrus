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

	public class OverlaysCommand : CommandHandler
	{
		public override void Execute()
		{
			UserPreferences.Instance.SceneViewUserPreferences.ShowOverlays = !UserPreferences.Instance.SceneViewUserPreferences.ShowOverlays;
			Window.Current.Invalidate();
		}
	}
}
