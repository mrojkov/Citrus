using System;
using System.Collections.Generic;
using System.Linq;
using Lime;
using Tangerine.Core;

namespace Tangerine.UI.Timeline.Operations
{
	public class SetCurrentContainer : InteractiveOperation
	{
		readonly Node container;

		public SetCurrentContainer(Node node)
		{
			this.container = node;
		}

		public override void Do()
		{
			var prevContainer = Timeline.Instance.Container;
			Timeline.Instance.Container = container;
			AddUndoAction(() => {
				Timeline.Instance.Container = prevContainer;
				prevContainer = null;
			});
			Execute(new ClearRowSelection());
			if (container.Nodes.Count > 0) {
				var r = Timeline.Instance.GetCachedRow(container.Nodes[0].EditorState().Uid);
				Execute(new SelectRow(r));
			}
		}
	}
}