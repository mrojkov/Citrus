using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	class ClampScrollOriginProcessor : IProcessor
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> MainLoop()
		{
			var doc = Document.Current;
			while (true) {
				var maxScrollOrigin = new Vector2(float.MaxValue, Math.Max(0, timeline.Grid.ContentSize.Y - timeline.Grid.Size.Y));
				timeline.ScrollOrigin = Vector2.Clamp(timeline.ScrollOrigin, Vector2.Zero, maxScrollOrigin);
				yield return null;
			}
		}
	}
}