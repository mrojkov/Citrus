using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Timeline
{
	class RollMouseScrollProcessor : Core.ITaskProvider
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Task()
		{
			var input = timeline.Roll.RootWidget.Input;
			bool wasPressed = false; 
			while (true) {
				if (input.IsMousePressed()) {
					// To allow click on EnterButton when row is partly visible.
					if (!wasPressed) {
						yield return 0.2;
					}
					var s = TimelineMetrics.DefaultRowHeight;
					if (input.LocalMousePosition.Y > timeline.Roll.RootWidget.Height - s / 2) {
						timeline.OffsetY += s;
					} else if (input.LocalMousePosition.Y < s / 2) {
						timeline.OffsetY -= s;
					}
					Window.Current.Invalidate();
					wasPressed = true;
				} else {
					wasPressed = false;
				}
				yield return 0.1f;
			}
		}
	}	
}
