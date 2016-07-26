using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public static class SetCurrentContainer
	{
		public static void Perform(Node container)
		{
			var timeline = Timeline.Instance;
			var prevContainer = Timeline.Instance.Container;
			DelegateOperation.Perform(
				() => {
					timeline.Container = container;
					timeline.EnsureColumnVisible(Document.Current.AnimationFrame);
				},
				() => {
					timeline.Container = prevContainer;
					timeline.EnsureColumnVisible(Document.Current.AnimationFrame);
				}
			);
			ClearRowSelection.Perform();
			if (container.Nodes.Count > 0) {
				var r = timeline.GetCachedRow(container.Nodes[0].EditorState().Uid);
				SelectRow.Perform(r);
			}
		}
	}
}