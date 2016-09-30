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
				var widget = timeline.PanelWidget;
				var wheelDown = widget.Input.WasKeyPressed(Key.MouseWheelDown);
				var wheelUp = widget.Input.WasKeyPressed(Key.MouseWheelUp);
				if (wheelDown) {
					timeline.ScrollOrigin.Y += Metrics.DefaultRowHeight;
				}
				if (wheelUp) {
					timeline.ScrollOrigin.Y -= Metrics.DefaultRowHeight;
				}
				yield return null;
			}
		}
	}
}

