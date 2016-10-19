using System;
using System.Linq;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Timeline
{	
	class GridMouseScrollProcessor : Core.IProcessor
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			var widget = timeline.Grid.RootWidget;
			while (true) {
				if (widget.Input.IsMouseOwner()) {
					var s = new Vector2(TimelineMetrics.ColWidth, TimelineMetrics.DefaultRowHeight);
					var p = widget.Input.LocalMousePosition;
					if (p.X > widget.Width - s.X / 2) {
						timeline.ScrollPos.X += s.X;
					} else if (p.X < s.X / 2) {
						timeline.ScrollPos.X -= s.X;
					} else if (p.Y > widget.Height - s.Y / 2) {
						timeline.ScrollPos.Y += s.Y;
					} else if (p.Y < s.Y / 2) {
						timeline.ScrollPos.Y -= s.Y;
					}
					Window.Current.Invalidate();
				}
				yield return null;
			}
		}
	}	
}
