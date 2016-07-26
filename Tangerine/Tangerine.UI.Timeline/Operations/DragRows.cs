using System;
using System.Linq;
using System.Collections.Generic;
using Lime;
using Tangerine.Core;
using Tangerine.UI.Timeline.Components;

namespace Tangerine.UI.Timeline.Operations
{
	public static class DragRows
	{
		public static void Perform(int destination)
		{
			var timeline = Timeline.Instance;
			var nodesToDrag = timeline.SelectedRows.Select(i => i.Components.Get<NodeRow>()?.Node).Where(i => i != null).ToList();
			var rowInsertBefore = timeline.Rows.FirstOrDefault(
				row => !timeline.SelectedRows.Contains(row) && row.Index >= destination && row.Components.Has<NodeRow>());
			var nodeInsertBefore = rowInsertBefore?.Components.Get<NodeRow>().Node;
			foreach (var node in nodesToDrag) {
				Core.Operations.UnlinkNode.Perform(node);
			}
			var container = timeline.Container;
			var insertionIndex = nodeInsertBefore == null ? container.Nodes.Count : container.Nodes.IndexOf(nodeInsertBefore);
			foreach (var node in nodesToDrag) {
				Core.Operations.InsertNode.Perform(container, insertionIndex++, node);
			}
		}
	}
}