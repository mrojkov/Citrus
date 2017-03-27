using System;
using System.Collections.Generic;
using Lime;
using Tangerine.UI.Timeline.Operations;

namespace Tangerine.UI.Timeline
{
	public class MouseWheelProcessor : Core.ITaskProvider
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Task()
		{
			while (true) {
				HandleScroll(timeline.Roll.RootWidget.Input);
				HandleScroll(timeline.Grid.RootWidget.Input);
				HandleScroll(timeline.Overview.RootWidget.Input);
				HandleCellsMagnification(timeline.Grid.RootWidget.Input);
				yield return null;
			}
		}

		void HandleScroll(WidgetInput input)
		{
			var delta = GetWheelDelta(input);
			if (delta != 0 && !input.IsKeyPressed(Key.Alt)) {
				timeline.OffsetY += delta * TimelineMetrics.DefaultRowHeight;
			}
		}

		void HandleCellsMagnification(WidgetInput input)
		{
			var delta = GetWheelDelta(input);
			if (delta != 0 && input.IsKeyPressed(Key.Alt)) {
				var prevColWidth = TimelineMetrics.ColWidth;
				TimelineMetrics.ColWidth = (TimelineMetrics.ColWidth + delta).Clamp(5, 30);
				if (prevColWidth != TimelineMetrics.ColWidth) {
					var mp = timeline.Grid.RootWidget.Input.LocalMousePosition.X + Timeline.Instance.Offset.X;
					Timeline.Instance.OffsetX += (mp / prevColWidth) * delta;
					Core.Operations.Dummy.Perform();
				}
			}
		}

		int GetWheelDelta(WidgetInput input)
		{
			if (input.WasKeyPressed(Key.MouseWheelDown)) {
				return 1;
			}
			if (input.WasKeyPressed(Key.MouseWheelUp)) {
				return -1;
			}
			return 0;
		}
	}
}

