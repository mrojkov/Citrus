using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{	
	class GridMouseScrollTask
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Main()
		{
			var input = timeline.Grid.RootWidget.Input;
			while (true) {
				if (input.IsMouseOwner()) {
					var rect = timeline.Grid.RootWidget.CalcAABBInSpaceOf(timeline.RootWidget);
					if (input.MousePosition.X > rect.B.X) {
						timeline.ScrollOrigin.X += Metrics.ColWidth;
					} else if (input.MousePosition.X < rect.A.X) {
						timeline.ScrollOrigin.X -= Metrics.ColWidth;
					} else if (input.MousePosition.Y > rect.B.Y) {
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
