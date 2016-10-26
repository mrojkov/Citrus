using System;
using System.Linq;
using Lime;
using Tangerine.Core;
using System.Collections.Generic;

namespace Tangerine.UI.Timeline
{
	class ClampScrollPosProcessor : ITaskProvider
	{
		Timeline timeline => Timeline.Instance;

		public IEnumerator<object> Task()
		{
			var doc = Document.Current;
			while (true) {
				var maxScrollPos = new Vector2(float.MaxValue, Math.Max(0, timeline.Grid.ContentSize.Y - timeline.Grid.Size.Y));
				timeline.ScrollPos = Vector2.Clamp(timeline.ScrollPos, Vector2.Zero, maxScrollPos);
				yield return null;
			}
		}
	}
}