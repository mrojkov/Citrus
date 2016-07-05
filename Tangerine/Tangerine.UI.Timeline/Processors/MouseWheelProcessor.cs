using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Timeline
{
	public class MouseWheelProcessor : Core.IProcessor
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				var widget = timeline.RootWidget;
				var wheelDown = widget.Input.WasKeyPressed(Key.MouseWheelDown);
				var wheelUp = widget.Input.WasKeyPressed(Key.MouseWheelUp);
				if (wheelDown) {
					timeline.ScrollOrigin.Y += Metrics.TimelineDefaultRowHeight;
				}
				if (wheelUp) {
					timeline.ScrollOrigin.Y -= Metrics.TimelineDefaultRowHeight;
				}
				yield return null;
			}
		}
	}
}

