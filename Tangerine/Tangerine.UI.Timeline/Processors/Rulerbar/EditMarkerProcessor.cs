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

		public IEnumerator<object> Task()
		{
			var input = timeline.Ruler.RootWidget.Input;
			while (true) {
				if (input.WasKeyReleased(Key.Mouse0DoubleClick)) {
					var marker = Document.Current.Container.Markers.FirstOrDefault(
						i => i.Frame == timeline.CurrentColumn);
					var newMarker = marker?.Clone() ?? new Marker { Frame = timeline.CurrentColumn };
					var r = new MarkerPropertiesDialog().Show(newMarker, canDelete: marker != null);
					if (r == MarkerPropertiesDialog.Result.Ok) {
						Core.Operations.SetMarker.Perform(Document.Current.Container.DefaultAnimation.Markers, newMarker);
					} else if (r == MarkerPropertiesDialog.Result.Delete) {
						Core.Operations.DeleteMarker.Perform(Document.Current.Container.DefaultAnimation.Markers, marker);
					}
				}
				yield return null;
			}
		}
	}
}
