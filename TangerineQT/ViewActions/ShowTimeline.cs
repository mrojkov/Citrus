using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qyoto;

namespace Tangerine
{
	public class ShowTimeline : Action
	{
		public static ShowTimeline Instance = new ShowTimeline();

		ShowTimeline()
		{
			Text = "Timeline";
			Shortcut = "Ctrl+L";
		}

		protected override void OnTriggered()
		{
			The.Timeline.DockWidget.Show();
			The.Timeline.DockWidget.ActivateWindow();
			The.Timeline.DockWidget.SetFocus();
		}
	}
}