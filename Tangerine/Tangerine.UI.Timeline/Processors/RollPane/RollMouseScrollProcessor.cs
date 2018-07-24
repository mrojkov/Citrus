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
			var rollWidget = timeline.Roll.RootWidget;
			bool wasPressed = false; 
			while (true) {
				if (rollWidget.Input.IsMousePressed()) {
					// To allow click on EnterButton when row is partly visible.
					if (!wasPressed) {
						yield return 0.2;
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
