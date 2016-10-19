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
							var cw = TimelineMetrics.ColWidth;
							var mp = input.LocalMousePosition.X;
							if (mp > rulerWidget.Width - cw / 2) {
								timeline.ScrollPos.X += cw;
							} else if (mp < cw / 2) {
								timeline.ScrollPos.X -= cw;
							}
							timeline.CurrentColumn = ((mp + timeline.ScrollPos.X) / cw).Floor().Max(0);
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
