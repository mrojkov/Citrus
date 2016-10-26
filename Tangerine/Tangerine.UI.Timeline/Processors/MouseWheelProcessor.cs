using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Timeline
{
	public class MouseWheelProcessor : Core.ITaskProvider
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			while (true) {
				HandleScroll(timeline.Roll.RootWidget.Input);
				HandleScroll(timeline.Grid.RootWidget.Input);
				HandleScroll(timeline.Overview.RootWidget.Input);
				yield return null;
			}
		}

		void HandleScroll(WidgetInput input)
		{
			if (input.WasKeyPressed(Key.MouseWheelDown)) {
				timeline.ScrollPos.Y += TimelineMetrics.DefaultRowHeight;
			}
			if (input.WasKeyPressed(Key.MouseWheelUp)) {
				timeline.ScrollPos.Y -= TimelineMetrics.DefaultRowHeight;
			}
		}
	}
}

