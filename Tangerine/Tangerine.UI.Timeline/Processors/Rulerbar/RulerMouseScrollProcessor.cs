using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class RulerMouseScrollProcessor : IProcessor
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			var rulerWidget = timeline.Ruler.RootWidget;
			var input = rulerWidget.Input;
			while (true) {
				if (input.WasMousePressed()) {
					input.CaptureMouse();
					var initialColumn = timeline.CurrentColumn;
					try {
						while (input.IsMousePressed()) {
							var colWidth = TimelineMetrics.ColWidth;
							var x = (input.MousePosition.X - rulerWidget.GlobalPosition.X).Clamp(-colWidth, rulerWidget.Width + colWidth);
							timeline.CurrentColumn = ((x + timeline.ScrollOrigin.X) / colWidth).Floor().Max(0);
							timeline.EnsureColumnVisible(timeline.CurrentColumn);
							Window.Current.Invalidate();
							yield return null;
						}
					} finally {
						input.ReleaseMouse();
					}
					var currentColumn = timeline.CurrentColumn;
					timeline.CurrentColumn = initialColumn;
					Operations.SetCurrentColumn.Perform(currentColumn);
				}
				yield return null;
			}
		}
	}	
}
