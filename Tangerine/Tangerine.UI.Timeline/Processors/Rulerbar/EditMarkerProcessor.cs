using System;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class EditMarkerProcessor : IProcessor
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			var input = timeline.Ruler.RootWidget.Input;
			while (true) {
				if (input.WasKeyReleased(Key.Mouse0DoubleClick) && timeline.Ruler.RootWidget.IsMouseOver()) {
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
