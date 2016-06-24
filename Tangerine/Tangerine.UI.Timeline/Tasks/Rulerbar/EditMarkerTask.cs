using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class EditMarkerTask
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Main()
		{
			var input = timeline.Ruler.Widget.Input;
			while (true) {
				if (input.WasMouseReleased()) {
					var x = input.MousePosition.X - timeline.Ruler.Widget.GlobalPosition.X - timeline.ScrollOrigin.X;
					var frame = (x / Metrics.TimelineColWidth).Floor();
					Document.Current.History.Execute(new Operations.SetCurrentColumn(frame));
				}
				if (input.WasKeyReleased(Key.Mouse0DoubleClick)) {
					foreach (var marker in timeline.Container.Markers) {
						if (marker.Frame == timeline.CurrentColumn) {
							new MarkerPropertiesDialog(marker);
						}
					}
				}
				yield return null;
			}
		}
	}
}
