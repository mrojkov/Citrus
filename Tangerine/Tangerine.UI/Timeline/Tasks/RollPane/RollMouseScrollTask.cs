using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Timeline
{
	class RollMouseScrollTask
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Main()
		{
			var input = timeline.Roll.RootWidget.Input;
			while (true) {
				if (input.IsMouseOwner()) {
					var rect = timeline.Roll.RootWidget.CalcAABBInSpaceOf(timeline.RootWidget);
					if (input.MousePosition.Y > rect.B.Y) {
						timeline.ScrollOrigin.Y += Metrics.DefaultRowHeight;
					} else if (input.MousePosition.Y < rect.A.Y) {
						timeline.ScrollOrigin.Y -= Metrics.DefaultRowHeight;
					}
					Window.Current.Invalidate();
				}
				yield return null;
			}
		}
	}	
}