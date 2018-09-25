using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Timeline
{	
	class GridMouseScrollProcessor : Core.ITaskProvider
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Task()
		{
			var widget = timeline.Grid.RootWidget;
			while (true) {
				if (widget.Input.IsMousePressed()) {
					yield return null;
					var cw = TimelineMetrics.ColWidth;
					var p = widget.LocalMousePosition();
					if (p.X > widget.Width - cw / 2) {
						timeline.OffsetX += cw;
					} else if (p.X < cw / 2) {
						timeline.OffsetX = Math.Max(0, timeline.OffsetX - cw);
					}
					Core.Document.Current.History.DoTransaction(() => {
						Operations.SetCurrentColumn.Perform(RulerbarMouseScrollProcessor.CalcColumn(p.X));
					});
					var rh = TimelineMetrics.DefaultRowHeight;
					if (p.Y > widget.Height - rh / 2) {
						timeline.OffsetY += rh;
					} else if (p.Y < rh / 2) {
						timeline.OffsetY -= rh;
					}
					Window.Current.Invalidate();
				}
				yield return null;
			}
		}
	}	
}
