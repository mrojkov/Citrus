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
			Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().ShowOverlays = !Core.UserPreferences.Instance.Get<UI.SceneView.UserPreferences>().ShowOverlays;
			Window.Current.Invalidate();
		}
	}
}
