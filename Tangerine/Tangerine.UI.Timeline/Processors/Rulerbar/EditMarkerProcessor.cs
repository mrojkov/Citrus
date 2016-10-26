using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline
{
	public class EditMarkerProcessor : ITaskProvider
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			var input = timeline.Ruler.RootWidget.Input;
			while (true) {
				if (input.WasKeyReleased(Key.Mouse0DoubleClick)) {
					var marker = timeline.Container.Markers.FirstOrDefault(i => i.Frame == timeline.CurrentColumn) ?? new Marker { Frame = timeline.CurrentColumn };
					var dlg = new MarkerPropertiesDialog(marker);
					var result = dlg.Show();
					if (result != null) {
						SetMarker(result);
					}
				}
				yield return null;
			}
		}

		void SetMarker(Marker marker)
		{
			Core.Operations.SetMarker.Perform(timeline.Container.DefaultAnimation.Markers, marker);
		}
	}
}
