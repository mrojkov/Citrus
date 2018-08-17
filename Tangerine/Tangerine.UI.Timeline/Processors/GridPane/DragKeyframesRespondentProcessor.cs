using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.Core.Operations;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	public class DragKeyframesRespondentProcessor : ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			var g = Timeline.Instance.Globals;
			while (true) {
				var r = g.Get<DragKeyframesRequest>();
				if (r != null) {
					Document.Current.History.DoTransaction(() => {
						Operations.DragKeyframes.Perform(r.Offset, r.RemoveOriginals);
						Operations.ShiftGridSelection.Perform(r.Offset);
						g.Remove<DragKeyframesRequest>();
					});
				}
				yield return null;
			}
		}
	}
}
