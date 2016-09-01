using System;
using System.Collections.Generic;
using Lime;

namespace Tangerine.UI.Timeline
{
	public class OverviewScrollProcessor : Core.IProcessor
	{
		private Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Loop()
		{
			var input = timeline.Overview.RootWidget.Input;
			while (true) {
				if (input.WasMousePressed()) {
					input.CaptureMouse();
					var originalMousePosition = input.MousePosition;
					var scrollOrigin = timeline.ScrollOrigin;
					while (input.IsMousePressed()) {
						var mouseDelta = input.MousePosition - originalMousePosition;
						var scrollDelta = Vector2.Round(mouseDelta / timeline.Overview.ContentWidget.Scale);
						var maxScrollOrigin = Vector2.Max(Vector2.Zero, timeline.Grid.ContentSize - timeline.Grid.Size);
						timeline.ScrollOrigin = Vector2.Clamp(scrollOrigin + scrollDelta, Vector2.Zero, maxScrollOrigin);
						yield return null;
					}
					input.ReleaseMouse();
				}
				yield return null;
			}
		}
	}
}

