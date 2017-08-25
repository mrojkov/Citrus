using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.Core.Components;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline
{
	class HasKeyframeRespondentProcessor : Core.ITaskProvider
	{
		public IEnumerator<object> Task()
		{
			var g = Timeline.Instance.Globals;
			while (true) {
				var r = g.Get<HasKeyframeRequest>();
				if (r != null) {
					r.Result = HasKeyframeOnCell(r.Cell);
				}
				yield return null;
			}
		}

		bool HasKeyframeOnCell(IntVector2 cell)
		{
			var row = Document.Current.Rows[cell.Y];
			var nodeData = row.Components.Get<NodeRow>();
			if (nodeData != null) {
				var hasKey = nodeData.Node.Animators.Any(i => i.Keys.Any(k => k.Frame == cell.X));
				return hasKey;
			}
			var pr = row.Components.Get<PropertyRow>();
			return pr != null && pr.Animator.Keys.Any(k => k.Frame == cell.X);
		}
	}
}