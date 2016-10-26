using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class RulerMouseScrollProcessor : ITaskProvider
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Task()
		{
			var rulerWidget = timeline.Ruler.RootWidget;
			var input = rulerWidget.Input;
			while (true) {
				if (input.WasMousePressed()) {
					input.CaptureMouse();
					try {
						int initialCol = CalcColumn(input.LocalMousePosition.X);
						while (input.IsMousePressed()) {
							var cw = TimelineMetrics.ColWidth;
							var mp = input.LocalMousePosition.X;
							if (mp > rulerWidget.Width - cw / 2) {
								timeline.ScrollPos.X += cw;
							} else if (mp < cw / 2) {
								timeline.ScrollPos.X -= cw;
							}
							Operations.SetCurrentColumn.Perform(CalcColumn(mp));
							timeline.Ruler.MeasuredFrameDistance = timeline.CurrentColumn - initialCol;
							Window.Current.Invalidate();
							yield return null;
						}
					} finally {
						timeline.Ruler.MeasuredFrameDistance = 0;
						input.ReleaseMouse();
					}
				}
				yield return null;
			}
		}

		int CalcColumn(float mouseX)
		{
			return ((mouseX + timeline.ScrollPos.X) / TimelineMetrics.ColWidth).Floor().Max(0);
		}
	}	
}
