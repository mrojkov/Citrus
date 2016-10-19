using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Timeline
{
	class RollMouseScrollProcessor : Core.IProcessor
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			var input = timeline.Roll.RootWidget.Input;
			while (true) {
				if (input.IsMouseOwner()) {
					var s = TimelineMetrics.DefaultRowHeight;
					if (input.LocalMousePosition.Y > timeline.Roll.RootWidget.Height - s / 2) {
						timeline.ScrollPos.Y += s;
					} else if (input.LocalMousePosition.Y < s / 2) {
						timeline.ScrollPos.Y -= s;
					}
					Application.InvalidateWindows();
				}
				yield return 0.1f;
			}
		}
	}	
}